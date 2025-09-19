using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Publisher.Application.Models;
using Publisher.Domain;
using Publisher.Infrastructure;
using System.Linq;

namespace API2.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ArticlesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    public ArticlesController(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ArticleDto>>> GetAll()
    {
        var articles = await _db.Articles.Include(a => a.Opinions).ToListAsync();
        return Ok(_mapper.Map<List<ArticleDto>>(articles));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ArticleDto>> GetById(int id)
    {
        var article = await _db.Articles.Include(a => a.Opinions).FirstOrDefaultAsync(a => a.Id == id);
        if (article == null) return NotFound();
        return Ok(_mapper.Map<ArticleDto>(article));
    }

    [HttpPost]
    public async Task<ActionResult<ArticleDto>> Create(ArticleDto dto)
    {
        var article = _mapper.Map<Article>(dto);
        
        // Guardar automáticamente el usuario logueado en CreatedBy
        var currentUser = User.Identity?.Name;
        if (!string.IsNullOrEmpty(currentUser))
        {
            article.CreatedBy = currentUser;
        }
        
        _db.Articles.Add(article);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = article.Id }, _mapper.Map<ArticleDto>(article));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ArticleDto dto)
    {
        var article = await _db.Articles.Include(a => a.Opinions).FirstOrDefaultAsync(a => a.Id == id);
        if (article == null) return NotFound();
                
        // Verificar autorización: solo el creador (CreatedBy) o admin pueden  editar
        var currentUser = User.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value
                     ?? User.Claims.FirstOrDefault(c => c.Type == "name")?.Value
                     ?? User.Claims.FirstOrDefault(c => c.Type.EndsWith("/name"))?.Value;
        var userRoles = User.Claims.Where(c => c.Type.EndsWith("role")).Select(c => c.Value).ToList();
        
        if (currentUser != article.CreatedBy && !userRoles.Contains("Admin"))
        {
            return StatusCode(403, "Solo el autor del artículo o un administrador pueden editarlo.");
        }
        
        // Preservar el CreatedBy original (no debe cambiar en edición)
        var originalCreatedBy = article.CreatedBy;
        
        _mapper.Map(dto, article);
        
        // Restaurar el CreatedBy original
        article.CreatedBy = originalCreatedBy;
        
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var article = await _db.Articles.FindAsync(id);
        if (article == null) return NotFound();
        
        // Verificar autorización: solo el creador (CreatedBy) o admin pueden eliminar
        var currentUser = User.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value
                ?? User.Claims.FirstOrDefault(c => c.Type == "name")?.Value
                ?? User.Claims.FirstOrDefault(c => c.Type.EndsWith("/name"))?.Value;
        var userRoles = User.Claims.Where(c => c.Type.EndsWith("role")).Select(c => c.Value).ToList();
        
        if (currentUser != article.CreatedBy && !userRoles.Contains("Admin"))
        {
            return StatusCode(403, $"Solo el autor del artículo o un administrador pueden eliminarlo. Usuario actual: '{currentUser}', Creador: '{article.CreatedBy}', Roles: [{string.Join(", ", userRoles)}]");
        }
        
        // RECARGAR EL ARTÍCULO DESDE LA BASE DE DATOS PARA EVITAR PROBLEMAS DE ESTADO
        var freshArticle = await _db.Articles.FindAsync(id);
        if (freshArticle == null)
        {
            return NotFound();
        }        
        
        _db.Articles.Remove(freshArticle);

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id}/comments")]
    public async Task<ActionResult<IEnumerable<OpinionDto>>> GetComments(int id)
    {
        var article = await _db.Articles.FindAsync(id);
        if (article == null) return NotFound();
        
        var opinions = await _db.Opinions
            .Where(o => o.ArticleId == id)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();
        
        return Ok(_mapper.Map<List<OpinionDto>>(opinions));
    }

    [HttpPost("{id}/comments")]
    public async Task<ActionResult<OpinionDto>> AddComment(int id, OpinionDto dto)
    {
        
        var article = await _db.Articles.FindAsync(id);
        if (article == null) 
        {
            return NotFound();
        }
        
        var opinion = _mapper.Map<Opinion>(dto);
        opinion.ArticleId = id;
        opinion.CreatedAt = DateTime.Now; // Establecer fecha de creación automáticamente
        
        _db.Opinions.Add(opinion);
        await _db.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetById), new { id = article.Id }, _mapper.Map<OpinionDto>(opinion));
    }
}

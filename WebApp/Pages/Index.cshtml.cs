using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

using WebApp.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ArticleService _articleService;
    private const int PageSize = 3;

    public List<ArticleDto> Articles { get; set; } = new();
    public List<ArticleDto> AllArticles { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public IndexModel(ILogger<IndexModel> logger, ArticleService articleService)
    {
        _logger = logger;
        _articleService = articleService;
    }

    public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
    {
        // Verificar autenticación antes de mostrar cualquier contenido
        var jwt = HttpContext.Session.GetString("JWToken");
        if (string.IsNullOrEmpty(jwt))
        {
            return RedirectToPage("/Account/Login");
        }
        
        CurrentPage = pageNumber;
        _logger.LogInformation($"Página solicitada: {pageNumber}, Página actual: {CurrentPage}");
        
        AllArticles = await _articleService.GetArticlesAsync(jwt);
        
        // Debug: Mostrar fechas antes del ordenamiento
        _logger.LogInformation("Fechas antes del ordenamiento:");
        foreach (var article in AllArticles)
        {
            _logger.LogInformation($"Artículo: {article.Title} - Fecha: {article.Date}");
        }
        
        // Ordenar por fecha de publicación descendente (más recientes primero)
        AllArticles = AllArticles.OrderByDescending(a => a.Date).ToList();
        
        // Debug: Mostrar fechas después del ordenamiento
        _logger.LogInformation("Fechas después del ordenamiento:");
        foreach (var article in AllArticles)
        {
            _logger.LogInformation($"Artículo: {article.Title} - Fecha: {article.Date}");
        }
        
        // Calcular paginación
        TotalPages = (int)Math.Ceiling((double)AllArticles.Count / PageSize);
        _logger.LogInformation($"Total artículos: {AllArticles.Count}, Total páginas: {TotalPages}");
        
        // Obtener artículos para la página actual
        Articles = AllArticles
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
        
        _logger.LogInformation($"Mostrando {Articles.Count} artículos en página {CurrentPage}");
        
        return Page();
    }
}

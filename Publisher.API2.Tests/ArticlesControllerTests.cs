using Xunit;
using Moq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Publisher.Application.Models;
using Publisher.Domain;
using Publisher.Infrastructure;
using API2.Controllers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Publisher.API2.Tests;

public class ArticlesControllerTests
{
    private IMapper GetMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<Publisher.Application.MappingProfile>());
        return config.CreateMapper();
    }

    private AppDbContext GetDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAll_ReturnsListOfArticles()
    {
        using var db = GetDb("GetAllDb");
        db.Articles.Add(new Article { Title = "Test", Summary = "Sum", Author = "Author", Date = System.DateTime.Now });
        db.SaveChanges();
        var controller = new ArticlesController(db, GetMapper());
        var result = await controller.GetAll();
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var articles = Assert.IsType<List<ArticleDto>>(okResult.Value);
        Assert.Single(articles);
        Assert.Equal("Test", articles[0].Title);
    }

    [Fact]
    public async Task GetById_ReturnsArticle()
    {
        using var db = GetDb("GetByIdDb");
        var art = new Article { Title = "Test2", Summary = "Sum2", Author = "Author2", Date = System.DateTime.Now };
        db.Articles.Add(art); db.SaveChanges();
        var controller = new ArticlesController(db, GetMapper());
        var result = await controller.GetById(art.Id);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ArticleDto>(okResult.Value);
        Assert.Equal("Test2", dto.Title);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound()
    {
        using var db = GetDb("GetByIdNotFoundDb");
        var controller = new ArticlesController(db, GetMapper());
        var result = await controller.GetById(999);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_AddsArticle()
    {
        using var db = GetDb("CreateDb");
        var controller = new ArticlesController(db, GetMapper());
        var dto = new ArticleDto { Title = "Nuevo", Summary = "Sum", Author = "Yo", Date = System.DateTime.Now };
        var result = await controller.Create(dto);
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdDto = Assert.IsType<ArticleDto>(created.Value);
        Assert.Equal("Nuevo", createdDto.Title);
    }

    [Fact]
    public async Task Update_UpdatesArticle()
    {
        using var db = GetDb("UpdateDb");
        var art = new Article { Title = "Viejo", Summary = "Sum", Author = "Yo", Date = System.DateTime.Now };
        db.Articles.Add(art); db.SaveChanges();
        var controller = new ArticlesController(db, GetMapper());
        var dto = new ArticleDto { Id = art.Id, Title = "Actualizado", Summary = "Sum", Author = "Yo", Date = art.Date };
        var result = await controller.Update(art.Id, dto);
        Assert.IsType<NoContentResult>(result);
        Assert.Equal("Actualizado", db.Articles.Find(art.Id)?.Title);
    }

    [Fact]
    public async Task Update_ReturnsNotFound()
    {
        using var db = GetDb("UpdateNotFoundDb");
        var controller = new ArticlesController(db, GetMapper());
        var dto = new ArticleDto { Id = 404, Title = "Nada", Summary = "", Author = "", Date = System.DateTime.Now };
        var result = await controller.Update(404, dto);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_RemovesArticle()
    {
        using var db = GetDb("DeleteDb");
        var art = new Article { Title = "Del", Summary = "Sum", Author = "Yo", Date = System.DateTime.Now };
        db.Articles.Add(art); db.SaveChanges();
        var controller = new ArticlesController(db, GetMapper());
        var result = await controller.Delete(art.Id);
        Assert.IsType<NoContentResult>(result);
        Assert.Empty(db.Articles);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound()
    {
        using var db = GetDb("DeleteNotFoundDb");
        var controller = new ArticlesController(db, GetMapper());
        var result = await controller.Delete(123);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddComment_AddsOpinion()
    {
        using var db = GetDb("AddCommentDb");
        var art = new Article { Title = "Com", Summary = "Sum", Author = "Yo", Date = System.DateTime.Now };
        db.Articles.Add(art); db.SaveChanges();
        var controller = new ArticlesController(db, GetMapper());
        var dto = new OpinionDto { Comments = "Buen artículo" };
        var result = await controller.AddComment(art.Id, dto);
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var opDto = Assert.IsType<OpinionDto>(created.Value);
        Assert.Equal("Buen artículo", opDto.Comments);
    }

    [Fact]
    public async Task AddComment_ReturnsNotFound()
    {
        using var db = GetDb("AddCommentNotFoundDb");
        var controller = new ArticlesController(db, GetMapper());
        var dto = new OpinionDto { Comments = "Nada" };
        var result = await controller.AddComment(404, dto);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_InvalidModel_ReturnsError()
    {
        using var db = GetDb("CreateInvalidDb");
        var controller = new ArticlesController(db, GetMapper());
        var dto = new ArticleDto { Title = null, Summary = "Sum", Author = "Yo", Date = System.DateTime.Now };
        // Simular validación manual (ya que ModelState no se valida automáticamente en pruebas)
        controller.ModelState.AddModelError("Title", "Required");
        var result = await controller.Create(dto);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}

    [Fact]
    public async Task GetAll_ReturnsListOfArticles()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        using var db = new AppDbContext(options);
        db.Articles.Add(new Article { Title = "Test", Summary = "Sum", Author = "Author", Date = System.DateTime.Now });
        db.SaveChanges();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<Publisher.Application.MappingProfile>());
        var mapper = config.CreateMapper();
        var controller = new ArticlesController(db, mapper);

        // Act
        var result = await controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var articles = Assert.IsType<List<ArticleDto>>(okResult.Value);
        Assert.Single(articles);
        Assert.Equal("Test", articles[0].Title);
    }
}

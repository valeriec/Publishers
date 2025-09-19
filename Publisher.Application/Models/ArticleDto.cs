namespace Publisher.Application.Models;

public class ArticleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string CreatedBy { get; set; } = string.Empty; // Usuario logueado que creó el artículo
    public List<OpinionDto> Opinions { get; set; } = new();
}

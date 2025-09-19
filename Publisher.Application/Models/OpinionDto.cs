namespace Publisher.Application.Models;

public class OpinionDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

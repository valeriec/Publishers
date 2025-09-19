using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Publisher.Domain;

public class Article
{
    [Key]
[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string CreatedBy { get; set; } = string.Empty; // Usuario logueado que creó el artículo
    public ICollection<Opinion> Opinions { get; set; } = new List<Opinion>();
}

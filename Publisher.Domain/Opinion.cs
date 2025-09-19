using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Publisher.Domain;

public class Opinion
{
    [Key]
[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
public int Id { get; set; }
    public int ArticleId { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Article? Article { get; set; }
}

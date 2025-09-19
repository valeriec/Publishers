using Microsoft.EntityFrameworkCore;
using Publisher.Domain;

namespace Publisher.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Opinion> Opinions => Set<Opinion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Article>()
            .HasMany(a => a.Opinions)
            .WithOne(o => o.Article)
            .HasForeignKey(o => o.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

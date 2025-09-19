using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Services;
using System.Threading.Tasks;

namespace WebApp.Pages.Articles
{
    public class EditModel : PageModel
    {
        private readonly ArticleService _articleService;
        public string? ErrorMessage { get; set; }
        [BindProperty]
        public ArticleDto Article { get; set; } = new();

        public EditModel(ArticleService articleService)
        {
            _articleService = articleService;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Obtener JWT de sesión
            var jwt = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(jwt))
            {
                return RedirectToPage("/Account/Login");
            }
            
            var article = await _articleService.GetArticleByIdAsync(id, jwt);
            if (article == null)
                return NotFound();
            Article = article;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Datos inválidos.";
                return Page();
            }
            
            // Obtener el artículo original para preservar la fecha de publicación
            var jwt = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(jwt))
            {
                ErrorMessage = "Debe iniciar sesión para editar artículos.";
                return Page();
            }
            
            var originalArticle = await _articleService.GetArticleByIdAsync(id, jwt);
            if (originalArticle != null)
            {
                // Preservar la fecha de publicación original
                Article.Date = originalArticle.Date;
            }
            
            var success = await _articleService.UpdateArticleAsync(id, Article, jwt);
            if (success)
                return RedirectToPage("Manage");
            ErrorMessage = "Error al actualizar el artículo.";
            return Page();
        }
    }
}

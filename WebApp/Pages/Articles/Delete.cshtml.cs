using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Services;
using System.Threading.Tasks;

namespace WebApp.Pages.Articles
{
    public class DeleteModel : PageModel
    {
        private readonly ArticleService _articleService;
        public string? ErrorMessage { get; set; }
        public ArticleDto? Article { get; set; }

        public DeleteModel(ArticleService articleService)
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
            
            Article = await _articleService.GetArticleByIdAsync(id, jwt);
            if (Article == null)
                return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var jwt = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(jwt))
            {
                ErrorMessage = "Debe iniciar sesión para eliminar artículos.";
                Article = await _articleService.GetArticleByIdAsync(id);
                return Page();
            }
            var (success, errorMessage) = await _articleService.DeleteArticleAsync(id, jwt);
            if (success)
                return RedirectToPage("Manage");
            
            // Mostrar el mensaje específico de error del backend
            ErrorMessage = !string.IsNullOrEmpty(errorMessage) 
                ? errorMessage 
                : "Error al eliminar el artículo.";
            
            Article = await _articleService.GetArticleByIdAsync(id);
            return Page();
        }
    }
}

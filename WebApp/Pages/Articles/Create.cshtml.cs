using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Services;
using System.Threading.Tasks;

namespace WebApp.Pages.Articles
{
    public class CreateModel : PageModel
    {
        private readonly ArticleService _articleService;
        private string CurrentUser { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        [BindProperty]
        public ArticleDto Article { get; set; } = new();

        public CreateModel(ArticleService articleService)
        {
            _articleService = articleService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Datos inválidos.";
                return Page();
            }
            
            // Asignar fecha de publicación automáticamente
            Article.Date = DateTime.Now;
            CurrentUser = HttpContext.Session.GetString("UserName") ?? string.Empty;
            Article.CreatedBy = CurrentUser;
            
            // Obtener JWT de sesión
            var jwt = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(jwt))
            {
                ErrorMessage = "Debe iniciar sesión para crear artículos.";
                return Page();
            }
            var success = await _articleService.CreateArticleAsync(Article, jwt);
            if (success)
                return RedirectToPage("Manage");
            ErrorMessage = "Error al crear el artículo.";
            return Page();
        }
    }
}

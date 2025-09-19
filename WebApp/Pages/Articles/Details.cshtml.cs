using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Articles
{
    using WebApp.Services;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using System;

    public class DetailsModel : PageModel
    {
        private readonly ArticleService _articleService;
        public ArticleDto? Article { get; set; }
        public List<CommentDto> Comments { get; set; } = new();

        public DetailsModel(ArticleService articleService)
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
            Comments = await _articleService.GetCommentsAsync(id, jwt);
            
            // Ordenar comentarios por fecha descendente (más recientes primero)
            Comments = Comments.OrderByDescending(c => DateTime.ParseExact(c.CreatedAt, "dd/MM/yyyy HH:mm", null)).ToList();
            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id, string content)
        {
            // Obtener JWT de sesión
            var jwt = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(jwt))
            {
                return RedirectToPage("/Account/Login");
            }

            // Obtener el usuario logueado de la sesión
            var loggedInUser = HttpContext.Session.GetString("UserName") ?? "Usuario Anónimo";

            // Crear el comentario
            var comment = new CommentDto
            {
                Content = content,
                Author = loggedInUser,
                CreatedAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
            };

            // Intentar agregar el comentario
            var success = await _articleService.AddCommentAsync(id, comment, jwt);
            
            // Recargar la página para mostrar el nuevo comentario
            return RedirectToPage(new { id = id });
        }
    }
}

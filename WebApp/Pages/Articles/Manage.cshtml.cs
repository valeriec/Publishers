using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using WebApp.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace WebApp.Pages.Articles
{
    public class ManageModel : PageModel
    {
        private readonly ArticleService _articleService;
        public List<ArticleDto> Articles { get; set; } = new();
        public string CurrentUser { get; set; } = string.Empty;
        public List<string> UserRoles { get; set; } = new();

        public ManageModel(ArticleService articleService)
        {
            _articleService = articleService;
        }

        public async Task OnGetAsync()
        {
            // LOGGING DETALLADO PARA DIAGNÓSTICO DE ROLES
            Console.WriteLine($"[MANAGE DEBUG] ===== DIAGNÓSTICO DE ROLES =====");
            Console.WriteLine($"[MANAGE DEBUG] Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            
            // Obtener JWT de sesión
            var jwt = HttpContext.Session.GetString("JWToken");
            Console.WriteLine($"[MANAGE DEBUG] JWT presente: {(!string.IsNullOrEmpty(jwt) ? "SÍ" : "NO")}");
            
            Articles = await _articleService.GetArticlesAsync(jwt);
            
            // Obtener información del usuario actual
            CurrentUser = HttpContext.Session.GetString("UserName") ?? string.Empty;
            Console.WriteLine($"[MANAGE DEBUG] CurrentUser: '{CurrentUser}'");
            
            var userRolesString = HttpContext.Session.GetString("UserRoles") ?? string.Empty;
            Console.WriteLine($"[MANAGE DEBUG] UserRoles string de sesión: '{userRolesString}'");
            Console.WriteLine($"[MANAGE DEBUG] UserRoles string IsNullOrEmpty: {string.IsNullOrEmpty(userRolesString)}");
            
            UserRoles = string.IsNullOrEmpty(userRolesString) 
                ? new List<string>() 
                : userRolesString.Split(',').ToList();
                
            Console.WriteLine($"[MANAGE DEBUG] UserRoles final: [{string.Join(", ", UserRoles)}]");
            Console.WriteLine($"[MANAGE DEBUG] UserRoles count: {UserRoles.Count}");
            
            // Si JWT está presente pero roles están vacíos, intentar extraer del JWT
            if (!string.IsNullOrEmpty(jwt) && UserRoles.Count == 0)
            {
                Console.WriteLine($"[MANAGE DEBUG] JWT presente pero roles vacíos, intentando extraer del JWT...");
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadJwtToken(jwt);
                    var rolesFromJwt = jsonToken.Claims
                        .Where(c => c.Type == "role")
                        .Select(c => c.Value)
                        .ToList();
                        
                    Console.WriteLine($"[MANAGE DEBUG] Roles extraídos directamente del JWT: [{string.Join(", ", rolesFromJwt)}]");
                    
                    if (rolesFromJwt.Count > 0)
                    {
                        UserRoles = rolesFromJwt;
                        var rolesString = string.Join(",", rolesFromJwt);
                        HttpContext.Session.SetString("UserRoles", rolesString);
                        Console.WriteLine($"[MANAGE DEBUG] Roles actualizados en sesión: '{rolesString}'");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MANAGE DEBUG] Error al extraer roles del JWT: {ex.Message}");
                }
            }
            
            Console.WriteLine($"[MANAGE DEBUG] Roles finales para autorización: [{string.Join(", ", UserRoles)}]");
            
            // Ordenar por fecha de publicación descendente (más recientes primero)
            Articles = Articles.OrderByDescending(a => a.Date).ToList();
        }
        
        /// <summary>
        /// Determina si el usuario actual puede editar o eliminar el artículo especificado
        /// </summary>
        /// <param name="article">El artículo a validar</param>
        /// <returns>True si el usuario puede editar/eliminar, False en caso contrario</returns>
        public bool CanEditOrDelete(ArticleDto article)
        {
            // El usuario puede editar/eliminar si:
            // 1. Es el creador del artículo (CreatedBy coincide con CurrentUser)
            // 2. O tiene rol de Admin
            return CurrentUser == article.CreatedBy || UserRoles.Contains("Admin");
        }
    }
}

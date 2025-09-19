using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebApp.Pages.Account
{
    using WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class LoginModel : PageModel
{
    private readonly AuthService _authService;
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    [BindProperty]
    public string Username { get; set; } = string.Empty;
    [BindProperty]
    public string Password { get; set; } = string.Empty;
    
    // Propiedades para el registro
    [BindProperty]
    public string RegisterUsername { get; set; } = string.Empty;
    [BindProperty]
    public string RegisterPassword { get; set; } = string.Empty;
    [BindProperty]
    public string ConfirmPassword { get; set; } = string.Empty;

    public LoginModel(AuthService authService)
    {
        _authService = authService;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        Console.WriteLine($"[LOGIN DEBUG] OnPostAsync iniciado. Username: {Username}, Password: {(string.IsNullOrEmpty(Password) ? "vacío" : "presente")}");
        
        // Limpiar las validaciones de registro para el login
        ModelState.Remove("RegisterUsername");
        ModelState.Remove("RegisterPassword");
        ModelState.Remove("ConfirmPassword");
        
        if (!ModelState.IsValid) 
        {
            Console.WriteLine("[LOGIN DEBUG] ModelState no válido. Errores:");
            foreach (var error in ModelState)
            {
                Console.WriteLine($"[LOGIN DEBUG] Campo: {error.Key}, Errores: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
            }
            Console.WriteLine($"[LOGIN DEBUG] Username recibido: '{Username}', Password presente: {!string.IsNullOrEmpty(Password)}");
            return Page();
        }
        
        Console.WriteLine("[LOGIN DEBUG] Llamando a AuthService.LoginAsync...");
        var (success, token, errorMessage) = await _authService.LoginAsync(Username, Password);
        
        Console.WriteLine($"[LOGIN DEBUG] Respuesta de AuthService - Success: {success}, Token: {(token != null ? "presente" : "null")}, Error: {errorMessage}");
        
        if (success && token != null)
        {
            Console.WriteLine("[LOGIN DEBUG] Login exitoso, guardando en sesión y redirigiendo");
            HttpContext.Session.SetString("JWToken", token);
            HttpContext.Session.SetString("UserName", Username);
            
            // Extraer roles del JWT token
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);
                var roles = jsonToken.Claims
                    .Where(c => c.Type == "role")
                    .Select(c => c.Value)
                    .ToList();
                
                var rolesString = string.Join(",", roles);
                HttpContext.Session.SetString("UserRoles", rolesString);
                
                Console.WriteLine($"[LOGIN DEBUG] Roles extraídos del JWT: [{rolesString}]");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOGIN DEBUG] Error al extraer roles del JWT: {ex.Message}");
                HttpContext.Session.SetString("UserRoles", ""); // Roles vacíos en caso de error
            }
            
            return RedirectToPage("/Index");
        }
        
        ErrorMessage = errorMessage ?? "Usuario o contraseña incorrectos. Verifica tus credenciales e intenta nuevamente.";
        Console.WriteLine($"[LOGIN DEBUG] Login fallido, mostrando error: {ErrorMessage}");
        return Page();
    }

    public async Task<IActionResult> OnPostRegisterAsync()
    {
        Console.WriteLine($"[REGISTER DEBUG] OnPostRegisterAsync iniciado. Username: {RegisterUsername}");
        
        // Validar que las contraseñas coincidan
        if (RegisterPassword != ConfirmPassword)
        {
            Console.WriteLine("[REGISTER DEBUG] Las contraseñas no coinciden");
            ErrorMessage = "Las contraseñas no coinciden.";
            return Page();
        }

        // Validar campos requeridos
        if (string.IsNullOrEmpty(RegisterUsername) || string.IsNullOrEmpty(RegisterPassword))
        {
            Console.WriteLine("[REGISTER DEBUG] Campos requeridos faltantes");
            ErrorMessage = "Todos los campos son requeridos.";
            return Page();
        }
        
        // Validar formato de nombre de usuario
        if (!IsValidUsername(RegisterUsername))
        {
            Console.WriteLine($"[REGISTER DEBUG] Nombre de usuario inválido: {RegisterUsername}");
            ErrorMessage = "El nombre de usuario debe tener al menos 3 caracteres y solo puede contener letras, números y guiones bajos.";
            return Page();
        }

        Console.WriteLine("[REGISTER DEBUG] Llamando a AuthService.RegisterAsync...");
        // Intentar registrar el usuario
        var (success, message) = await _authService.RegisterAsync(RegisterUsername, RegisterPassword);
        
        Console.WriteLine($"[REGISTER DEBUG] Respuesta de AuthService - Success: {success}, Message: {message}");
        
        if (success)
        {
            Console.WriteLine("[REGISTER DEBUG] Registro exitoso, mostrando mensaje de éxito");
            SuccessMessage = "Usuario registrado exitosamente. Ahora puedes iniciar sesión.";
            // Limpiar los campos del registro
            RegisterUsername = string.Empty;
            RegisterPassword = string.Empty;
            ConfirmPassword = string.Empty;
            return Page();
        }
        else
        {
            Console.WriteLine($"[REGISTER DEBUG] Registro falló, mostrando error: {message}");
            ErrorMessage = $"Error al registrar el usuario: {message}";
            return Page();
        }
    }
    
    private bool IsValidUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;
            
        // El nombre de usuario debe tener al menos 3 caracteres
        if (username.Length < 3)
            return false;
            
        try
        {
            // Solo letras, números y guiones bajos
            var usernameRegex = new Regex(@"^[a-zA-Z0-9_]+$");
            return usernameRegex.IsMatch(username);
        }
        catch
        {
            return false;
        }
    }
}

}

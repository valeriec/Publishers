using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Account
{
    using WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class RegisterModel : PageModel
{
    private readonly AuthService _authService;
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    [BindProperty]
    public string Username { get; set; } = string.Empty;
    [BindProperty]
    public string Password { get; set; } = string.Empty;
    [BindProperty]
    public string ConfirmPassword { get; set; } = string.Empty;

    public RegisterModel(AuthService authService)
    {
        _authService = authService;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid || Password != ConfirmPassword)
        {
            ErrorMessage = "Las contraseñas no coinciden.";
            return Page();
        }
        var (success, message) = await _authService.RegisterAsync(Username, Password);
        if (success)
        {
            SuccessMessage = "Registro exitoso. Ahora puedes iniciar sesión.";
            return RedirectToPage("/Account/Login");
        }
        ErrorMessage = $"No se pudo registrar el usuario: {message}";
        return Page();
    }
}

}

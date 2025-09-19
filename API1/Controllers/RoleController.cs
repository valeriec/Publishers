using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using API1.Data;

namespace API1.Controllers
{
    [ApiController]
    [Route("api/roles")]
    [Authorize(Roles = "Admin")] // Solo Admin puede gestionar roles
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // Crear un nuevo rol
        [HttpPost("create")]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest("El nombre del rol es requerido.");
            var exists = await _roleManager.RoleExistsAsync(roleName);
            if (exists)
                return BadRequest("El rol ya existe.");
            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            return Ok($"Rol '{roleName}' creado correctamente.");
        }

        // Asignar un rol a un usuario
        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null)
                return NotFound("Usuario no encontrado.");
            var exists = await _roleManager.RoleExistsAsync(dto.RoleName);
            if (!exists)
                return NotFound("Rol no existe.");
            var result = await _userManager.AddToRoleAsync(user, dto.RoleName);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            return Ok($"Rol '{dto.RoleName}' asignado a '{dto.UserName}'.");
        }
    }

    public class AssignRoleDto
    {
        public string UserName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
}

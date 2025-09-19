using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using API1.Data;
using API1.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace API1.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IMapper mapper, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            Console.WriteLine("[API1 DEBUG] ===== MÉTODO REGISTER INICIADO =====");
            Console.WriteLine($"[API1 DEBUG] Timestamp: {DateTime.Now}");
            try
            {
                Console.WriteLine($"[API1 DEBUG] DTO recibido: {(dto == null ? "NULL" : "NOT NULL")}");
                if (dto == null)
                {
                    Console.WriteLine("[API1 DEBUG] DTO es null, devolviendo BadRequest");
                    return BadRequest("DTO is null");
                }
                    
                if (string.IsNullOrEmpty(dto.UserName) || string.IsNullOrEmpty(dto.Password))
                    return BadRequest("Username and password are required");

                // Verificar si el usuario ya existe por UserName
                var existingUserByName = await _userManager.FindByNameAsync(dto.UserName);
                if (existingUserByName != null)
                {
                    Console.WriteLine($"[API1 DEBUG] Usuario ya existe por UserName: {dto.UserName}");
                    return BadRequest("El usuario ya existe");
                }
                    
                // Si no se proporciona email, generar uno basado en el username con GUID único
                Console.WriteLine($"[API1 DEBUG] dto.Email recibido: '{dto.Email}'");
                var email = string.IsNullOrEmpty(dto.Email) ? $"{dto.UserName}-{Guid.NewGuid():N}@sistema.local" : dto.Email;
                Console.WriteLine($"[API1 DEBUG] Email generado: '{email}'");
                
                // Verificar si el usuario ya existe por Email (después de generar email automático)
                var existingUserByEmail = await _userManager.FindByEmailAsync(email);
                if (existingUserByEmail != null)
                {
                    Console.WriteLine($"[API1 DEBUG] Usuario ya existe por Email: {email}");
                    return BadRequest("El usuario ya existe (email duplicado)");
                }
                var user = new ApplicationUser { UserName = dto.UserName, Email = email };
                Console.WriteLine($"[API1 DEBUG] Usuario creado con UserName: '{user.UserName}', Email: '{user.Email}'");
                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest($"Error al crear usuario: {errors}");
                }
            
                // Verificar que el rol "User" existe
                var roleManager = HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();
                var roleExists = await roleManager.RoleExistsAsync("User");
                if (!roleExists)
                {
                    return BadRequest("El rol 'User' no existe en el sistema");
                }
                
                // Esperar un momento para asegurar que el usuario esté completamente creado
                await Task.Delay(100);
                
                // Recargar el usuario desde la base de datos
                user = await _userManager.FindByNameAsync(dto.UserName);
                if (user == null)
                {
                    return BadRequest("Error: Usuario creado pero no se puede recuperar");
                }
                
                // Intentar asignar rol "User" usando Identity
                Console.WriteLine($"[API1 DEBUG] Intentando asignar rol 'User' al usuario {user.Id}");
                var roleResult = await _userManager.AddToRoleAsync(user, "User");
                
                if (roleResult.Succeeded)
                {
                    Console.WriteLine($"[API1 DEBUG] Rol asignado exitosamente con Identity");
                }
                else
                {
                    var identityErrors = string.Join(", ", roleResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    Console.WriteLine($"[API1 DEBUG] Falló asignación con Identity: {identityErrors}");
                    
                    // Si falla con Identity, intentar inserción directa en la base de datos
                    Console.WriteLine($"[API1 DEBUG] Intentando inserción SQL directa...");
                    var dbContext = HttpContext.RequestServices.GetRequiredService<Data.ApplicationDbContext>();
                    var userRole = await dbContext.Database.ExecuteSqlRawAsync(
                        "INSERT INTO AspNetUserRoles (UserId, RoleId) SELECT @p0, Id FROM AspNetRoles WHERE Name = 'User'",
                        user.Id);
                    
                    Console.WriteLine($"[API1 DEBUG] Filas afectadas por SQL: {userRole}");
                    
                    if (userRole == 0)
                    {
                        Console.WriteLine($"[API1 DEBUG] ERROR: No se pudo insertar la relación usuario-rol");
                        return BadRequest($"Usuario creado pero falló la asignación del rol (Identity y SQL): {identityErrors}");
                    }
                    else
                    {
                        Console.WriteLine($"[API1 DEBUG] Rol asignado exitosamente con SQL directo");
                    }
                }
                
                // Verificar que el rol se asignó correctamente
                var userRoles = await _userManager.GetRolesAsync(user);
                
                return Ok($"Usuario registrado exitosamente. ID: {user.Id}, Username: {user.UserName}, Roles: [{string.Join(", ", userRoles)}]");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error inesperado: {ex.Message} - {ex.StackTrace}");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // LOGGING DETALLADO PARA DIAGNÓSTICO
            Console.WriteLine($"[API1 DEBUG LOGIN] ===== LOGIN INICIADO =====");
            Console.WriteLine($"[API1 DEBUG LOGIN] Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            Console.WriteLine($"[API1 DEBUG LOGIN] DTO recibido: {(dto == null ? "NULL" : "NOT NULL")}");
            
            if (dto != null)
            {
                Console.WriteLine($"[API1 DEBUG LOGIN] UserName: '{dto.UserName}'");
                Console.WriteLine($"[API1 DEBUG LOGIN] Password: '{(string.IsNullOrEmpty(dto.Password) ? "EMPTY" : "[PRESENTE]")}'");
                Console.WriteLine($"[API1 DEBUG LOGIN] UserName IsNullOrEmpty: {string.IsNullOrEmpty(dto.UserName)}");
            }
            else
            {
                Console.WriteLine($"[API1 DEBUG LOGIN] ERROR: DTO es null");
                return BadRequest("Datos de login no recibidos");
            }
            
            if (string.IsNullOrEmpty(dto.UserName))
            {
                Console.WriteLine($"[API1 DEBUG LOGIN] ERROR: UserName está vacío");
                return BadRequest("Nombre de usuario requerido");
            }
            
            Console.WriteLine($"[API1 DEBUG LOGIN] Buscando usuario en base de datos...");
            var user = await _userManager.FindByNameAsync(dto.UserName);
            
            if (user == null)
            {
                Console.WriteLine($"[API1 DEBUG LOGIN] Usuario '{dto.UserName}' NO encontrado en base de datos");
                return Unauthorized("Usuario o contraseña incorrectos");
            }
            
            Console.WriteLine($"[API1 DEBUG LOGIN] Usuario '{dto.UserName}' encontrado. ID: {user.Id}");
            Console.WriteLine($"[API1 DEBUG LOGIN] Verificando contraseña...");
            
            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            
            if (!result.Succeeded)
            {
                Console.WriteLine($"[API1 DEBUG LOGIN] Contraseña incorrecta para usuario '{dto.UserName}'");
                return Unauthorized("Usuario o contraseña incorrectos");
            }
            
            Console.WriteLine($"[API1 DEBUG LOGIN] Login exitoso para usuario '{dto.UserName}'");

            // Crear claims básicos
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Id),
                new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName, user.UserName!),
                new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, user.Email ?? "")
            };
    
            // Agregar roles del usuario a los claims
            Console.WriteLine($"[API1 DEBUG LOGIN] Obteniendo roles del usuario...");
            var userRoles = await _userManager.GetRolesAsync(user);
            Console.WriteLine($"[API1 DEBUG LOGIN] Roles obtenidos: [{string.Join(", ", userRoles)}]");
            Console.WriteLine($"[API1 DEBUG LOGIN] Cantidad de roles: {userRoles.Count}");
            
            foreach (var role in userRoles)
            {
                Console.WriteLine($"[API1 DEBUG LOGIN] Agregando rol '{role}' al JWT");
                claims.Add(new System.Security.Claims.Claim("role", role));
            }
            
            Console.WriteLine($"[API1 DEBUG LOGIN] Total de claims en JWT: {claims.Count}");
            Console.WriteLine($"[API1 DEBUG LOGIN] Claims de roles en JWT: [{string.Join(", ", claims.Where(c => c.Type == "role").Select(c => c.Value))}]");
            
            // Leer configuración JWT
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiresInMinutes"]));

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var tokenString = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    
    // LOGGING DEL JWT GENERADO PARA DIAGNÓSTICO
    Console.WriteLine($"[API1 DEBUG LOGIN] JWT generado exitosamente");
    Console.WriteLine($"[API1 DEBUG LOGIN] JWT length: {tokenString.Length}");
    
    // Verificar que el JWT contiene los roles
    try
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(tokenString);
        var rolesInToken = jsonToken.Claims
            .Where(c => c.Type == "role")
            .Select(c => c.Value)
            .ToList();
        Console.WriteLine($"[API1 DEBUG LOGIN] Roles en JWT generado: [{string.Join(", ", rolesInToken)}]");
        Console.WriteLine($"[API1 DEBUG LOGIN] Todos los claims en JWT: [{string.Join(", ", jsonToken.Claims.Select(c => $"{c.Type}:{c.Value}"))}]");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[API1 DEBUG LOGIN] Error al leer JWT generado: {ex.Message}");
    }
    
    var userDto = _mapper.Map<UserDto>(user);
    Console.WriteLine($"[API1 DEBUG LOGIN] Devolviendo respuesta exitosa con JWT");
    return Ok(new { token = tokenString, user = userDto });
        }

        [HttpPost("test-role-assignment")]
        public async Task<IActionResult> TestRoleAssignment([FromBody] string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return BadRequest("Usuario no encontrado");
                
                // Verificar roles actuales
                var currentRoles = await _userManager.GetRolesAsync(user);
                
                // Intentar asignar rol User
                var roleResult = await _userManager.AddToRoleAsync(user, "User");
                
                // Verificar roles después de la asignación
                var newRoles = await _userManager.GetRolesAsync(user);
                
                // Verificar directamente en la base de datos
                var dbContext = HttpContext.RequestServices.GetRequiredService<Data.ApplicationDbContext>();
                var dbRoles = await dbContext.Database.SqlQueryRaw<string>(
                    "SELECT r.Name FROM AspNetRoles r INNER JOIN AspNetUserRoles ur ON r.Id = ur.RoleId WHERE ur.UserId = {0}",
                    userId).ToListAsync();
                
                return Ok(new {
                    UserId = userId,
                    UserName = user.UserName,
                    RoleAssignmentSuccess = roleResult.Succeeded,
                    RoleAssignmentErrors = roleResult.Errors.Select(e => $"{e.Code}: {e.Description}"),
                    CurrentRolesBeforeAssignment = currentRoles,
                    CurrentRolesAfterAssignment = newRoles,
                    DirectDatabaseQuery = dbRoles
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
        
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();
            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }
    }
}

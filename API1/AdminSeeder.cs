using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace API1
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<Data.ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            
            // Crear roles por defecto si no existen
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }
            
            string adminEmail = "admin@demo.com";
            string adminUser = "admin";
            string password = "Admin123$";
            // Crear usuario admin si no existe
            var user = await userManager.FindByNameAsync(adminUser);
            if (user == null)
            {
                user = new Data.ApplicationUser { UserName = adminUser, Email = adminEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded) return;
            }
            // Asignar rol Admin si no lo tiene
            if (!await userManager.IsInRoleAsync(user, "Admin"))
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}

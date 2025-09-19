using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Threading.Tasks;
using WebApp.Pages.Account;
using WebApp.Services;
using Xunit;

namespace WebApp.Tests
{
    public class LoginModelTests
    {
        [Fact]
        public async Task OnPostAsync_SetsErrorMessage_WhenLoginFails()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>(null!, "https://localhost:7001");
            authServiceMock.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((false, null, "Usuario o contraseña incorrectos"));
            var httpContext = new DefaultHttpContext();
            var pageModel = new LoginModel(authServiceMock.Object)
            {
                Username = "user",
                Password = "wrongpass",
                PageContext = new PageContext { HttpContext = httpContext }
            };
            // Act
            var result = await pageModel.OnPostAsync();
            // Assert
            Assert.IsType<PageResult>(result);
            Assert.Equal("Usuario o contraseña incorrectos.", pageModel.ErrorMessage);
        }
    }
}

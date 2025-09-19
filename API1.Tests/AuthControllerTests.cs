using Xunit;
using Moq;
using API1.Controllers;
using API1.Models;
using API1.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace API1.Tests
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task Register_ReturnsOk_WhenUserIsCreated()
        {
            // Arrange
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);
            var mapperMock = new Mock<IMapper>();
            var configMock = new Mock<IConfiguration>();

            userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var controller = new AuthController(
                userManagerMock.Object,
                signInManagerMock.Object,
                mapperMock.Object,
                configMock.Object
            );

            var dto = new RegisterDto { UserName = "test", Email = "test@mail.com", Password = "Test123$" };

            // Act
            var result = await controller.Register(dto);

            // Assert
            Assert.IsType<OkResult>(result);
        }
    }
}

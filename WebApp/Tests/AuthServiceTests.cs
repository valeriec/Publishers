using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using WebApp.Services;
using Xunit;

namespace WebApp.Tests
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task LoginAsync_ReturnsToken_WhenSuccess()
        {
            // Arrange
            var expectedToken = "jwt-token";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"{{\"token\":\"{expectedToken}\"}}")
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object);
            var service = new AuthService(httpClient, "https://localhost:7001");

            // Act
            var (success, token, errorMessage) = await service.LoginAsync("user", "pass");

            // Assert
            Assert.True(success);
            Assert.Equal(expectedToken, token);
            Assert.Null(errorMessage);
        }
    }
}

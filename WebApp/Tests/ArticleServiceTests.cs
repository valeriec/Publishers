using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using WebApp.Services;
using Xunit;
using System.Text.Json;

namespace WebApp.Tests
{
    public class ArticleServiceTests
    {
        [Fact]
        public async Task GetArticlesAsync_ReturnsList_WhenSuccess()
        {
            // Arrange
            var articles = new List<ArticleDto>
            {
                new ArticleDto { Id = 1, Title = "Test Article 1", Author = "Author 1", Summary = "Content 1", Date = DateTime.Now },
                new ArticleDto { Id = 2, Title = "Test Article 2", Author = "Author 2", Summary = "Content 2", Date = DateTime.Now }
            };
            var json = JsonSerializer.Serialize(articles);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
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
            var service = new ArticleService(httpClient, "https://localhost:7002");

            // Act
            var result = await service.GetArticlesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("A", result[0].Title);
        }
    }
}

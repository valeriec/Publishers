using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Publisher.Application.Models;

namespace WebApp.Services
{
    public class ArticleDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string CreatedBy { get; set; } = string.Empty; // Usuario logueado que creó el artículo
    }

    public class ArticleService
    {
        private readonly HttpClient _httpClient;
        private readonly string _api2BaseUrl;

        public ArticleService(HttpClient httpClient, string api2BaseUrl)
        {
            _httpClient = httpClient;
            _api2BaseUrl = api2BaseUrl.TrimEnd('/');
        }

        public async Task<List<ArticleDto>> GetArticlesAsync(string? jwtToken = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_api2BaseUrl}/articles");
            if (!string.IsNullOrEmpty(jwtToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var articles = JsonSerializer.Deserialize<List<ArticleDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return articles ?? new List<ArticleDto>();
            }
            return new List<ArticleDto>();
        }

        public async Task<ArticleDto?> GetArticleByIdAsync(int id, string? jwtToken = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_api2BaseUrl}/articles/{id}");
            if (!string.IsNullOrEmpty(jwtToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ArticleDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            return null;
        }

        public async Task<List<CommentDto>> GetCommentsAsync(int articleId, string? jwtToken = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_api2BaseUrl}/articles/{articleId}/comments");
            if (!string.IsNullOrEmpty(jwtToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                // Deserializar como OpinionDto y mapear a CommentDto
                var opinions = JsonSerializer.Deserialize<List<OpinionDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (opinions != null)
                {
                    return opinions.Select(o => new CommentDto
                    {
                        Id = o.Id,
                        Author = o.Author,
                        Content = o.Comments, // Mapear Comments a Content
                        CreatedAt = o.CreatedAt.ToString("dd/MM/yyyy HH:mm") // Convertir DateTime a string
                    }).ToList();
                }
            }
            return new List<CommentDto>();
        }

        public async Task<bool> CreateArticleAsync(ArticleDto article, string jwtToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_api2BaseUrl}/articles");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            request.Content = new StringContent(JsonSerializer.Serialize(article), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateArticleAsync(int id, ArticleDto article, string jwtToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"{_api2BaseUrl}/articles/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            request.Content = new StringContent(JsonSerializer.Serialize(article), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<(bool success, string errorMessage)> DeleteArticleAsync(int id, string jwtToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_api2BaseUrl}/articles/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            var response = await _httpClient.SendAsync(request);
        
            if (response.IsSuccessStatusCode)
            {
                return (true, string.Empty);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return (false, errorContent);
            }
        }

        public async Task<bool> AddCommentAsync(int articleId, CommentDto comment, string jwtToken)
        {
            // Enviar OpinionDto con campos separados para Comments y Author
            var opinionDto = new
            {
                Comments = comment.Content, // Solo el contenido del comentario
                Author = comment.Author     // El autor en campo separado
                // CreatedAt se establece automáticamente en el servidor
            };
            
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_api2BaseUrl}/articles/{articleId}/comments");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            request.Content = new StringContent(JsonSerializer.Serialize(opinionDto), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}

// CommentDto fuera del namespace para evitar conflictos
public class CommentDto
{
    public int Id { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}

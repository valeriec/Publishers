using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebApp.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _api1BaseUrl;

        public AuthService(HttpClient httpClient, string api1BaseUrl)
        {
            _httpClient = httpClient;
            _api1BaseUrl = api1BaseUrl.TrimEnd('/');
        }

        public async Task<(bool Success, string? Token, string? ErrorMessage)> LoginAsync(string username, string password)
        {
            Console.WriteLine($"[AUTHSERVICE DEBUG] LoginAsync iniciado. Username: {username}, API1 URL: {_api1BaseUrl}");
            
            try
            {
                var payload = new { username, password };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                
                Console.WriteLine($"[AUTHSERVICE DEBUG] Enviando petición a: {_api1BaseUrl}/auth/login");
                var response = await _httpClient.PostAsync($"{_api1BaseUrl}/auth/login", content);
                
                Console.WriteLine($"[AUTHSERVICE DEBUG] Respuesta recibida. StatusCode: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[AUTHSERVICE DEBUG] Respuesta JSON: {json}");
                    
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("token", out var tokenProp))
                    {
                        Console.WriteLine("[AUTHSERVICE DEBUG] Token encontrado en respuesta");
                        return (true, tokenProp.GetString(), null);
                    }
                    Console.WriteLine("[AUTHSERVICE DEBUG] Token NO encontrado en respuesta");
                    return (false, null, "Token no encontrado en la respuesta del servidor");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[AUTHSERVICE DEBUG] Error del servidor: {errorContent}");
                    return (false, null, $"Error {(int)response.StatusCode}: {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[AUTHSERVICE DEBUG] Error de conexión: {ex.Message}");
                return (false, null, $"Error de conexión: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AUTHSERVICE DEBUG] Error inesperado: {ex.Message}");
                return (false, null, $"Error inesperado: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> RegisterAsync(string username, string password)
        {
            Console.WriteLine($"[AUTHSERVICE DEBUG] RegisterAsync iniciado. Username: {username}");
            
            try
            {
                // Enviar email vacío para que API1 genere uno automáticamente
                var payload = new { username, email = "", password };
                var payloadJson = JsonSerializer.Serialize(payload);
                Console.WriteLine($"[AUTHSERVICE DEBUG] Payload JSON: {payloadJson}");
                
                var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
                Console.WriteLine($"[AUTHSERVICE DEBUG] Enviando petición a: {_api1BaseUrl}/auth/register");
                
                var response = await _httpClient.PostAsync($"{_api1BaseUrl}/auth/register", content);
                Console.WriteLine($"[AUTHSERVICE DEBUG] Respuesta recibida. StatusCode: {response.StatusCode}");
                
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[AUTHSERVICE DEBUG] Respuesta contenido: {responseContent}");
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("[AUTHSERVICE DEBUG] Registro exitoso");
                    return (true, responseContent);
                }
                else
                {
                    Console.WriteLine($"[AUTHSERVICE DEBUG] Registro falló con código: {response.StatusCode}");
                    return (false, responseContent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AUTHSERVICE DEBUG] Excepción: {ex.Message}");
                return (false, $"Error de conexión: {ex.Message}");
            }
        }
    }
}

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace learning_platform.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5215");
        _httpContextAccessor = httpContextAccessor;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    private async Task AddAuthHeaderAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            var token = context.User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        await AddAuthHeaderAsync();
        var json = JsonSerializer.Serialize(data);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(endpoint, httpContent);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API Error: {response.StatusCode} - {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        return string.IsNullOrEmpty(responseContent) ? default : JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions);
    }
    
    public async Task<bool> PostAsync<TRequest>(string endpoint, TRequest data)
    {
        await AddAuthHeaderAsync();
        var json = JsonSerializer.Serialize(data);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(endpoint, httpContent);
        return response.IsSuccessStatusCode;
    }
}

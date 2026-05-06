using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LearningPlatform.Desktop.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ApiClient(string baseUrl)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public void SetToken(string? token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        await EnsureSuccess(response);
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        var response = await _httpClient.PostAsync(endpoint, BuildJson(data));
        await EnsureSuccess(response);
        var content = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(content) ? default : JsonSerializer.Deserialize<TResponse>(content, _jsonOptions);
    }

    public async Task<bool> PostAsync<TRequest>(string endpoint, TRequest data)
    {
        var response = await _httpClient.PostAsync(endpoint, BuildJson(data));
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> PutAsync<TRequest>(string endpoint, TRequest? data = default)
    {
        HttpContent? content = data is null ? null : BuildJson(data);
        var response = await _httpClient.PutAsync(endpoint, content);
        await EnsureSuccess(response);
        return true;
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        var response = await _httpClient.DeleteAsync(endpoint);
        await EnsureSuccess(response);
        return true;
    }

    private static StringContent BuildJson<T>(T data) =>
        new(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

    private static async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var error = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException($"API Error: {(int)response.StatusCode} - {error}");
    }
}

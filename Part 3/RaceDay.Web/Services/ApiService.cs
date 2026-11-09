using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Services;

public class ApiService
{
    private const string ApiCookieName = "RaceDay.Auth";
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ApiService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiService(HttpClient http, IHttpContextAccessor httpContextAccessor, ILogger<ApiService> logger)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public Task<ApiResult<T>> GetAsync<T>(string path) => SendAsync<T>(HttpMethod.Get, path);

    public Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string path, TRequest body) =>
        SendAsync<TResponse>(HttpMethod.Post, path, body);

    public Task<ApiResult<TResponse>> PutAsync<TRequest, TResponse>(string path, TRequest body) =>
        SendAsync<TResponse>(HttpMethod.Put, path, body);

    public Task<ApiResult<JsonElement>> DeleteAsync(string path) =>
        SendAsync<JsonElement>(HttpMethod.Delete, path);

    public async Task<ApiResult<ApiLoginResponse>> LoginAsync(LoginRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
            return Failure<ApiLoginResponse>(500, "The web session could not be started. Please try again.");

        var response = await SendAsyncForCookieCapture(request);
        if (!response.Result.IsSuccess)
            return response.Result;

        if (response.CookiePair == null)
        {
            context.Session.Clear();
            return Failure<ApiLoginResponse>(500,
                "The API accepted the login but did not return its authentication cookie. Check the API login response and try again.");
        }

        context.Session.SetString("ApiCookie", response.CookiePair);
        return response.Result;
    }

    private async Task<(string? CookiePair, ApiResult<ApiLoginResponse> Result)> SendAsyncForCookieCapture(LoginRequest request)
    {
        try
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, "api/auth/login")
            {
                Content = JsonContent.Create(request, options: JsonOptions)
            };

            using var response = await _http.SendAsync(message);
            var result = await ReadResponseAsync<ApiLoginResponse>(response);
            string? cookiePair = null;

            if (response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
            {
                foreach (var header in setCookieValues)
                {
                    var firstPart = header.Split(';', 2)[0].Trim();
                    if (firstPart.StartsWith(ApiCookieName + "=", StringComparison.OrdinalIgnoreCase))
                    {
                        cookiePair = firstPart;
                        break;
                    }
                }
            }

            return (cookiePair, result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Could not connect to the RaceDay API during login.");
            return (null, Failure<ApiLoginResponse>(503,
                "Could not connect to the RaceDay API. Make sure RaceDayApp_POE is running and ApiSettings:BaseUrl is correct."));
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "The RaceDay API login request timed out.");
            return (null, Failure<ApiLoginResponse>(504, "The API request timed out. Please try again."));
        }
    }

    public async Task<ApiResult<JsonElement>> LogoutAsync()
    {
        var result = await SendAsync<JsonElement>(HttpMethod.Post, "api/auth/logout");
        _httpContextAccessor.HttpContext?.Session.Clear();
        return result;
    }

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string path, object? body = null, bool includeCookie = true)
    {
        try
        {
            using var request = new HttpRequestMessage(method, path);
            if (body != null)
                request.Content = JsonContent.Create(body, body.GetType(), options: JsonOptions);

            if (includeCookie)
            {
                var cookie = _httpContextAccessor.HttpContext?.Session.GetString("ApiCookie");
                if (!string.IsNullOrWhiteSpace(cookie))
                    request.Headers.TryAddWithoutValidation("Cookie", cookie);
            }

            using var response = await _http.SendAsync(request);
            var result = await ReadResponseAsync<T>(response);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                _httpContextAccessor.HttpContext?.Session.Clear();
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Could not connect to the RaceDay API at {BaseAddress} for {Path}.", _http.BaseAddress, path);
            return Failure<T>(503,
                "Could not connect to the RaceDay API. Make sure the API project is running and check RaceDay.Web/appsettings.json.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "The RaceDay API request timed out for {Path}.", path);
            return Failure<T>(504, "The API request timed out. Please try again.");
        }
    }

    private static async Task<ApiResult<T>> ReadResponseAsync<T>(HttpResponseMessage response)
    {
        var content = response.Content == null ? string.Empty : await response.Content.ReadAsStringAsync();
        var statusCode = (int)response.StatusCode;
        var success = response.IsSuccessStatusCode;
        var message = ExtractMessage(content);

        T? data = default;
        if (success && !string.IsNullOrWhiteSpace(content))
        {
            try
            {
                data = JsonSerializer.Deserialize<T>(content, JsonOptions);
            }
            catch (JsonException)
            {
                // Some successful endpoints may return plain text or an empty body.
            }
        }

        if (string.IsNullOrWhiteSpace(message))
            message = success ? "Request completed successfully." : response.ReasonPhrase ?? "The API request failed.";

        return new ApiResult<T>
        {
            IsSuccess = success,
            StatusCode = statusCode,
            Message = message,
            Data = data
        };
    }

    private static string ExtractMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
                    return message.GetString() ?? string.Empty;
                if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                    return title.GetString() ?? string.Empty;
                if (root.TryGetProperty("errors", out var errors))
                    return errors.ToString();
            }
        }
        catch (JsonException)
        {
            return content.Length > 250 ? content[..250] : content;
        }

        return string.Empty;
    }

    private static ApiResult<T> Failure<T>(int statusCode, string message) => new()
    {
        IsSuccess = false,
        StatusCode = statusCode,
        Message = message
    };
}

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Services;


public class ApiResult<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }
    public int StatusCode { get; init; }
}


public class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _accessor;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient http, IHttpContextAccessor accessor, ILogger<ApiClient> logger)
    {
        _http = http;
        _accessor = accessor;
        _logger = logger;
    }

    public Task<ApiResult<T>> GetAsync<T>(string url) =>
        SendAsync<T>(HttpMethod.Get, url, null);

    public Task<ApiResult<T>> PostAsync<T>(string url, object body) =>
        SendAsync<T>(HttpMethod.Post, url, ToJson(body));

    public Task<ApiResult<T>> PutAsync<T>(string url, object body) =>
        SendAsync<T>(HttpMethod.Put, url, ToJson(body));

    public Task<ApiResult<bool>> DeleteAsync(string url) =>
        SendAsync<bool>(HttpMethod.Delete, url, null);

   
    public async Task<ApiResult<ImageUploadResponse>> UploadImageAsync(string url, IFormFile file)
    {
        using var form = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        form.Add(fileContent, "file", file.FileName);
        return await SendAsync<ImageUploadResponse>(HttpMethod.Post, url, form);
    }

   
    public async Task<(byte[] Bytes, string ContentType)?> GetImageAsync(string name)
    {
        try
        {
            using var response = await _http.GetAsync($"api/images?name={Uri.EscapeDataString(name)}");
            if (!response.IsSuccessStatusCode) return null;

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            return (bytes, contentType);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return null;
        }
    }

    private static HttpContent ToJson(object body) =>
        JsonContent.Create(body, body.GetType(), options: JsonOptions);

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, HttpContent? content)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url) { Content = content };

            var token = _accessor.HttpContext?.Session.GetToken();
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _http.SendAsync(request);
            var status = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
                    return new ApiResult<T> { Success = true, StatusCode = status };

                var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
                return new ApiResult<T> { Success = true, Data = data, StatusCode = status };
            }

            var body = await response.Content.ReadAsStringAsync();
            return new ApiResult<T> { Success = false, StatusCode = status, Error = FriendlyError(status, body) };
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "API call failed: {Method} {Url}", method, url);
            return new ApiResult<T>
            {
                Success = false,
                StatusCode = 503,
                Error = "RaceDay is having trouble reaching its service right now. Please try again in a moment."
            };
        }
    }

   
    private static string FriendlyError(int status, string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    var messages = new List<string>();
                    foreach (var field in errors.EnumerateObject())
                        foreach (var message in field.Value.EnumerateArray())
                            messages.Add(message.GetString() ?? string.Empty);

                    var joined = string.Join(" ", messages.Where(m => m.Length > 0));
                    if (joined.Length > 0) return joined;
                }

                if (status < 500 && root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                    return detail.GetString() ?? Fallback(status);
            }
        }
        catch (JsonException)
        {
            // not JSON - use the standard message below
        }

        return Fallback(status);
    }

    private static string Fallback(int status) => status switch
    {
        401 => "Please log in to continue.",
        403 => "You do not have permission to do that.",
        404 => "We could not find what you were looking for.",
        >= 500 => "Something went wrong on our side. Please try again.",
        _ => "That request could not be completed. Please check your details and try again."
    };
}

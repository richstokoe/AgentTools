using System.ComponentModel;

namespace RichStokoe.AgentTools.Web;

public static class WebFetchTools
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    private const int MaxResponseLength = 100_000;

    static WebFetchTools()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "RichStokoe.AgentTools/1.0");
    }

    [AgentTool(Type = AgentToolTypes.Read)]
    [Description("Performs an HTTP GET request to the given URL and returns the response body as text. Only http and https URLs are supported. Response is truncated if very large.")]
    public static async Task<string> Fetch_Url(
        [Description("The absolute http(s) URL to fetch.")] string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return "Error: URL must not be empty.";
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return $"Error: '{url}' is not a valid absolute URL.";
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return $"Error: only http and https URLs are supported. Got scheme '{uri.Scheme}'.";
        }

        try
        {
            using var response = await _httpClient.GetAsync(uri);
            var body = await response.Content.ReadAsStringAsync();

            if (body.Length > MaxResponseLength)
            {
                body = body.Substring(0, MaxResponseLength) + $"\n\n[Response truncated at {MaxResponseLength} characters.]";
            }

            var contentType = response.Content.Headers.ContentType?.ToString() ?? "unknown";
            var status = (int)response.StatusCode;

            return $"HTTP {status} {response.StatusCode}\nContent-Type: {contentType}\n\n{body}";
        }
        catch (TaskCanceledException)
        {
            return $"Error: request to '{url}' timed out.";
        }
        catch (HttpRequestException ex)
        {
            return $"Error fetching '{url}': {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error fetching '{url}': {ex.Message}";
        }
    }
}

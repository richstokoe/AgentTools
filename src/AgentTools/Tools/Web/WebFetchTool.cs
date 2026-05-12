using System.ComponentModel;
using System.Text.RegularExpressions;

namespace RichStokoe.AgentTools.Web;

public static class WebFetchTool
{
    private static readonly HttpClient Http = new();

    static WebFetchTool()
    {
        Http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; RS-AgentTools/1.1)");
        Http.Timeout = TimeSpan.FromSeconds(30);
    }

    [Description("Fetch the content of a URL and return it as plain text. Useful for reading web pages, documentation, APIs, or any HTTP resource after finding URLs via web search.")]
    public static async Task<string> FetchUrl(
        [Description("The URL to fetch.")] string url,
        [Description("Maximum number of characters to return (default 8000). Use a higher value for long documents.")] int maxLength = 8000)
    {
        try
        {
            var response = await Http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
            var raw = await response.Content.ReadAsStringAsync();

            var text = contentType.Contains("html", StringComparison.OrdinalIgnoreCase)
                ? StripHtml(raw)
                : raw;

            text = text.Trim();

            if (text.Length > maxLength)
                text = text[..maxLength] + $"\n\n[truncated — {text.Length - maxLength} more characters]";

            return string.IsNullOrEmpty(text) ? "[no content]" : text;
        }
        catch (Exception ex)
        {
            return $"Error fetching {url}: {ex.Message}";
        }
    }

    private static string StripHtml(string html)
    {
        // Remove script and style blocks entirely
        html = Regex.Replace(html, @"<(script|style)[^>]*>[\s\S]*?</(script|style)>", "", RegexOptions.IgnoreCase);
        // Remove all remaining tags
        html = Regex.Replace(html, @"<[^>]+>", " ");
        // Decode HTML entities
        html = System.Net.WebUtility.HtmlDecode(html);
        // Collapse whitespace
        html = Regex.Replace(html, @"[ \t]+", " ");
        html = Regex.Replace(html, @"\n[ \t]*\n[ \t]*\n+", "\n\n");
        return html.Trim();
    }
}

using RichStokoe.AgentTools.Tests.Helpers;
using RichStokoe.AgentTools.Web;

namespace RichStokoe.AgentTools.Tests.Web;

public class WebFetchToolsTests
{
    // --- Validation (no HTTP calls) ---

    [Fact]
    public async Task Fetch_Url_EmptyUrl_ReturnsError()
    {
        var result = await WebFetchTools.Fetch_Url("");
        Assert.StartsWith("Error:", result);
    }

    [Fact]
    public async Task Fetch_Url_WhitespaceUrl_ReturnsError()
    {
        var result = await WebFetchTools.Fetch_Url("   ");
        Assert.StartsWith("Error:", result);
    }

    [Fact]
    public async Task Fetch_Url_NotAUrl_ReturnsInvalidUrlError()
    {
        var result = await WebFetchTools.Fetch_Url("not-a-url");
        Assert.StartsWith("Error:", result);
        Assert.Contains("valid", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Fetch_Url_FtpScheme_ReturnsUnsupportedSchemeError()
    {
        var result = await WebFetchTools.Fetch_Url("ftp://example.com/file.txt");
        Assert.StartsWith("Error:", result);
        Assert.Contains("ftp", result);
    }

    [Fact]
    public async Task Fetch_Url_FileScheme_ReturnsUnsupportedSchemeError()
    {
        var result = await WebFetchTools.Fetch_Url("file:///etc/passwd");
        Assert.StartsWith("Error:", result);
        Assert.Contains("file", result);
    }

    // --- Integration (local HTTP server) ---

    [Fact]
    public async Task Fetch_Url_SuccessfulResponse_ReturnsStatusCodeAndBody()
    {
        using var server = new TestHttpServer();
        server.EnqueueResponse(200, "text/plain", "Hello, Agent!");

        var result = await WebFetchTools.Fetch_Url($"{server.BaseUrl}/hello");

        Assert.Contains("HTTP 200", result);
        Assert.Contains("text/plain", result);
        Assert.Contains("Hello, Agent!", result);
    }

    [Fact]
    public async Task Fetch_Url_HttpResponse_ReturnsStatusCode()
    {
        using var server = new TestHttpServer();
        server.EnqueueResponse(404, "text/plain", "Not Found");

        var result = await WebFetchTools.Fetch_Url($"{server.BaseUrl}/missing");

        Assert.Contains("HTTP 404", result);
    }

    [Fact]
    public async Task Fetch_Url_ResponseExceedsLimit_TruncatesWithMessage()
    {
        using var server = new TestHttpServer();
        server.EnqueueResponse(200, "text/plain", new string('x', 110_000));

        var result = await WebFetchTools.Fetch_Url($"{server.BaseUrl}/large");

        Assert.Contains("truncated", result, StringComparison.OrdinalIgnoreCase);
        // Should not contain the full 110K body
        Assert.True(result.Length < 110_500);
    }

    [Fact]
    public async Task Fetch_Url_ResponseWithinLimit_NotTruncated()
    {
        using var server = new TestHttpServer();
        var body = new string('x', 50_000);
        server.EnqueueResponse(200, "text/plain", body);

        var result = await WebFetchTools.Fetch_Url($"{server.BaseUrl}/small");

        Assert.DoesNotContain("truncated", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(body, result);
    }

    [Fact]
    public async Task Fetch_Url_JsonResponse_ReturnsParsedContent()
    {
        using var server = new TestHttpServer();
        server.EnqueueResponse(200, "application/json", """{"key":"value"}""");

        var result = await WebFetchTools.Fetch_Url($"{server.BaseUrl}/api");

        Assert.Contains("application/json", result);
        Assert.Contains("""{"key":"value"}""", result);
    }
}

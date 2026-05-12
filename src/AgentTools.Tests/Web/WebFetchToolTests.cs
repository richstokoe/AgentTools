using RichStokoe.AgentTools.Tests.Helpers;
using RichStokoe.AgentTools.Web;

namespace RichStokoe.AgentTools.Tests.Web;

public class WebFetchToolTests
{
    // --- Invalid URLs (no HTTP call, exception caught) ---

    [Fact]
    public async Task FetchUrl_EmptyUrl_ReturnsError()
    {
        var result = await WebFetchTool.FetchUrl("");
        Assert.StartsWith("Error fetching", result);
    }

    [Fact]
    public async Task FetchUrl_NotAUrl_ReturnsError()
    {
        var result = await WebFetchTool.FetchUrl("not-a-url");
        Assert.StartsWith("Error fetching", result);
    }

    // --- Integration (local HTTP server) ---

    [Fact]
    public async Task FetchUrl_PlainTextResponse_ReturnsBody()
    {
        using var server = new TestHttpServer();
        server.EnqueueResponse(200, "text/plain", "Hello from agent!");

        var result = await WebFetchTool.FetchUrl($"{server.BaseUrl}/text");

        Assert.Equal("Hello from agent!", result);
    }

    [Fact]
    public async Task FetchUrl_HtmlResponse_StripsTagsAndReturnsText()
    {
        using var server = new TestHttpServer();
        server.EnqueueResponse(200, "text/html", "<html><body><p>Hello, <b>world</b>!</p></body></html>");

        var result = await WebFetchTool.FetchUrl($"{server.BaseUrl}/page");

        Assert.Contains("Hello", result);
        Assert.Contains("world", result);
        Assert.DoesNotContain("<p>", result);
        Assert.DoesNotContain("<b>", result);
    }

    [Fact]
    public async Task FetchUrl_HtmlWithScriptAndStyle_RemovesScriptAndStyleBlocks()
    {
        using var server = new TestHttpServer();
        server.EnqueueResponse(200, "text/html", """
            <html>
            <head><style>body { color: red; }</style></head>
            <body>
              <script>alert('xss');</script>
              <p>Visible text</p>
            </body>
            </html>
            """);

        var result = await WebFetchTool.FetchUrl($"{server.BaseUrl}/html");

        Assert.Contains("Visible text", result);
        Assert.DoesNotContain("alert", result);
        Assert.DoesNotContain("color: red", result);
    }

    [Fact]
    public async Task FetchUrl_EmptyResponseBody_ReturnsNoContent()
    {
        using var server = new TestHttpServer();
        server.EnqueueResponse(200, "text/plain", "");

        var result = await WebFetchTool.FetchUrl($"{server.BaseUrl}/empty");

        Assert.Equal("[no content]", result);
    }

    [Fact]
    public async Task FetchUrl_NonSuccessStatusCode_ReturnsError()
    {
        using var server = new TestHttpServer();
        server.EnqueueResponse(404, "text/plain", "Not Found");

        var result = await WebFetchTool.FetchUrl($"{server.BaseUrl}/missing");

        Assert.StartsWith("Error fetching", result);
    }

    [Fact]
    public async Task FetchUrl_ResponseExceedsDefaultMaxLength_TruncatesWithMessage()
    {
        using var server = new TestHttpServer();
        server.EnqueueResponse(200, "text/plain", new string('x', 10_000));

        var result = await WebFetchTool.FetchUrl($"{server.BaseUrl}/large", maxLength: 100);

        Assert.True(result.Length < 10_000);
        Assert.Contains("truncated", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("more characters", result);
    }

    [Fact]
    public async Task FetchUrl_ResponseWithinMaxLength_NotTruncated()
    {
        using var server = new TestHttpServer();
        var body = new string('x', 500);
        server.EnqueueResponse(200, "text/plain", body);

        var result = await WebFetchTool.FetchUrl($"{server.BaseUrl}/small", maxLength: 1000);

        Assert.Equal(body, result);
        Assert.DoesNotContain("truncated", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FetchUrl_CustomMaxLength_RespectsLimit()
    {
        using var server = new TestHttpServer();
        server.EnqueueResponse(200, "text/plain", new string('a', 200));

        var result = await WebFetchTool.FetchUrl($"{server.BaseUrl}/custom", maxLength: 50);

        // First 50 chars + truncation message
        Assert.StartsWith(new string('a', 50), result);
        Assert.Contains("150 more characters", result);
    }
}

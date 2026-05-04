using RichStokoe.AgentTools.Tests.Helpers;
using RichStokoe.AgentTools.Web;

namespace RichStokoe.AgentTools.Tests.Web;

public class RssFeedToolsTests
{
    private const string SampleFeed = """
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="2.0">
          <channel>
            <title>Test Feed</title>
            <description>A test RSS feed</description>
            <link>http://example.com</link>
            <item>
              <title>Article One</title>
              <link>http://example.com/1</link>
              <pubDate>Mon, 01 Jan 2024 12:00:00 GMT</pubDate>
              <description>First article summary.</description>
            </item>
            <item>
              <title>Article Two</title>
              <link>http://example.com/2</link>
              <pubDate>Tue, 02 Jan 2024 12:00:00 GMT</pubDate>
              <description>Second article summary.</description>
            </item>
            <item>
              <title>Article Three</title>
              <link>http://example.com/3</link>
              <pubDate>Wed, 03 Jan 2024 12:00:00 GMT</pubDate>
              <description>Third article summary.</description>
            </item>
          </channel>
        </rss>
        """;

    // --- Get_Latest_News (pure path: unknown source, no HTTP) ---

    [Fact]
    public async Task Get_Latest_News_UnknownSource_ReturnsError()
    {
        var result = await RssFeedTools.Get_Latest_News("no_such_source_xyz");
        Assert.Contains("Unknown news source", result);
        Assert.Contains("no_such_source_xyz", result);
    }

    [Fact]
    public async Task Get_Latest_News_UnknownSource_ListsAvailableSources()
    {
        var result = await RssFeedTools.Get_Latest_News("unknown");
        Assert.Contains("bbc", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("hackernews", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("techcrunch", result, StringComparison.OrdinalIgnoreCase);
    }

    // --- Read_Rss_Feed (integration with local HTTP server) ---

    [Fact]
    public async Task Read_Rss_Feed_ValidFeed_ReturnsFeedTitleAndArticles()
    {
        using var server = new TestHttpServer();
        server.EnqueueResponse(200, "application/rss+xml", SampleFeed);

        var result = await RssFeedTools.Read_Rss_Feed($"{server.BaseUrl}/feed.rss");

        Assert.Contains("Test Feed", result);
        Assert.Contains("Article One", result);
        Assert.Contains("Article Two", result);
    }

    [Fact]
    public async Task Read_Rss_Feed_MaxArticles_LimitsReturnedItems()
    {
        using var server = new TestHttpServer();
        server.EnqueueResponse(200, "application/rss+xml", SampleFeed);

        var result = await RssFeedTools.Read_Rss_Feed($"{server.BaseUrl}/feed.rss", maxArticles: 1);

        Assert.Contains("Article One", result);
        Assert.DoesNotContain("Article Two", result);
        Assert.DoesNotContain("Article Three", result);
    }

    [Fact]
    public async Task Read_Rss_Feed_MaxArticlesExceedsItems_ReturnsAllItems()
    {
        using var server = new TestHttpServer();
        server.EnqueueResponse(200, "application/rss+xml", SampleFeed);

        // Feed has 3 items; requesting 20 should return all 3
        var result = await RssFeedTools.Read_Rss_Feed($"{server.BaseUrl}/feed.rss", maxArticles: 20);

        Assert.Contains("Article One", result);
        Assert.Contains("Article Two", result);
        Assert.Contains("Article Three", result);
    }

    [Fact]
    public async Task Read_Rss_Feed_MaxArticlesZero_ClampsToOne()
    {
        using var server = new TestHttpServer();
        server.EnqueueResponse(200, "application/rss+xml", SampleFeed);

        var result = await RssFeedTools.Read_Rss_Feed($"{server.BaseUrl}/feed.rss", maxArticles: 0);

        // Clamped to 1, so only first article returned
        Assert.Contains("Article One", result);
        Assert.DoesNotContain("Article Two", result);
    }

    [Fact]
    public async Task Read_Rss_Feed_InvalidXml_ReturnsError()
    {
        using var server = new TestHttpServer();
        server.EnqueueResponse(200, "application/rss+xml", "this is not << valid >> xml");

        var result = await RssFeedTools.Read_Rss_Feed($"{server.BaseUrl}/bad.rss");

        Assert.Contains("Error", result);
    }

    [Fact]
    public async Task Read_Rss_Feed_ServerError_ReturnsError()
    {
        using var server = new TestHttpServer();
        server.EnqueueResponse(500, "text/plain", "Internal Server Error");

        var result = await RssFeedTools.Read_Rss_Feed($"{server.BaseUrl}/error");

        Assert.Contains("Error", result);
    }
}

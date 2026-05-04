namespace RichStokoe.AgentTools.Tests;

public class ToolManagerTests
{
    private readonly ToolManager _manager = new();

    [Fact]
    public void GetTools_NoFilter_ReturnsAllTools()
    {
        var tools = _manager.GetTools();
        Assert.NotEmpty(tools);
    }

    [Fact]
    public void GetTools_ExactNameMatch_ReturnsSingleTool()
    {
        var tools = _manager.GetTools("Add_Numbers");
        Assert.Single(tools);
        Assert.Equal("Add_Numbers", tools[0].Name);
    }

    [Fact]
    public void GetTools_WildcardPattern_ReturnsMatchingTools()
    {
        var tools = _manager.GetTools("Get_*");
        Assert.NotEmpty(tools);
        Assert.All(tools, t => Assert.StartsWith("Get_", t.Name));
    }

    [Fact]
    public void GetTools_NoMatchPattern_ReturnsEmpty()
    {
        var tools = _manager.GetTools("Nonexistent_Tool_XYZ");
        Assert.Empty(tools);
    }

    [Fact]
    public void GetTools_ReadTypeFilter_IncludesFetchUrl()
    {
        var tools = _manager.GetTools(typeFilter: AgentToolTypes.Read);
        Assert.Contains(tools, t => t.Name == "Fetch_Url");
    }

    [Fact]
    public void GetTools_ReadTypeFilter_ExcludesUnclassifiedTools()
    {
        var allTools = _manager.GetTools();
        var readTools = _manager.GetTools(typeFilter: AgentToolTypes.Read);

        // At least one tool has no type classification so the read-filtered set is smaller
        Assert.True(readTools.Count < allTools.Count);
    }

    [Fact]
    public void GetTools_PatternAndTypeFilter_BothApplied()
    {
        // Fetch_Url is tagged Read and matches "Fetch_*"
        var tools = _manager.GetTools("Fetch_*", AgentToolTypes.Read);
        Assert.Single(tools);
        Assert.Equal("Fetch_Url", tools[0].Name);
    }
}

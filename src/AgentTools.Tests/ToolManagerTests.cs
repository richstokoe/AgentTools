namespace RichStokoe.AgentTools.Tests;

public class ToolManagerTests
{
    private readonly ToolManager _manager = new();

    // --- Opt-in: no type filter → no tools ---

    [Fact]
    public void GetTools_NoFilter_ReturnsEmpty()
    {
        Assert.Empty(_manager.GetTools());
    }

    [Fact]
    public void GetTools_NamePatternWithNoTypeFilter_ReturnsEmpty()
    {
        // Name pattern alone is not enough — type must be opted into
        Assert.Empty(_manager.GetTools("RunCommand"));
        Assert.Empty(_manager.GetTools("Add_Numbers"));
        Assert.Empty(_manager.GetTools("*"));
    }

    // --- Dangerous type ---

    [Fact]
    public void GetTools_DangerousFilter_ReturnsBashTool()
    {
        var tools = _manager.GetTools(typeFilter: AgentToolTypes.Dangerous);
        Assert.Contains(tools, t => t.Name == "RunCommand");
    }

    [Fact]
    public void GetTools_DangerousFilter_ExcludesUnclassifiedTools()
    {
        var tools = _manager.GetTools(typeFilter: AgentToolTypes.Dangerous);
        Assert.DoesNotContain(tools, t => t.Name == "Add_Numbers");
    }

    [Fact]
    public void GetTools_ExactNameAndDangerousType_ReturnsSingleTool()
    {
        var tools = _manager.GetTools("RunCommand", AgentToolTypes.Dangerous);
        Assert.Single(tools);
        Assert.Equal("RunCommand", tools[0].Name);
    }

    [Fact]
    public void GetTools_WrongNameAndDangerousType_ReturnsEmpty()
    {
        Assert.Empty(_manager.GetTools("Nonexistent_XYZ", AgentToolTypes.Dangerous));
    }

    // --- Read type (no tools currently tagged Read) ---

    [Fact]
    public void GetTools_ReadFilter_ReturnsEmpty_WhenNoToolsTaggedRead()
    {
        Assert.Empty(_manager.GetTools(typeFilter: AgentToolTypes.Read));
    }

    // --- Combined flags ---

    [Fact]
    public void GetTools_ReadOrDangerousFilter_IncludesBashTool()
    {
        var tools = _manager.GetTools(typeFilter: AgentToolTypes.Read | AgentToolTypes.Dangerous);
        Assert.Contains(tools, t => t.Name == "RunCommand");
    }

    [Fact]
    public void GetTools_WildcardPatternAndDangerousType_FiltersCorrectly()
    {
        var tools = _manager.GetTools("Run*", AgentToolTypes.Dangerous);
        Assert.NotEmpty(tools);
        Assert.All(tools, t => Assert.StartsWith("Run", t.Name));
    }
}

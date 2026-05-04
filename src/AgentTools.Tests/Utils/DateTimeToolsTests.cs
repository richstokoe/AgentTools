using RichStokoe.AgentTools.Utils;

namespace RichStokoe.AgentTools.Tests.Utils;

public class DateTimeToolsTests
{
    [Fact]
    public void Get_Current_Time_ReturnsNonEmptyString()
    {
        var result = DateTimeTools.Get_Current_Time();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Get_Current_Date_ReturnsNonEmptyString()
    {
        var result = DateTimeTools.Get_Current_Date();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Get_Current_Date_ContainsCurrentYear()
    {
        var result = DateTimeTools.Get_Current_Date();
        Assert.Contains(DateTime.Now.Year.ToString(), result);
    }

    [Fact]
    public void Get_Current_Time_MatchesCurrentHour()
    {
        // Snapshot before and after to guard against a clock tick between calls
        var before = DateTime.Now;
        var result = DateTimeTools.Get_Current_Time();
        var after = DateTime.Now;

        Assert.True(
            result.Contains(before.Hour.ToString()) || result.Contains(after.Hour.ToString()),
            $"Expected time string to contain hour {before.Hour} or {after.Hour}, got: {result}");
    }
}

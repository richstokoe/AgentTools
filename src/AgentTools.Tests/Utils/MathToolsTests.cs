using RichStokoe.AgentTools.Utils;

namespace RichStokoe.AgentTools.Tests.Utils;

public class MathToolsTests
{
    [Fact]
    public void Add_Numbers_EmptySequence_ReturnsZero()
    {
        Assert.Equal(0m, MathTools.Add_Numbers([]));
    }

    [Fact]
    public void Add_Numbers_SingleValue_ReturnsThatValue()
    {
        Assert.Equal(42m, MathTools.Add_Numbers([42m]));
    }

    [Fact]
    public void Add_Numbers_MultipleIntegers_ReturnsCorrectSum()
    {
        Assert.Equal(6m, MathTools.Add_Numbers([1m, 2m, 3m]));
    }

    [Fact]
    public void Add_Numbers_NegativeValues_ReturnsCorrectSum()
    {
        Assert.Equal(0m, MathTools.Add_Numbers([-5m, 5m]));
        Assert.Equal(-3m, MathTools.Add_Numbers([-1m, -2m]));
    }

    [Fact]
    public void Add_Numbers_Decimals_ReturnsExactSum()
    {
        // Decimal arithmetic is exact, unlike floating-point
        Assert.Equal(0.3m, MathTools.Add_Numbers([0.1m, 0.2m]));
    }

    [Fact]
    public void Add_Numbers_LargeSequence_ReturnsCorrectSum()
    {
        var numbers = Enumerable.Range(1, 100).Select(i => (decimal)i);
        Assert.Equal(5050m, MathTools.Add_Numbers(numbers));
    }
}

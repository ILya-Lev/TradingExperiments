using FluentAssertions;
using FluentAssertions.Execution;

namespace Udemy.Fin.Stat.Tests;

[Trait("Category", "Unit")]
public class UrnModelTests(ITestOutputHelper output)
{
    [Fact]
    public void GetProbabilityOfPickingColorAt_Red2And1_Blue0And2_2ndPick()
    {
        var urnModel = new UrnModel([20, 30], [[2, 1], [0, 2]]);

        var redAt1AttemptProbability = urnModel.GetProbabilityOfPickingColorAt(0, 1);
        var blueAt1AttemptProbability = urnModel.GetProbabilityOfPickingColorAt(1, 1);
    
        var redAt2AttemptProbability = urnModel.GetProbabilityOfPickingColorAt(0, 2);
        var blueAt2AttemptProbability = urnModel.GetProbabilityOfPickingColorAt(1, 2);

        using var _ = new AssertionScope();
        redAt1AttemptProbability.Should().Be(0.4);
        blueAt1AttemptProbability.Should().Be(0.6);
        
        redAt2AttemptProbability.Should().BeApproximately(0.3968, 1e-4);
        blueAt2AttemptProbability.Should().BeApproximately(0.6032, 1e-4);
    }

    [Fact]
    public void GetProbabilityOfPickingColorAt_Any10All_2ndPick()
    {
        const int d = 10;
        var urnModel = new UrnModel([10, 15, 25], [[d, d, d], [d, d, d], [d, d, d]]);

        var blueAt1AttemptProbability = urnModel.GetProbabilityOfPickingColorAt(0, 1);
        var redAt1AttemptProbability = urnModel.GetProbabilityOfPickingColorAt(1, 1);
        var greenAt1AttemptProbability = urnModel.GetProbabilityOfPickingColorAt(2, 1);
     
        var blueAt2AttemptProbability = urnModel.GetProbabilityOfPickingColorAt(0, 2);
        var redAt2AttemptProbability = urnModel.GetProbabilityOfPickingColorAt(1, 2);
        var greenAt2AttemptProbability = urnModel.GetProbabilityOfPickingColorAt(2, 2);

        using var _ = new AssertionScope();
        blueAt1AttemptProbability.Should().Be(0.2);
        redAt1AttemptProbability.Should().Be(0.3);
        greenAt1AttemptProbability.Should().Be(0.5);
        
        blueAt2AttemptProbability.Should().Be(0.2);
        redAt2AttemptProbability.Should().Be(0.3);
        greenAt2AttemptProbability.Should().Be(0.5);
    }

    [Fact]
    public void GetProbabilityOfPickingColorAt_R10r_B5b5r_G10b5g_2ndPick()
    {
        var urnModel = new UrnModel([10, 15, 25], [[5,5,0], [0, 10, 0], [5, 0, 5]]);

        var blueAt1AttemptProbability = urnModel.GetProbabilityOfPickingColorAt(0, 1);
        var redAt1AttemptProbability = urnModel.GetProbabilityOfPickingColorAt(1, 1);
        var greenAt1AttemptProbability = urnModel.GetProbabilityOfPickingColorAt(2, 1);
     
        var blueAt2AttemptProbability = urnModel.GetProbabilityOfPickingColorAt(0, 2);
        var redAt2AttemptProbability = urnModel.GetProbabilityOfPickingColorAt(1, 2);
        var greenAt2AttemptProbability = urnModel.GetProbabilityOfPickingColorAt(2, 2);

        using var _ = new AssertionScope();
        blueAt1AttemptProbability.Should().Be(0.2);
        redAt1AttemptProbability.Should().Be(0.3);
        greenAt1AttemptProbability.Should().Be(0.5);
        
        blueAt2AttemptProbability.Should().BeApproximately(0.225, 1e-3);
        redAt2AttemptProbability.Should().BeApproximately(0.3167, 1e-4);
        greenAt2AttemptProbability.Should().BeApproximately(0.4583, 1e-4);
    }
}
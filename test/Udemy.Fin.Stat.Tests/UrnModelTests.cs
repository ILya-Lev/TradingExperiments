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

        var firstAttempt = urnModel.GetProbabilityOfPickingColorAt(1);
        var secondAttempt = urnModel.GetProbabilityOfPickingColorAt(2);
        
        using var _ = new AssertionScope();
        firstAttempt.Should().HaveElementAt(0, 0.4);
        firstAttempt.Should().HaveElementAt(1, 0.6);
        firstAttempt.Sum().Should().BeApproximately(1, 1e-4);

        secondAttempt[0].Should().BeApproximately(0.3968, 1e-4);
        secondAttempt[1].Should().BeApproximately(0.6032, 1e-4);
        secondAttempt.Sum().Should().BeApproximately(1, 1e-4);
    }

    [Fact]
    public void GetProbabilityOfPickingColorAt_Any10All_2ndPick()
    {
        const int d = 1123;//could be any - in fact is a scale for the identity matrix
        var urnModel = new UrnModel([10, 15, 25], [[d, 0, 0], [0, d, 0], [0, 0, d]]);

        var firstAttempt = urnModel.GetProbabilityOfPickingColorAt(1);
        var secondAttempt = urnModel.GetProbabilityOfPickingColorAt(2);
        
        using var _ = new AssertionScope();
        firstAttempt.Should().HaveElementAt(0, 0.2);
        firstAttempt.Should().HaveElementAt(1, 0.3);
        firstAttempt.Should().HaveElementAt(2, 0.5);
        firstAttempt.Sum().Should().BeApproximately(1, 1e-4);

        secondAttempt.Should().HaveElementAt(0, 0.2);
        secondAttempt.Should().HaveElementAt(1, 0.3);
        secondAttempt.Should().HaveElementAt(2, 0.5);
        secondAttempt.Sum().Should().BeApproximately(1, 1e-4);
    }

    [Fact]
    public void GetProbabilityOfPickingColorAt_R10r_B5b5r_G5b5g_2ndPick()
    {
        var urnModel = new UrnModel([10, 15, 25], [[5, 5, 0], [0, 10, 0], [5, 0, 5]]);

        var firstAttempt = urnModel.GetProbabilityOfPickingColorAt( 1);
        var secondAttempt = urnModel.GetProbabilityOfPickingColorAt( 2);

        using var _ = new AssertionScope();
        firstAttempt.Should().HaveElementAt(0, 0.2);
        firstAttempt.Should().HaveElementAt(1, 0.3);
        firstAttempt.Should().HaveElementAt(2, 0.5);
        firstAttempt.Sum().Should().BeApproximately(1, 1e-4);

        secondAttempt[0].Should().BeApproximately(0.225, 1e-3);
        secondAttempt[1].Should().BeApproximately(0.3167, 1e-4);
        secondAttempt[2].Should().BeApproximately(0.4583, 1e-4);
        secondAttempt.Sum().Should().BeApproximately(1, 1e-4);
    }

    [Fact]//growth exponentially with base 3 and power of iterations: 3^15 ~ 14M - should be affordable
    public void GetProbabilityOfPickingColorAt_R10r_B5b5r_G5b5g_50thPick()
    {
        var urnModel = new UrnModel([10, 15, 25], [[5, 5, 0], [0, 10, 0], [5, 0, 5]]);

        var theAttempt = urnModel.GetProbabilityOfPickingColorAt(15);

        using var _ = new AssertionScope();
        theAttempt[0].Should().BeApproximately(0.2787, 1e-4);
        theAttempt[1].Should().BeApproximately(0.4601, 1e-4);
        theAttempt[2].Should().BeApproximately(0.2613, 1e-4);
        theAttempt.Sum().Should().BeApproximately(1, 1e-4);
    }
}
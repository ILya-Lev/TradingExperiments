using FluentAssertions;

namespace Udemy.Fin.Stat.Tests;

[Trait("Category", "Unit")]
public class ElevatorProblemStateCounterTests
{
    [Fact]
    public void GetStates_9n9_Observe()
    {
        var states = ElevatorProblemStateCounter.GetStates(9, 9);
        states.Should().Be(9L * 8 * 7 * 6 * 5 * 4 * 3 * 2 * 1);
        //found 9_426_384
    }
}
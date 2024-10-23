using FluentAssertions;
using Xunit.Abstractions;

namespace Sudoku.Tests;

public class ChallengeTests(ITestOutputHelper output)
{
    [Fact]
    public void Evolve_AlwaysOne_Observe()
    {
        var steps = Enumerable.Repeat(1, 100)
            .Aggregate(new List<int>(), (acc, _) =>
            {
                acc.AddRange(new[] { 1, 1, 1, 1, 1 });
                return acc;
            })
            .ToArray();
        var initial = new Challenge.State(0, 0, 0, 0, 0);
        
        var evolution = Challenge.Evolve(initial, steps).ToArray();

        var loopLength = evolution.Length / evolution.Count(s => s == evolution[0]);
        output.WriteLine($"loop size is {loopLength}");

        foreach (var (state, index) in evolution.Select((s,i) => (s,i))) output.WriteLine($"{index+1} {state}");

        evolution.Contains(new Challenge.State(4, 4, 4, 4, 4)).Should().BeTrue();
    }
}
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

    [Fact]
    public void BugOnCube_MinimalPathBetweenDiagonalVertices()
    {
        const int scale = 100;
        var minDistanceData = Enumerable.Range(0, 45*scale)
            .AsParallel()
            .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
            .WithDegreeOfParallelism(Environment.ProcessorCount-1)
            .Select(grad => (grad: grad*1.0/scale, rad: grad*1.0/scale * Math.PI / 180))
            .Select(item => (item.grad, 1 / Math.Cos(item.rad) + 1 / Math.Cos(Math.PI / 4 - item.rad)))
            .MinBy(item => item.Item2);

        output.WriteLine($"min distance is {minDistanceData.Item2} for alpha {minDistanceData.grad}");
    }
}
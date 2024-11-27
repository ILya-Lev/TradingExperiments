using System.Collections.Concurrent;
using ApprovalTests;
using ApprovalTests.Reporters;
using ApprovalTests.Reporters.TestFrameworks;
using FluentAssertions;
using Xunit.Abstractions;

namespace Sudoku.Tests;

[Trait("Category", "Unit")]
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

        foreach (var (state, index) in evolution.Select((s, i) => (s, i))) output.WriteLine($"{index + 1} {state}");

        evolution.Contains(new Challenge.State(4, 4, 4, 4, 4)).Should().BeTrue();
    }

    [Fact]
    public void BugOnCube_MinimalPathBetweenDiagonalVertices()
    {
        const int scale = 100;
        var minDistanceData = Enumerable.Range(0, 45 * scale)
            .AsParallel()
            .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
            .WithDegreeOfParallelism(Environment.ProcessorCount - 1)
            .Select(grad => (grad: grad * 1.0 / scale, rad: grad * 1.0 / scale * Math.PI / 180))
            .Select(item => (item.grad, 1 / Math.Cos(item.rad) + 1 / Math.Cos(Math.PI / 4 - item.rad)))
            .MinBy(item => item.Item2);

        output.WriteLine($"min distance is {minDistanceData.Item2} for alpha {minDistanceData.grad}");
    }

    [Fact]
    [UseReporter(typeof(VisualStudioReporter), typeof(XUnit2Reporter))]
    public void GenerateStatistics_DifferentTotals_ObserveOptimal()
    {
        LinksInTeams.ClearCache();
        var statistics = Enumerable
            .Range(9, 81) //from 9 to 90
            .Select(n => LinksInTeams
                .GenerateStatistics(n)
                .MinBy(s => s.mLinks))
            .ToArray();

        Approvals.VerifyAll(statistics,
            item => $"{item.n}->{item.nLinks} vs {item.m}->{item.mLinks};" +
                    $" {item.n % item.m} by {item.n / item.m + 1}" +
                    $" and {item.m - item.n % item.m} by {item.n / item.m}");

        foreach (var item in statistics)
        {
            output.WriteLine($"{item.n}->{item.nLinks} vs {item.m}->{item.mLinks};" +
                             $" {item.n % item.m} by {item.n / item.m + 1}" +
                             $" and {item.m - item.n % item.m} by {item.n / item.m}");
        }
    }

    [Fact]
    public void GenerateStatistics_DifferentTotals_FindMaxGroupSize()
    {
        LinksInTeams.ClearCache();
        var merger = new ConcurrentBag<(int n, long nLinks, int m, long mLinks)>();
        
        var organization = Enumerable.Range(10, 1000);//.Reverse();//.ToArray();

        Parallel.ForEach(Partitioner.Create(organization, EnumerablePartitionerOptions.NoBuffering),
            () => new List<(int n, long nLinks, int m, long mLinks)>(),
            (n, _, _, local) =>
            {
                local.Add(LinksInTeams.GenerateStatistics(n).MinBy(s => s.mLinks));
                return local;
            },
            local => merger.Add(local.Where(s => s.m != 0).MaxBy(s => s.n / s.m + 1))
        );

        var r = merger.Where(s => s.m != 0).MaxBy(s => s.n / s.m + 1);

        //var r = Enumerable
        //    .Range(10, 100_000)
        //    .AsParallel()
        //    .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
        //    .WithDegreeOfParallelism(Environment.ProcessorCount - 1)
        //    .Select(n => LinksInTeams
        //        .GenerateStatistics(n)
        //        .MinBy(s => s.mLinks))
        //    .MaxBy(s => s.n / s.m + 1)
        //    ;

        var report = $"case of the biggest group is" +
                           $" {r.n} -> {r.nLinks:N0} vs {r.m} -> {r.mLinks:N0}";
        if (r.n % r.m != 0)
            report += $" of {r.n % r.m} by {r.n / r.m + 1}";
        report += $" and {r.m - r.n % r.m} by {r.n / r.m}";
        
        output.WriteLine(report);
    }
}
using System.Diagnostics;
using FluentAssertions;
using TDF.Lib;
using Xunit.Abstractions;

namespace TDF.Tests;

public class AsyncStreamDemoTests(ITestOutputHelper output)
{
    [Fact]
    public async Task GetNumbers_CancelAfterOneSecond_Observe()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        await foreach (var number in AsyncStreamDemo.GetNumbers().WithCancellation(cts.Token))
            output.WriteLine($"{number}");
    }

    [Fact]
    public void GetAverage_Bln_Fast()
    {
        var numbers = Enumerable.Range(1, 1_000_000_000)
            .Select(n => Random.Shared.NextDouble() * 100)
            .ToArray();

        var sw = Stopwatch.StartNew();
        var parallel = AsyncStreamDemo.GetAverage(numbers);
        sw.Stop();
        var elapsedParallel = sw.Elapsed;

        sw = Stopwatch.StartNew();
        var plinq = AsyncStreamDemo.GetAveragePlinq(numbers);
        sw.Stop();
        var elapsedPlinq = sw.Elapsed;

        sw = Stopwatch.StartNew();
        var sequential = numbers.Average();
        sw.Stop();
        var elapsedSequentially = sw.Elapsed;
        
        output.WriteLine($"sequential run took {elapsedSequentially}");
        output.WriteLine($"parallel run took {elapsedParallel}");
        output.WriteLine($"plinq run took {elapsedPlinq}");
        parallel.Should().BeApproximately(sequential, 1e-8);
        plinq.Should().BeApproximately(sequential, 1e-8);
    }
}
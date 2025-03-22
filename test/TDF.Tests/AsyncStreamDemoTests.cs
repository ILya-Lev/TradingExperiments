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

    [Fact]
    public void Multiply_WithAndWithoutAsOrdered_Compare()
    {
        var numbers = Enumerable.Range(1, 1_000_000_000).ToArray();

        var sw = Stopwatch.StartNew();
        var sequential = numbers.Select(n => n * 2).ToArray();
        sw.Stop();
        var elapsedSequentially = sw.Elapsed;

        sw = Stopwatch.StartNew();
        var unordered = numbers
            .AsParallel().AsUnordered().WithDegreeOfParallelism(Environment.ProcessorCount-1)
            .Select(n => n * 2).ToArray();
        sw.Stop();
        var elapsedUnordered = sw.Elapsed;

        sw = Stopwatch.StartNew();
        var ordered = numbers
            .AsParallel().AsOrdered().WithDegreeOfParallelism(Environment.ProcessorCount-1)
            .Select(n => n * 2).ToArray();
        sw.Stop();
        var elapsedOrdered = sw.Elapsed;

        output.WriteLine($"sequential run took {elapsedSequentially}");
        output.WriteLine($"plinq unordered run took {elapsedUnordered}");
        output.WriteLine($"plinq ordered run took {elapsedOrdered}");

        //up to this place ~ half of the method run time 
        for (int i = 0; i < sequential.Length; i++)
        {
            if (unordered[i] != sequential[i])
                throw new Exception($"{i} element unordered {unordered[i]} mismatch sequential {sequential[i]}");

            if (ordered[i] != sequential[i])
                throw new Exception($"{i} element ordered {ordered[i]} mismatch sequential {sequential[i]}");
        }
    }
}
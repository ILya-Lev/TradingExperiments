using System.Diagnostics;
using FluentAssertions;
using TDF.Lib;
using Xunit.Abstractions;

namespace TDF.Tests;

public class WordCounterTests(ITestOutputHelper output)
{
    [Fact]
    public async Task GetWordsStatistic_MbFiles_DifferentApproachesMatch()
    {
        var folder = @"";
        var fullNames = Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories).ToArray();

        var sw = Stopwatch.StartNew();
        var benchmark = await new WordCounterLinqAggregate().GetWordsStatistic(fullNames);
        sw.Stop();
        var elapsedBenchmark = sw.Elapsed;
        
        sw = Stopwatch.StartNew();
        var plinqResult = await new WordCounterPlinqAggregate().GetWordsStatistic(fullNames);
        sw.Stop();
        var elapsedPlinq = sw.Elapsed;
        
        sw = Stopwatch.StartNew();
        var asyncEnumerable = await new WordCounterParallelAsyncEnumerable().GetWordsStatistic(fullNames);
        sw.Stop();
        var elapsedAsyncEnumerable = sw.Elapsed;

        output.WriteLine($"benchmark (linq, single threaded) took {elapsedBenchmark}");
        output.WriteLine($"plinq took {elapsedPlinq}");
        output.WriteLine($"parallel and async enumerable took {elapsedAsyncEnumerable}");

        AssertMatchingStatistic(benchmark, plinqResult);
        AssertMatchingStatistic(benchmark, asyncEnumerable);
    }

    private static void AssertMatchingStatistic(IReadOnlyDictionary<string, int> lhs
        , IReadOnlyDictionary<string, int> rhs)
    {
        var mismatch = new List<string>();
        foreach (var (word, count) in lhs)
        {
            if (rhs.TryGetValue(word, out var rhsCount))
                count.Should().Be(rhsCount);
            else
                mismatch.Add(word);
        }

        mismatch.Should().BeEmpty();
    }
}
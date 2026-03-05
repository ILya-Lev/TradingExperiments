using FluentAssertions;
using static Udemy.Fin.Stat.CoinTossingStateGenerator;

namespace Udemy.Fin.Stat.Tests;

[Trait("Category", "Unit")]
public class CoinTossingStateGeneratorTests(ITestOutputHelper output)
{
    [Fact]
    public void GenerateFairCoinTosses_3_MatchExpectations()
    {
        GenerateFairCoinTosses(3)
            .Should()
            .BeEquivalentTo(["HHH", "HHT", "HTH", "THH", "HTT", "THT", "TTH", "TTT"]);
    }

    [Fact]
    public void GenerateFairCoinTosses_5_Observe()
    {
        var tosses = GenerateFairCoinTosses(5).ToArray();
        tosses.Should().HaveCount(1 << 5, "growth exponentially N = 2^tosses");

        foreach (var toss in tosses)
            output.WriteLine(toss);

        var probabilityFirstTails = tosses.Count(t => t[0] == Tails) * 1.0 / tosses.Length;
        var probabilityFirstHeads = tosses.Count(t => t[0] == Heads) * 1.0 / tosses.Length;
        var probability2n4Heads = tosses
            .Count(t => t[1] == Heads && t[3] == Heads)
            * 1.0 / tosses.Length;
        var probability1n3n5Tails = tosses
            .Count(t => t[0] == Tails && t[2] == Tails && t[4] == Tails) 
            * 1.0 / tosses.Length;
        var probabilityTreeHeadsInARow = tosses.Count(t => t.Has3HeadsInARow()) * 1.0 / tosses.Length;

        output.WriteLine(
            $"p(first tails) = {probabilityFirstTails};" +
            $" p(first heads) = {probabilityFirstHeads};" +
            $" p(3 heads in a row) = {probabilityTreeHeadsInARow};" +
            $" p(2 & 4 heads) = {probability2n4Heads};" +
            $" p(1&3&5 tails) = {probability1n3n5Tails}"
        );
    }

    [Fact]
    public void GenerateBiasedCoinTosses_4Times_Head85_Observe()
    {
        var tosses = GenerateBiasedCoinTosses(4, 0.85).ToArray();
        tosses.Should().HaveCount(1 << 4, "growth exponentially N = 2^tosses");

        foreach (var toss in tosses)
            output.WriteLine(toss);

        var probabilityFirstTails = tosses.Count(t => t[0] == Tails) * 1.0 / tosses.Length;
        var probabilityFirstHeads = tosses.Count(t => t[0] == Heads) * 1.0 / tosses.Length;
        var probability2n4Heads = tosses
            .Count(t => t[1] == Heads && t[3] == Heads)
            * 1.0 / tosses.Length;
        var probabilityTreeHeadsInARow = tosses.Count(t => t.Has3HeadsInARow()) * 1.0 / tosses.Length;

        output.WriteLine(
            $"p(first tails) = {probabilityFirstTails};" +
            $" p(first heads) = {probabilityFirstHeads};" +
            $" p(3 heads in a row) = {probabilityTreeHeadsInARow};" +
            $" p(2 & 4 heads) = {probability2n4Heads};"
        );
    }
}
using System.Text.RegularExpressions;

namespace Udemy.Fin.Stat;

public static partial class CoinTossingStateGenerator
{
    public const char Heads = 'H';
    public const char Tails = 'T';

    [GeneratedRegex("H{3}", RegexOptions.Compiled, 10)]
    private static partial Regex Get3HeadsInARow();

    public static bool Has3HeadsInARow(this ReadOnlySpan<char> tossingState)
        => Get3HeadsInARow().IsMatch(tossingState);

    public static IEnumerable<string> GenerateFairCoinTosses(int counter = 1)
    {
        if (counter <= 1) return [$"{Heads}", $"{Tails}"];

        var previous = GenerateFairCoinTosses(counter - 1).ToArray();
        return
        [
            ..previous.Select(state => $"{state}{Heads}"),
            ..previous.Select(state => $"{state}{Tails}")
        ];
    }

    public static IEnumerable<string> GenerateBiasedCoinTosses(int counter = 1, double pHead = 0.5)
    {
        if (counter <= 1) return [$"{GetToss(pHead)}", $"{GetToss(1 - pHead)}"];

        var previous = GenerateBiasedCoinTosses(counter - 1, pHead).ToArray();
        return
        [
            ..previous.Select(state => $"{state}{GetToss(pHead)}"),
            ..previous.Select(state => $"{state}{GetToss(1 - pHead)}")
        ];
    }

    private static char GetToss(double pHead) => Random.Shared.NextDouble() <= pHead ? Heads : Tails;
}
namespace Udemy.Fin.Stat;

public static class ReturnsCalculator
{
    public static IEnumerable<decimal> CalculateDailyGrossReturns(IEnumerable<decimal> prices)
    {
        decimal? previous = null;
        
        using var iterator = prices.GetEnumerator();
        while (iterator.MoveNext())
        {
            if (previous is null)
            {
                previous = iterator.Current;
                continue;
            }

            yield return DivideZeroGuarded(iterator.Current, previous.Value);
            previous = iterator.Current;
        }
    }

    public static IEnumerable<decimal> CalculateDailyNetReturns(IEnumerable<decimal> prices)
        => CalculateDailyGrossReturns(prices).Select(g => g - 1);

    public static IEnumerable<decimal> CalculateContinuouslyCompoundedDailyReturns(IEnumerable<decimal> prices)
        => CalculateDailyGrossReturns(prices).Select(g => (decimal)Math.Log((double)g));

    public static IEnumerable<decimal> CalculateContinuouslyCompoundedByNetDailyReturns(IEnumerable<decimal> netReturns)
        => netReturns.Select(r => (decimal)Math.Log((double)r + 1.0));

    public static IEnumerable<decimal> ReconstructDailyPricesByNetDailyReturns(IEnumerable<decimal> netReturns,
        decimal startingPrice)
    {
        var price = startingPrice;
        yield return price;

        using var iterator = netReturns.GetEnumerator();
        while (iterator.MoveNext())
        {
            price *= iterator.Current + 1;
            yield return price;
        }
    }

    public static IEnumerable<decimal> CalculateAggregatedGrossReturns(IEnumerable<decimal> prices,
        int aggregationWindow)
    {
        var index = 0;
        var buffer = new decimal[aggregationWindow];
        using var iterator = prices.GetEnumerator();
        while (iterator.MoveNext())
        {
            if (index >= aggregationWindow)
            {
                yield return DivideZeroGuarded(iterator.Current, buffer[index % aggregationWindow]);
            }

            buffer[index % aggregationWindow] = iterator.Current;
            index++;
        }
    }

    public static IEnumerable<decimal> CalculateAggregatedNetReturns(IEnumerable<decimal> prices, int aggregationWindow)
        => CalculateAggregatedGrossReturns(prices, aggregationWindow).Select(g => g - 1);

    public static IEnumerable<decimal> CalculateAggregatedContinuouslyCompoundedReturns(IEnumerable<decimal> prices, int aggregationWindow)
        => CalculateAggregatedGrossReturns(prices, aggregationWindow)
            .Select(g => (decimal)Math.Log((double)g));

    public static IEnumerable<decimal> CalculateAggregatedContinuouslyCompoundedByDailyNetReturns(
        IEnumerable<decimal> netReturns, int aggregationWindow)
    {
        var dailyContinuous = CalculateContinuouslyCompoundedByNetDailyReturns(netReturns);
        var index = 0;
        var windowSum = 0M;
        var buffer = new decimal[aggregationWindow];
        using var iterator = dailyContinuous.GetEnumerator();
        while (iterator.MoveNext())
        {
            if (index >= aggregationWindow)
            {
                yield return windowSum;
                windowSum -= buffer[index % aggregationWindow];
            }

            buffer[index % aggregationWindow] = iterator.Current;
            windowSum += iterator.Current;
            index++;
        }

        if (index >= aggregationWindow)
        {
            yield return windowSum;
        }
    }

    private static decimal DivideZeroGuarded(decimal nominator, decimal denominator)
    {
        if (Math.Abs(denominator) < 1e-10m)
            throw new InvalidOperationException($"current value {nominator}, previous {denominator}");
        
        return nominator / denominator;
    }
}

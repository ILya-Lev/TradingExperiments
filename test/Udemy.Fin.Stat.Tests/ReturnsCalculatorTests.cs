namespace Udemy.Fin.Stat.Tests;

[Trait("Category", "Unit")]
public class ReturnsCalculatorTests(ITestOutputHelper output)
{
    [Fact]
    public void CalculateVariousReturns_SamplePrices_ObserveValue()
    {
        var prices = new[] { 1447m, 1465, 1471, 1453, 1468, 1477, 1482, 1467, 1484, 1474 };
        
        var dailyGross = ReturnsCalculator.CalculateDailyGrossReturns(prices);
        var dailyNet = ReturnsCalculator.CalculateDailyNetReturns(prices);
        var dailyCompounded = ReturnsCalculator.CalculateContinuouslyCompoundedDailyReturns(prices);
        
        var weeklyGross = ReturnsCalculator.CalculateAggregatedGrossReturns(prices, 5);
        var weeklyNet = ReturnsCalculator.CalculateAggregatedNetReturns(prices, 5);
        var weeklyCompounded = ReturnsCalculator.CalculateAggregatedContinuouslyCompoundedReturns(prices, 5);

        output.WriteLine($"daily");
        output.WriteLine($"gross: {string.Join("; ", dailyGross.Select(g => g.ToString("N4")))}");
        output.WriteLine($"net:   {string.Join("; ", dailyNet.Select(r => $"{r*10_000:N0} bp"))}");
        output.WriteLine($"comp:  {string.Join("; ", dailyCompounded.Select(r => $"{r*10_000:N0} bp"))}");
        
        output.WriteLine($"weekly");
        output.WriteLine($"gross: {string.Join("; ", weeklyGross.Select(g => g.ToString("N4")))}");
        output.WriteLine($"net:   {string.Join("; ", weeklyNet.Select(r => $"{r * 10_000:N0} bp"))}");
        output.WriteLine($"comp:  {string.Join("; ", weeklyCompounded.Select(r => $"{r * 10_000:N0} bp"))}");
    }

    [Fact]
    public void RestoreBasedOnNetReturn_Observe()
    {
        var netReturns = new decimal[] { -1.4m,1.3m,0.8m,-0.9m,1.9m,0.7m,1.1m,-0.7m,1.1m,-1.2m }
            .Select(r => r / 100/*convert percents into decimal fraction*/)
            .ToArray();

        var dailyCompounded = ReturnsCalculator.CalculateContinuouslyCompoundedByNetDailyReturns(netReturns);
        var weeklyCompounded = ReturnsCalculator
            .CalculateAggregatedContinuouslyCompoundedByDailyNetReturns(netReturns, 5);
        var prices = ReturnsCalculator.ReconstructDailyPricesByNetDailyReturns(netReturns, 1_000);

        output.WriteLine($"daily compounded: {string.Join("; ", dailyCompounded.Select(r => $"{r*10_000:N0} bp"))}");
        output.WriteLine($"weekly compounded: {string.Join("; ", weeklyCompounded.Select(r => $"{r*10_000:N0} bp"))}");
        output.WriteLine($"prices: {string.Join("; ", prices.Skip(1).Select(p => $"{p:N0}"))}");
    }
}
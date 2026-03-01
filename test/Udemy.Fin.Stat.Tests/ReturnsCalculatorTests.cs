using static Udemy.Fin.Stat.ReturnsCalculator;

namespace Udemy.Fin.Stat.Tests;

[Trait("Category", "Unit")]
public class ReturnsCalculatorTests(ITestOutputHelper output)
{
    [Fact]
    public void CalculateVariousReturns_SamplePrices_ObserveValue()
    {
        var prices = new[] { 1447m, 1465, 1471, 1453, 1468, 1477, 1482, 1467, 1484, 1474 };
        
        var dailyGross = CalculateDailyGrossReturns(prices);
        var dailyNet = CalculateDailyNetReturns(prices);
        var dailyCompounded = CalculateContinuouslyCompoundedDailyReturns(prices);
        
        var weeklyGross = CalculateAggregatedGrossReturns(prices, 5);
        var weeklyNet = CalculateAggregatedNetReturns(prices, 5);
        var weeklyCompounded = CalculateAggregatedContinuouslyCompoundedReturns(prices, 5);

        output.WriteLine($"daily");
        output.WriteLine($"gross: {string.Join("; ", dailyGross.Select(g => g.ToString("N4")))}");
        output.WriteLine($"net:   {string.Join("; ", dailyNet.Select(r => $"{r.ToBp()} bp"))}");
        output.WriteLine($"comp:  {string.Join("; ", dailyCompounded.Select(r => $"{r.ToBp()} bp"))}");
        
        output.WriteLine($"weekly");
        output.WriteLine($"gross: {string.Join("; ", weeklyGross.Select(g => g.ToString("N4")))}");
        output.WriteLine($"net:   {string.Join("; ", weeklyNet.Select(r => $"{r.ToBp()} bp"))}");
        output.WriteLine($"comp:  {string.Join("; ", weeklyCompounded.Select(r => $"{r.ToBp()} bp"))}");
    }

    [Fact]
    public void RestoreBasedOnNetReturn_Observe()
    {
        var netReturns = new decimal[] { -1.4m,1.3m,0.8m,-0.9m,1.9m,0.7m,1.1m,-0.7m,1.1m,-1.2m }
            .Select(r => r / 100/*convert percents into decimal fraction*/)
            .ToArray();

        var dailyCompounded = CalculateContinuouslyCompoundedByNetDailyReturns(netReturns);
        var weeklyCompounded = CalculateAggregatedContinuouslyCompoundedByDailyNetReturns(netReturns, 5);
        var prices = ReconstructDailyPricesByNetDailyReturns(netReturns, 1_000);

        output.WriteLine($"daily compounded: {string.Join("; ", dailyCompounded.Select(r => $"{r.ToBp()} bp"))}");
        output.WriteLine($"weekly compounded: {string.Join("; ", weeklyCompounded.Select(r => $"{r.ToBp()} bp"))}");
        output.WriteLine($"prices: {string.Join("; ", prices.Skip(1).Select(p => $"{p:N0}"))}");
    }

    [Fact]
    public async Task SnP500_ContinuouslyCompounded_DailyNetReturns_Plot()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "GSPC.csv");
        var index = await DataLoader.Load<ExIndex>(path).ToArrayAsync();

        var closeContinuousReturns = CalculateContinuouslyCompoundedDailyReturns(index.Select(i => i.Close));
        var closeDailyNetReturns = CalculateDailyNetReturns(index.Select(i => i.Close));
        var closeDailyGrossReturns = CalculateDailyGrossReturns(index.Select(i => i.Close));

        var data = index
            .Skip(1)
            .Zip(closeDailyGrossReturns, (i, g) => new PricesWithReturns(i.Date, i.Close, g.ToBp(), 0, 0))
            .ToArray();

        data = data.Zip(closeDailyNetReturns, (d, n) => d with { DailyNet = n.ToBp() }).ToArray();
        data = data.Zip(closeContinuousReturns, (d, c) => d with { ContinuouslyCompound = c.ToBp() }).ToArray();

        foreach(var item in data.Take(10))
        {
            output.WriteLine($"{item.Date} {item.ContinuouslyCompound} bp {item.DailyNet} bp");
        }

        var savingTasks = new[]
        {
            Plot("SnP500 close prices.png", data, i => i.Price),
            Plot("SnP500 continuous returns.png", data, i => i.ContinuouslyCompound),
            Plot("SnP500 daily net returns.png", data, i => i.DailyNet),
            Plot("SnP500 continuous returns the 90th.png", data, i => i.ContinuouslyCompound,
                d => d.Year is >= 1990 and < 2000),
            Plot("SnP500 continuous returns dot com.png", data, i => i.ContinuouslyCompound,
                d => d.Year is >= 2000 and < 2002),
            Plot("SnP500 continuous returns the 2001-2007.png", data, i => i.ContinuouslyCompound,
                d => d.Year is >= 2002 and < 2008),
            Plot("SnP500 continuous returns big crisis 2008.png", data, i => i.ContinuouslyCompound,
                d => d.Year is >= 2008 and < 2010),
        };
        await Task.WhenAll(savingTasks);
    }

    private static async Task Plot(string name,
        PricesWithReturns[] data,
        Func<PricesWithReturns, decimal> projection, 
        Func<DateOnly, bool>? filter = null)
    {
        await Task.Yield();
        const int width = 3200, height = 1600;

        var filteredData = filter is not null ? data.Where(i => filter(i.Date)).ToArray() : data;

        var plot = new ScottPlot.Plot();
        plot.Add.Scatter
        (
            filteredData.Select(i => i.Date.ToDateTime(TimeOnly.MinValue)).ToArray(),
            filteredData.Select(projection).ToArray()
        );
        plot.Axes.DateTimeTicksBottom();
        plot.SavePng(name, width, height);
    }
}
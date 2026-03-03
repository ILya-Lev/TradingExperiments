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
        var netReturns = new decimal[] { -1.4m, 1.3m, 0.8m, -0.9m, 1.9m, 0.7m, 1.1m, -0.7m, 1.1m, -1.2m }
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
        var index = await DataLoader.LoadCsv<ExIndex>(path).ToArrayAsync();

        var closeContinuousReturns = CalculateContinuouslyCompoundedDailyReturns(index.Select(i => i.Close));
        var closeDailyNetReturns = CalculateDailyNetReturns(index.Select(i => i.Close));
        var closeDailyGrossReturns = CalculateDailyGrossReturns(index.Select(i => i.Close));

        var data = index
            .Skip(1)
            .Zip(closeDailyGrossReturns, (i, g) => new PricesWithReturns(i.Date, i.Close, g.ToBp(), 0, 0))
            .ToArray();

        data = data.Zip(closeDailyNetReturns, (d, n) => d with { DailyNet = n.ToBp() }).ToArray();
        data = data.Zip(closeContinuousReturns, (d, c) => d with { ContinuouslyCompound = c.ToBp() }).ToArray();

        foreach (var item in data.Take(10))
        {
            output.WriteLine($"{item.Date} {item.ContinuouslyCompound} bp {item.DailyNet} bp");
        }

        var savingTasks = new[]
        {
            Plot("SnP500 close prices.png", data,i => i.Date.ToDateTime(TimeOnly.MinValue), i => i.Price),
            Plot("SnP500 continuous returns.png", data,i => i.Date.ToDateTime(TimeOnly.MinValue), i => i.ContinuouslyCompound),
            Plot("SnP500 daily net returns.png", data, i => i.Date.ToDateTime(TimeOnly.MinValue),i => i.DailyNet),
            Plot("SnP500 continuous returns the 90th.png", data,i => i.Date.ToDateTime(TimeOnly.MinValue), i => i.ContinuouslyCompound,
                i => i.Date.Year is >= 1990 and < 2000),
            Plot("SnP500 continuous returns dot com.png", data,i => i.Date.ToDateTime(TimeOnly.MinValue), i => i.ContinuouslyCompound,
                i => i.Date.Year is >= 2000 and < 2002),
            Plot("SnP500 continuous returns the 2001-2007.png", data,i => i.Date.ToDateTime(TimeOnly.MinValue), i => i.ContinuouslyCompound,
                i => i.Date.Year is >= 2002 and < 2008),
            Plot("SnP500 continuous returns big crisis 2008.png", data,i => i.Date.ToDateTime(TimeOnly.MinValue), i => i.ContinuouslyCompound,
                i => i.Date.Year is >= 2008 and < 2010),
        };
        await Task.WhenAll(savingTasks);
    }

    [Fact]
    public async Task Russell2000_ContinuouslyCompounded_DailyNetReturns_Plot()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "RUT.csv");
        var index = await DataLoader.LoadCsv<ExIndex>(path).ToArrayAsync();

        var closeContinuousReturns = CalculateContinuouslyCompoundedDailyReturns(index.Select(i => i.Close));
        var closeDailyNetReturns = CalculateDailyNetReturns(index.Select(i => i.Close));
        var closeDailyGrossReturns = CalculateDailyGrossReturns(index.Select(i => i.Close));

        var data = index
            .Skip(1)
            .Zip(closeDailyGrossReturns, (i, g) => new PricesWithReturns(i.Date, i.Close, g.ToBp(), 0, 0))
            .ToArray();

        data = data.Zip(closeDailyNetReturns, (d, n) => d with { DailyNet = n.ToBp() }).ToArray();
        data = data.Zip(closeContinuousReturns, (d, c) => d with { ContinuouslyCompound = c.ToBp() }).ToArray();

        foreach (var item in data.Take(10))
        {
            output.WriteLine($"{item.Date} {item.ContinuouslyCompound} bp {item.DailyNet} bp");
        }

        var savingTasks = new[]
        {
            Plot("Russell 2000 close prices.png", data,i => i.Date.ToDateTime(TimeOnly.MinValue), i => i.Price),
            Plot("Russell 2000 continuous returns.png", data,i => i.Date.ToDateTime(TimeOnly.MinValue), i => i.ContinuouslyCompound),
            Plot("Russell 2000 daily net returns.png", data, i => i.Date.ToDateTime(TimeOnly.MinValue),i => i.DailyNet),
            Plot("Russell 2000 continuous returns the 90th.png", data,i => i.Date.ToDateTime(TimeOnly.MinValue), i => i.ContinuouslyCompound,
                i => i.Date.Year is >= 1990 and < 2000),
            Plot("Russell 2000 continuous returns dot com.png", data,i => i.Date.ToDateTime(TimeOnly.MinValue), i => i.ContinuouslyCompound,
                i => i.Date.Year is >= 2000 and < 2002),
            Plot("Russell 2000 continuous returns the 2001-2007.png", data, i => i.Date.ToDateTime(TimeOnly.MinValue),i => i.ContinuouslyCompound,
                i => i.Date.Year is >= 2002 and < 2008),
            Plot("Russell 2000 continuous returns big crisis 2008.png", data, i => i.Date.ToDateTime(TimeOnly.MinValue),i => i.ContinuouslyCompound,
                i => i.Date.Year is >= 2008 and < 2010),
        };
        await Task.WhenAll(savingTasks);
    }

    [Fact]
    public async Task EuroStoxx50_ContinuouslyCompounded_DailyNetReturns_Plot()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "SX5E.csv");
        var index = await DataLoader.LoadCsv<ExOhlc>(path).ToArrayAsync();

        var closeContinuousReturns = CalculateContinuouslyCompoundedDailyReturns(index.Select(i => i.Close));
        var closeDailyNetReturns = CalculateDailyNetReturns(index.Select(i => i.Close));
        var closeDailyGrossReturns = CalculateDailyGrossReturns(index.Select(i => i.Close));

        var data = index
            .Skip(1)
            .Zip(closeDailyGrossReturns, (i, g) => new PricesWithReturns(i.Date, i.Close, g.ToBp(), 0, 0))
            .ToArray();

        data = data.Zip(closeDailyNetReturns, (d, n) => d with { DailyNet = n.ToBp() }).ToArray();
        data = data.Zip(closeContinuousReturns, (d, c) => d with { ContinuouslyCompound = c.ToBp() }).ToArray();

        foreach (var item in data.Take(10))
        {
            output.WriteLine($"{item.Date} {item.ContinuouslyCompound} bp {item.DailyNet} bp");
        }

        var savingTasks = new[]
        {
            Plot("EuroStoxx50 close prices.png", data, i => i.Date.ToDateTime(TimeOnly.MinValue), i => i.Price),
            Plot("EuroStoxx50 continuous returns.png", data, i => i.Date.ToDateTime(TimeOnly.MinValue),i => i.ContinuouslyCompound),
            Plot("EuroStoxx50 daily net returns.png", data, i => i.Date.ToDateTime(TimeOnly.MinValue),i => i.DailyNet),
            Plot("EuroStoxx50 continuous returns the 90th.png", data,i => i.Date.ToDateTime(TimeOnly.MinValue), i => i.ContinuouslyCompound,
                i => i.Date.Year is >= 1990 and < 2000),
            Plot("EuroStoxx50 continuous returns dot com.png", data, i => i.Date.ToDateTime(TimeOnly.MinValue),i => i.ContinuouslyCompound,
                i => i.Date.Year is >= 2000 and < 2002),
            Plot("EuroStoxx50 continuous returns the 2001-2007.png", data,i => i.Date.ToDateTime(TimeOnly.MinValue), i => i.ContinuouslyCompound,
                i => i.Date.Year is >= 2002 and < 2008),
            Plot("EuroStoxx50 continuous returns big crisis 2008.png", data, i => i.Date.ToDateTime(TimeOnly.MinValue),i => i.ContinuouslyCompound,
                i => i.Date.Year is >= 2008 and < 2010),
        };
        await Task.WhenAll(savingTasks);
    }

    [Theory]
    [InlineData("GSPC")]
    [InlineData("RUT")]
    public async Task Sharpe_ForIndexes_Plot(string fileName)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "TestData", $"{fileName}.parquet");
        var index = await DataLoader.LoadParquet<ExIndex>(path).ToArrayAsync();

        var yearlySharpe = SharpeCalculator.CalculateDailySharpe(index.Select(i => i.Close), 250);
        var quarterlySharpe = SharpeCalculator.CalculateDailySharpe(index.Select(i => i.Close), 62);

        var yearlySharpeWithDate = index.Skip(250).Zip(yearlySharpe, (i, y) => new PricesWithReturns(i.Date, i.Close, y, 0, 0)).ToArray();
        var quarterlySharpeWithDate = index.Skip(62).Zip(quarterlySharpe, (i, y) => new PricesWithReturns(i.Date, i.Close, y, 0, 0)).ToArray();

        var savingTasks = new[]
        {
            Plot($"{fileName} yearly Sharpe ratio.png", yearlySharpeWithDate, i => i.Date.ToDateTime(TimeOnly.MinValue), i => i.DailyGross),
            Plot($"{fileName} quarterly Sharpe ratio.png", yearlySharpeWithDate, i => i.Date.ToDateTime(TimeOnly.MinValue), i => i.DailyGross),
        };
        await Task.WhenAll(savingTasks);
    }

    private static async Task Plot<T>(string name,
        T[] data,
        Func<T, DateTime> projectionX,
        Func<T, decimal> projectionY,
        Func<T, bool>? filter = null)
    {
        await Task.Yield();
        const int width = 3200, height = 1600;

        var filteredData = filter is not null ? data.Where(filter).ToArray() : data;

        var plot = new ScottPlot.Plot();
        plot.Add.Scatter
        (
            filteredData.Select(projectionX).ToArray(),
            filteredData.Select(projectionY).ToArray()
        );
        plot.Axes.DateTimeTicksBottom();
        plot.SavePng(name, width, height);
    }
}
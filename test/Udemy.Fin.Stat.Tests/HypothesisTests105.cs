namespace Udemy.Fin.Stat.Tests;

[Trait("Category", "Integration")]
public class HypothesisTests105(ITestOutputHelper output)
{
    [Theory]
    [InlineData("19830101", "19860831")]
    [InlineData("19930101", "19970831")]
    [InlineData("20010101", "20060831")]
    public async Task WhiteNoiseTest_StablePeriods_Pass(string fromDate, string toDate)
    {
        var from = DateOnly.ParseExact(fromDate, "yyyyMMdd");
        var to = DateOnly.ParseExact(toDate, "yyyyMMdd");
        var source = $"S&P500 {from}-{to}";
        var filter = (DateOnly d) => from <= d && d <= to;
        
        var closePrices = await LoadData("GSPC", filter).ContinueWith(t => t.Result.ToArray());
        
        var logReturns = closePrices.Skip(1).Zip(closePrices.SkipLast(1), (n, c) => Math.Log(n) - Math.Log(c)).ToArray();
        var logReturnsInfo = DemoHelpers.PlotSeries(source, "log returns", logReturns);
        output.WriteLine(logReturnsInfo.Path);

        var logRetAutoCor = logReturns.GetAutoCorrelationParallel(50);
        var logRetAutoCorInfo = DemoHelpers.PlotSeries(source, "auto corr", logRetAutoCor);
        output.WriteLine(logRetAutoCorInfo.Path);

        var (qStat, lbValue) = logReturns.Select(r => (decimal)r).ToArray().GetLjungBoxTestValues(50);
        output.WriteLine($"Ljung-Box test statistic {qStat:N4}; p-value {lbValue:N4}");

        var squaredLogRet = logReturns.Select(r => r * r).ToArray();
        var squaredLogReturnsInfo = DemoHelpers.PlotSeries(source, "squared log returns", squaredLogRet);
        output.WriteLine(squaredLogReturnsInfo.Path);

        var squaredLogRetAutoCor = squaredLogRet.GetAutoCorrelationParallel(50);
        var squaredLogRetAutoCorInfo = DemoHelpers.PlotSeries(source, "squared auto corr", squaredLogRetAutoCor);
        output.WriteLine(squaredLogRetAutoCorInfo.Path);

        var (squaredQStat, mlValue) = squaredLogRet.Select(r => (decimal)r).ToArray().GetLjungBoxTestValues(50);
        output.WriteLine($"Mcleod-Li test statistic {squaredQStat:N4}; p-value {mlValue:N4}");

    }

    private static async Task<IEnumerable<double>> LoadData(string file, Func<DateOnly, bool>? dateFilter = null)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "TestData", $"{file}.parquet");
        try
        {
            return await DataLoader.LoadParquet<ExIndex>(path)
                .ContinueWith(t => t.Result
                    .Where(p => dateFilter?.Invoke(p.Date) ?? true)
                    .Select(p => (double)p.Close));
        }
        catch (Exception exc)
        {
            //motivation: file contains either ExIndex or ExOhlc => if the first fails, try with the last one.
            return await DataLoader.LoadParquet<ExOhlc>(path)
                .ContinueWith(t => t.Result
                    .Where(p => dateFilter?.Invoke(p.Date) ?? true)
                    .Select(p => (double)p.Close));
        }
    }

}
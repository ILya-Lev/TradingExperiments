using System.Numerics.Tensors;
using FluentAssertions;
using FluentAssertions.Execution;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using Parquet;

namespace Udemy.Fin.Stat.Tests;

[Trait("Category", "Integration")]
public class ProblemSet082(ITestOutputHelper output)
{
    [Theory]
    [InlineData("GSPC", "S&P 500")]
    [InlineData("RUT", "Russell2000")]
    [InlineData("SX5E", "STOXX500")]
    public async Task ClosePrices_SampleMean_Autocorrelation_Plot(string file, string indexName)
    {
        var closeSeries = await LoadData(file).ContinueWith(t => t.Result.ToArray());
        var logReturns = closeSeries.Skip(1).Zip(closeSeries.SkipLast(1), (n, p) => Math.Log(n / p) * 100).ToArray();

        var series = logReturns;
        var squareSeries = logReturns.Select(c => c * c).ToArray();
        var absSeries = logReturns.Select(Math.Abs).ToArray();

        var mean = TensorPrimitives.Sum(series) / series.Length;
        var variance = series.GetSampleVariance(mean);

        var acLogRet = series.GetAutoCorrelationParallel(series.Length - 1);
        var acSquareRet = squareSeries.GetAutoCorrelationParallel(squareSeries.Length - 1);
        var acAbs = absSeries.GetAutoCorrelationParallel(absSeries.Length - 1);

        output.WriteLine($"sample mean {mean}, variance {variance}, std dev {Math.Sqrt(variance)}");
        output.WriteLine($"auto correlation log ret 0 {acLogRet[0]}, 1 {acLogRet[1]}, 2 {acLogRet[2]}");
        output.WriteLine($"auto correlation square ret 0 {acSquareRet[0]}, 1 {acSquareRet[1]}, 2 {acSquareRet[2]}");
        output.WriteLine($"auto correlation abs close 0 {acAbs[0]}, 1 {acAbs[1]}, 2 {acAbs[2]}");

        var plotSeriesInfo = DemoHelpers.PlotSeries(indexName, "log returns", series, false);
        DemoHelpers.PlotSeries($"{indexName} log ret auto corr", "auto correlation", acLogRet, false);
        DemoHelpers.PlotSeries($"{indexName} squared ret auto corr", "auto correlation", acSquareRet, false);
        DemoHelpers.PlotSeries($"{indexName} abs auto corr", "auto correlation", acAbs, false);
        output.WriteLine($"plotted {plotSeriesInfo.Path}");
    }

    /// <summary>
    /// before 1957-03-15 (inclusively) it was SnP90 as of 90 companies. After became SnP500 of 500 companies.
    /// </summary>
    [Theory]
    [InlineData("", "1957-03-15")]
    [InlineData("1957-03-16", "")]
    [InlineData("1991-07-01", "1996-12-31")]
    public async Task ClosePrices_Autocorrelation_SnpSubset_Plot(string startDate, string endDate)
    {
        var fromDate = string.IsNullOrWhiteSpace(startDate) ? DateOnly.MinValue : DateOnly.Parse(startDate);
        var toDate = string.IsNullOrWhiteSpace(endDate) ? DateOnly.MaxValue : DateOnly.Parse(endDate);
        var dateFilter = (DateOnly d) => fromDate <= d && d <= toDate;

        var indexName = "S&P500";
        var file = "GSPC";

        var closeSeries = await LoadData(file, dateFilter).ContinueWith(t => t.Result.ToArray());

        //var logReturns = closeSeries.Skip(1).Zip(closeSeries.SkipLast(1), (n, p) => Math.Log(n / p) * 100).ToArray();
        var returns = closeSeries.Skip(1).Zip(closeSeries.SkipLast(1), (n, p) => n - p).ToArray();
        var absReturns = returns.Select(Math.Abs).ToArray();
        var acAbs = absReturns.GetAutoCorrelationParallel(absReturns.Length - 1);

        DemoHelpers.PlotSeries($"{indexName} returns {startDate}--{endDate}", "returns", returns, false);
        DemoHelpers.PlotSeries($"{indexName} abs ret auto corr {startDate}--{endDate}", "auto correlation", acAbs, false);
    }

    //lecture 86 - geometric random walk - parameter calibration for Bernoulli model
    //P(t) = P(0) * exp(S(t)); S(t) = sum (r(i), i=1..t); r(t) = a + b * X(t), where X(t) ~ Bern(0.5)
    //we're figuring out a and b from E(r) and Var(r); E(r) = a + b/2; Var(r) = b*b/4;
    [Theory]
    //[InlineData("1991-07-01", "1996-12-31")]//mid 90s
    [InlineData("1992-01-01", "1996-12-31")]//mid 90s
    [InlineData("2003-07-01", "2006-12-31")]//mid 2000s
    public async Task GeometricRandomWalk_Bernoulli_Calibrate(string startDate, string endDate)
    {
        var fromDate = string.IsNullOrWhiteSpace(startDate) ? DateOnly.MinValue : DateOnly.Parse(startDate);
        var toDate = string.IsNullOrWhiteSpace(endDate) ? DateOnly.MaxValue : DateOnly.Parse(endDate);
        var dateFilter = (DateOnly d) => fromDate <= d && d <= toDate;

        var indexName = "S&P500";
        var file = "GSPC";

        var closeSeries = await LoadData(file, dateFilter).ContinueWith(t => t.Result.ToArray());

        var logReturns = closeSeries.Skip(1).Zip(closeSeries.SkipLast(1), (n, p) => Math.Log(n / p)).ToArray();

        var logReturnsStatistics = new DescriptiveStatistics(logReturns);
        var volatility = logReturnsStatistics.StandardDeviation;
        var b = 2 * volatility;
        var a = logReturnsStatistics.Mean - volatility;//drift - volatility

        var bernoulliSample = new Bernoulli(0.5).Samples().Take(1_000);
        var modeledReturns = bernoulliSample.Select(brn => a + b * brn);
        var modeledSeries = GetCumulativeSum(modeledReturns).Select(s => closeSeries[0] * Math.Exp(s)).ToArray();

        var p1 = DemoHelpers.PlotSeries($"{indexName} {startDate}--{endDate}", "prices", closeSeries, false);
        var p2 = DemoHelpers.PlotSeries($"geometric random walk {indexName} returns {startDate}--{endDate}", "geometric random walk", modeledSeries, false);

        output.WriteLine($"plotted\n {p1.Path}\n\n and \n{p2.Path}\n\n");

        //using var _ = new AssertionScope();
        //a.Should().BeApproximately(-0.005630, 1e-6);
        //b.Should().BeApproximately(0.012168, 1e-6);
        output.WriteLine($"a = {a:N6}; b = {b:N6}");//a = -0.5816; b = 1.2598 1991-07-01; a = -0.005630; b = 0.012168 1992-01-01
    }

    //lecture 87 - geometric random walk - parameter calibration for LogNormal model
    //P(t) = P(0) * exp(S(t)); S(t) = sum (r(i), i=1..t); r(t) ~ N(mu, sigma)
    //X(t) = exp(S(t)) itself is a LogNormal(t*mu, sqrt(t)*sigma))
    //E(X(t)) = exp(mu + sigma/2)
    //Var(X(t)) = exp(2*mu + sigma^2) * (exp(sigma^2) - 1)
    [Theory]
    //[InlineData("1991-07-01", "1996-12-31")]//mid 90s
    //[InlineData("1992-01-01", "1996-12-31")]//mid 90s
    [InlineData("2003-07-01", "2006-12-31")]//mid 2000s
    public async Task GeometricRandomWalk_LogNormal_Calibrate(string startDate, string endDate)
    {
        var fromDate = string.IsNullOrWhiteSpace(startDate) ? DateOnly.MinValue : DateOnly.Parse(startDate);
        var toDate = string.IsNullOrWhiteSpace(endDate) ? DateOnly.MaxValue : DateOnly.Parse(endDate);
        var dateFilter = (DateOnly d) => fromDate <= d && d <= toDate;

        var indexName = "S&P500";
        var file = "GSPC";

        var closeSeries = await LoadData(file, dateFilter).ContinueWith(t => t.Result.ToArray());

        var logReturns = closeSeries.Skip(1).Zip(closeSeries.SkipLast(1), (n, p) => Math.Log(n / p)).ToArray();

        var logReturnsStatistics = new DescriptiveStatistics(logReturns);
        var drift = logReturnsStatistics.Mean;
        var volatility = logReturnsStatistics.StandardDeviation;

        var trend = Enumerable.Range(1, closeSeries.Length).Select(t => drift * t);
        var randomWalk = AutoRegressiveMovingAverageProcess.GenerateArma([1.0], [], volatility, 0)
            .Take(closeSeries.Length)
            .ToArray();
        
        var lognormalRandomWalk = new double[closeSeries.Length];
        new LogNormal(drift, volatility).Samples(lognormalRandomWalk);

        var modeledSeries = randomWalk.Zip(trend, (rw, t) => closeSeries[0] * Math.Exp(t + rw)).ToArray();

        var autoCorrelation = logReturns.GetAutoCorrelationParallel(100);
        var correlation = closeSeries.CorrelationFunction(modeledSeries);

        var squaredLogReturns = new Span<double>(new double[logReturns.Length]);
        TensorPrimitives.Multiply(logReturns, logReturns, squaredLogReturns);
        var autoCorrelationOfSquared = squaredLogReturns.GetAutoCorrelationParallel(100);

        var maxLags = logReturns.Length / 20;//the rule of thumb - use up to 5% of the sample data for Ljung-Box
        var (qStatDirect, pValueDirect) = logReturns.Select(r => (decimal)r).ToArray().GetLjungBoxTestValues(maxLags);
        var (qStat, pValue) = autoCorrelation.Take(maxLags + 1 /*as we skip #1 - always = to 1*/)
            .Select(r => (decimal)r).ToArray().GetLjungBoxTestValuesFromAcf(logReturns.Length);

        qStat.Should().BeApproximately(qStatDirect, 2e-2m);
        pValue.Should().BeApproximately(pValueDirect, 2e-2m);

        var p1 = DemoHelpers.PlotSeries($"{indexName} {startDate}--{endDate}", "prices", closeSeries, false);
        var p2 = DemoHelpers.PlotSeries($"geometric random walk {indexName} returns {startDate}--{endDate}", "lognormal", modeledSeries, false);
        var p3 = DemoHelpers.PlotSeries($"geometric random walk {indexName} autocorrelation {startDate}--{endDate}", "autocorrelation", autoCorrelation, false);
        var p4 = DemoHelpers.PlotSeries($"geometric random walk {indexName} autocorrelation or square {startDate}--{endDate}", "autocorrelation of square", autoCorrelationOfSquared, false);

        output.WriteLine($"correlation {correlation}; Ljung-Box qStat {qStat}; pValue {pValue}");
        output.WriteLine($"plotted\n {p1.Path}\n\n and \n{p2.Path}\n\n and \n{p3.Path}\n\n and \n{p4.Path}\n\n");
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

    private static IEnumerable<double> GetLogarithmicReturns(IEnumerable<double> prices)
    {
        using var iterator = prices.GetEnumerator();
        if (!iterator.MoveNext())
            yield break;

        var previous = iterator.Current;
        while (iterator.MoveNext())
        {
            var ratio = iterator.Current / previous;
            if (ratio < 0 || ratio == 0 || !double.IsFinite(ratio))
                continue;

            yield return 100 * Math.Log(ratio);
            previous = iterator.Current;
        }
    }

    private static IEnumerable<double> GetCumulativeSum(IEnumerable<double> numbers)
    {
        var sumSoFar = 0.0;
        foreach (var number in numbers)
        {
            sumSoFar += number;
            yield return sumSoFar;
        }
    }
}
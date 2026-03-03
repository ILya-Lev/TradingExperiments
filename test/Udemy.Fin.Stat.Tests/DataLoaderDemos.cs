using FluentAssertions;
using MathNet.Numerics.Statistics;

namespace Udemy.Fin.Stat.Tests;

[Trait("Category", "Unit")]
public class DataLoaderDemos(ITestOutputHelper output)
{
    private void PrintStatReport(DescriptiveStatistics stat) => output.WriteLine
    (
        $"\nmin {stat.Minimum}" +
        $"\nmax {stat.Maximum}" +
        $"\naverage {stat.Mean}" +
        $"\nvariance {stat.Variance}" +
        $"\nstandard deviation {stat.StandardDeviation}" +
        $"\nkurtosis {stat.Kurtosis}" +
        $"\nskewness {stat.Skewness}"
    );

    [Fact]
    public async Task Load_EurUsd_Observe()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "EURUSD.csv");
        var rates = await DataLoader.LoadCsv<FxRate>(path).ToArrayAsync();

        output.WriteLine(
            $"have loaded {rates.Length} rates" +
            $"\n from {rates.MinBy(r => r.Date)}" +
            $"\n to {rates.MaxBy(r => r.Date)}");

        var stat = new DescriptiveStatistics(rates.Select(r => (double)r.Rate));
        PrintStatReport(stat);
    }

    [Fact]
    public async Task Load_SnP500_Observe()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "GSPC.csv");
        var index = await DataLoader.LoadCsv<ExIndex>(path).ToArrayAsync();

        output.WriteLine(
            $"have loaded {index.Length} S&P500 indexes" +
            $"\n from {index.MinBy(r => r.Date)}" +
            $"\n to {index.MaxBy(r => r.Date)}");

        var closeStat = new DescriptiveStatistics(index.Select(r => (double)r.Close));
        PrintStatReport(closeStat);
    }

    [Fact]
    public async Task Load_Russel2000_Observe()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "RUT.csv");
        var index = await DataLoader.LoadCsv<ExIndex>(path).ToArrayAsync();

        output.WriteLine(
            $"have loaded {index.Length} Russel 2000 indexes" +
            $"\n from {index.MinBy(r => r.Date)}" +
            $"\n to {index.MaxBy(r => r.Date)}");

        var closeStat = new DescriptiveStatistics(index.Select(r => (double)r.Close));
        PrintStatReport(closeStat);
    }

    [Fact]
    public async Task Load_Eurostoxx50_Observe()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "SX5E.csv");
        var index = await DataLoader.LoadCsv<ExOhlc>(path).ToArrayAsync();

        output.WriteLine(
            $"have loaded {index.Length} Eurostoxx 50 indexes" +
            $"\n from {index.MinBy(r => r.Date)}" +
            $"\n to {index.MaxBy(r => r.Date)}");

        var closeStat = new DescriptiveStatistics(index.Select(r => (double)r.Close));
        PrintStatReport(closeStat);
    }
    
    [Fact]
    public async Task Eurostoxx50_LoadCsv_WriteParquet_LoadParquet_MatchItself()
    {
        var pathCsv = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "SX5E.csv");
        var pathParquet = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "SX5E.parquet");
        
        var index = await DataLoader.LoadCsv<ExOhlc>(pathCsv).ToArrayAsync();
        await DataLoader.DumpParquet(index, pathParquet);
        var readIndex = await DataLoader.LoadParquet<ExOhlc>(pathParquet).ToArrayAsync();

        index.Should().BeEquivalentTo(readIndex);
    }

    [Fact]
    public async Task Russell2000_LoadCsv_WriteParquet_LoadParquet_MatchItself()
    {
        var pathCsv = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "RUT.csv");
        var pathParquet = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "RUT.parquet");
        
        var index = await DataLoader.LoadCsv<ExIndex>(pathCsv).ToArrayAsync();
        await DataLoader.DumpParquet(index, pathParquet);
        var readIndex = await DataLoader.LoadParquet<ExIndex>(pathParquet).ToArrayAsync();

        index.Should().BeEquivalentTo(readIndex);
    }

    [Fact]
    public async Task SnP500_LoadCsv_WriteParquet_LoadParquet_MatchItself()
    {
        var pathCsv = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "GSPC.csv");
        var pathParquet = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "GSPC.parquet");
        
        var index = await DataLoader.LoadCsv<ExIndex>(pathCsv).ToArrayAsync();
        await DataLoader.DumpParquet(index, pathParquet);
        var readIndex = await DataLoader.LoadParquet<ExIndex>(pathParquet).ToArrayAsync();

        index.Should().BeEquivalentTo(readIndex);
    }

    [Fact]
    public async Task EurUsd_LoadCsv_WriteParquet_LoadParquet_MatchItself()
    {
        var pathCsv = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "EurUsd.csv");
        var pathParquet = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "EurUsd.parquet");
        
        var index = await DataLoader.LoadCsv<FxRate>(pathCsv).ToArrayAsync();
        await DataLoader.DumpParquet(index, pathParquet);
        var readIndex = await DataLoader.LoadParquet<FxRate>(pathParquet).ToArrayAsync();

        index.Should().BeEquivalentTo(readIndex);
    }
}


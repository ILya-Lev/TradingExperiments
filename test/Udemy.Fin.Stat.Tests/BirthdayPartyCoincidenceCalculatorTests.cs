using FluentAssertions;

namespace Udemy.Fin.Stat.Tests;

[Trait("Category", "Unit")]
public class BirthdayPartyCoincidenceCalculatorTests(ITestOutputHelper output)
{
    [Fact]
    public void Generate2OrMoreSameDay_65_Observe()
    {
        BirthdayPartyCoincidenceCalculator.Calculate2OrMoreSameDay(65)
            .Should().BeApproximately(0.99768, 1e-5);
    }

    [Fact]
    public void Generate2OrMoreSameDay_50_Observe()
    {
        BirthdayPartyCoincidenceCalculator.Calculate2OrMoreSameDay(50)
            .Should().BeApproximately(0.97, 1e-3);
    }

    [Fact]
    public void Generate2OrMoreSameDay_30_Observe()
    {
        BirthdayPartyCoincidenceCalculator.Calculate2OrMoreSameDay(30)
            .Should().BeApproximately(0.706, 1e-3);
    }

    [Fact]
    public async Task Generate2OrMoreSameDaySeries_50_Observe()
    {
        var series = BirthdayPartyCoincidenceCalculator
            .Generate2OrMoreSameDaySeries(350)
            .ToArray();
        
        series.Should().NotBeEmpty();
        await Plot("at least 2 bdays at the same day.png", series);
    }

    [Fact]
    public async Task Generate2OrMoreSameDaySeriesFast_65_Observe()
    {
        var series = BirthdayPartyCoincidenceCalculator
            .Generate2OrMoreSameDaySeriesFast(65)
            .ToArray();
        
        series.Should().NotBeEmpty();
        await Plot("at least 2 bdays at the same day fast.png", series);
    }

    private static async Task Plot<T>(string name,
        T[] data,
        Func<T, bool>? filter = null)
    {
        await Task.Yield();
        const int width = 3200, height = 1600;

        var filteredData = filter is not null ? data.Where(filter).ToArray() : data;

        var plot = new ScottPlot.Plot();
        plot.Add.Scatter
        (
            filteredData.Select((_, i) => i).ToArray(),
            filteredData
        );
        plot.SavePng(name, width, height);
    }
}
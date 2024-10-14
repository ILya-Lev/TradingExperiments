using Xunit.Abstractions;

namespace Sudoku.Tests;

[Trait("Category", "Unit")]
public class BinaryGameFrequencyFinderTests(ITestOutputHelper output)
{
    [Fact]
    public void BuildFrequencyReport_Observe()
    {
        var report = BinaryGameFrequencyFinder.BuildFrequencyReport(1, 100);
        foreach (var row in report)
        {
            output.WriteLine(row);
        }
    }
}
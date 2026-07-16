using MathNet.Numerics.Distributions;

namespace Udemy.Fin.Stat.Tests;

[Trait("Category", "Unit")]
public class JarqueBeraTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData("20030701", "20061231")]
    [InlineData("19920101", "19961231")]
    [InlineData("19830101", "19860731")]
    public async Task GetPoints_SnP500_VsNormal(string startStr, string endStr)
    {
        var startDate = DateOnly.ParseExact(startStr, "yyyyMMdd");
        var endDate = DateOnly.ParseExact(endStr, "yyyyMMdd");
        var filter = (DateOnly d) => startDate <= d && d <= endDate;

        var sample = await DemoHelpers.LoadClosePrices("GSPC", filter).ContinueWith(t => t.Result.ToArray());
        var logReturns = sample.Skip(1).Zip(sample.SkipLast(1), (n, c) => Math.Log(n) - Math.Log(c)).ToArray();

        //maximum likelihood estimators - dlnf/dmu = 0 and dlnf/dvariance = 0 where f = product (f_i, i=1..n)
        var mean = logReturns.Average();
        var variance = logReturns.Average(r => (r - mean) * (r - mean));

        var stats = new MathNet.Numerics.Statistics.DescriptiveStatistics(logReturns, true);
        var skew = stats.Skewness;
        var excessiveKurt = stats.Kurtosis;
        var jb = logReturns.Length / 6.0 * (skew * skew + excessiveKurt * excessiveKurt / 4);

        var chi2 = ChiSquared.CDF(2, jb);
        
        output.WriteLine(
            $"""
             S&P500 in date range {startStr} -- {endStr}
             sample mean {mean:g4}, variance {variance:g4}, skewness {skew:g4}, kurtosis {excessiveKurt + 3:g4}
             Jarque-Bera test {jb}
             Chi-Squared(2) for jb {chi2} - the probability of JB test tobe less than {jb:g2} if the underlying dist is normal
             p-value {1-chi2}
             """
            );
    }
}
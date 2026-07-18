using System.Data;
using MathNet.Numerics.Distributions;

namespace Udemy.Fin.Stat.Tests;

[Trait("Category", "Unit")]
public class StudentTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData("20030701", "20061231")]
    [InlineData("19920101", "19961231")]
    [InlineData("19830101", "19860731")]
    public async Task Mom_SnP500_VsStudent(string startStr, string endStr)
    {
        var startDate = DateOnly.ParseExact(startStr, "yyyyMMdd");
        var endDate = DateOnly.ParseExact(endStr, "yyyyMMdd");
        var filter = (DateOnly d) => startDate <= d && d <= endDate;

        var sample = await DemoHelpers.LoadClosePrices("GSPC", filter).ContinueWith(t => t.Result.ToArray());
        var logReturns = sample.Skip(1).Zip(sample.SkipLast(1), (n, c) => Math.Log(n) - Math.Log(c)).ToArray();

        //mom = method of moments
        //mu = average; pho = scale = std dev = sqrt (var * kurt / (2*kurt - 3)); nu = 4+6/(kurt - 3)
        //mu - location, pho - scale, nu - degrees of freedom, type of double
        var stats = new MathNet.Numerics.Statistics.DescriptiveStatistics(logReturns, true);
        var mu = stats.Mean;
        var variance = stats.Variance;
        var skew = stats.Skewness;
        var kurt = stats.Kurtosis + 3;

        var pho = Math.Sqrt(variance * kurt / (2 * kurt - 3));
        var nu = 4 + 6 / (kurt - 3);
        
        var students = StudentT.CDF(mu, pho, nu, 1.2);
        
        output.WriteLine(
            $"""
             S&P500 in date range {startStr} -- {endStr}
             sample mean {mu:g4}, variance {variance:g4}, skewness {skew:g4}, kurtosis {kurt:g4}
             method of moments estimation: location {mu:g4}, scale {pho:g4}, freedom {nu:g4}
             Students(10) p-value {1-students}
             """
        );
    }
    [Theory]
    [InlineData("20030701", "20061231")]
    [InlineData("19920101", "19961231")]
    [InlineData("19830101", "19860731")]
    public async Task Mle_SnP500_VsStudent(string startStr, string endStr)
    {
        var startDate = DateOnly.ParseExact(startStr, "yyyyMMdd");
        var endDate = DateOnly.ParseExact(endStr, "yyyyMMdd");
        var filter = (DateOnly d) => startDate <= d && d <= endDate;

        var sample = await DemoHelpers.LoadClosePrices("GSPC", filter).ContinueWith(t => t.Result.ToArray());
        var logReturns = sample.Skip(1).Zip(sample.SkipLast(1), (n, c) => Math.Log(n) - Math.Log(c)).ToArray();

        //mom = method of moments
        //mu = average; pho = scale = std dev = sqrt (var * kurt / (2*kurt - 3)); nu = 4+6/(kurt - 3)
        //mu - location, pho - scale, nu - degrees of freedom, type of double
        var stats = new MathNet.Numerics.Statistics.DescriptiveStatistics(logReturns, true);
        var mu = stats.Mean;
        var variance = stats.Variance;
        var skew = stats.Skewness;
        var kurt = stats.Kurtosis + 3;

        var pho = Math.Sqrt(variance * kurt / (2 * kurt - 3));
        var nu = 4 + 6 / (kurt - 3);

        //mle = maximum likelihood estimators
        //d (ln(L(mu, rho, nu)))/dmu = 0 etc (by rho and nu).
        Estimation.MleStudentT(new DataSet())

        var students = StudentT.CDF(mu, pho, nu, 1.2);
        
        output.WriteLine(
            $"""
             S&P500 in date range {startStr} -- {endStr}
             sample mean {mu:g4}, variance {variance:g4}, skewness {skew:g4}, kurtosis {kurt:g4}
             method of moments estimation: location {mu:g4}, scale {pho:g4}, freedom {nu:g4}
             Students(10) p-value {1-students}
             """
        );
    }
}
using MathNet.Numerics.Distributions;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Sinks.XUnit3;

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
        //mu = average; rho = scale = std dev = sqrt (var * kurt / (2*kurt - 3)); nu = 4+6/(kurt - 3)
        //mu - location, rho - scale, nu - degrees of freedom, type of double
        var stats = new MathNet.Numerics.Statistics.DescriptiveStatistics(logReturns, true);
        var mu = stats.Mean;
        var variance = stats.Variance;
        var skew = stats.Skewness;
        var kurt = stats.Kurtosis + 3;

        var rho = Math.Sqrt(variance * kurt / (2 * kurt - 3));
        var nu = 4 + 6 / (kurt - 3);
        
        var students = StudentT.CDF(mu, rho, nu, 1.2);
        
        output.WriteLine(
            $"""
             S&P500 in date range {startStr} -- {endStr}
             sample mean {mu:g4}, variance {variance:g4}, skewness {skew:g4}, kurtosis {kurt:g4}
             method of moments estimation: location {mu:g4}, scale {rho:g4}, freedom {nu:g4}
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
        //mu = average; rho = scale = std dev = sqrt (var * kurt / (2*kurt - 3)); nu = 4+6/(kurt - 3)
        //mu - location, rho - scale, nu - degrees of freedom, type of double
        var stats = new MathNet.Numerics.Statistics.DescriptiveStatistics(logReturns, true);
        var mu = stats.Mean;
        var variance = stats.Variance;
        var skew = stats.Skewness;
        var kurt = stats.Kurtosis + 3;

        var rho = Math.Sqrt(variance * kurt / (2 * kurt - 3));
        var nu = 4 + 6 / (kurt - 3);

        //mle = maximum likelihood estimators
        //d (ln(L(mu, rho, nu)))/dmu = 0 etc (by rho and nu).
        await using var logger = new LoggerConfiguration().WriteTo.XUnit3TestOutput().CreateLogger();
        using ILoggerFactory loggerFactory = new SerilogLoggerFactory(logger);
        var mleCalculatorLogger = loggerFactory.CreateLogger<MleCalculator>();

        var studentsParameters = new StudentParametersBuilder().RegisterMu(mu).RegisterRho(rho).RegisterNu(nu).Build();
        var mle = new MleCalculator(mleCalculatorLogger).MleStudentT(logReturns, studentsParameters);

        //var students = StudentT.CDF(mu, pho, nu, 1.2);
        
        output.WriteLine(
            $"""
             S&P500 in date range {startStr} -- {endStr}
             sample mean {mu:g4}, variance {variance:g4}, skewness {skew:g4}, kurtosis {kurt:g4}
             method of moments estimation:  location {mu:g4}, scale {rho:g4}, freedom {nu:g4}
             maximum likelihood estimation: location {mle.Mu:g4}, scale {mle.Rho:g4}, freedom {mle.Nu:g4}
             """
        );
    }
}
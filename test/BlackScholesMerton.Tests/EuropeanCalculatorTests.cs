using FluentAssertions;
using FluentAssertions.Execution;
using Xunit.Abstractions;

namespace BlackScholesMerton.Tests;

[Trait("Category", "Unit")]
public class EuropeanCalculatorTests
{
    private readonly ITestOutputHelper _output;

    public EuropeanCalculatorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private const double Precision = 5e-5;

    [Fact]
    public void GetPricesAndGreeks_HullBookExample_MatchManualCalculations()
    {
        var sut = new EuropeanCalculator(49, 50, 0.05, 0.2, 20/52.0);
        
        using var scope = new AssertionScope();

        sut.CallPrice.Should().BeApproximately(2.4005, Precision);
        sut.PutPrice.Should().BeApproximately(2.4482, Precision);
        
        sut.CallDelta.Should().BeApproximately(0.5216, Precision);
        sut.PutDelta.Should().BeApproximately(-0.4784, Precision);
        
        sut.CallTheta.Should().BeApproximately(-4.3053, Precision);
        sut.PutTheta.Should().BeApproximately(-1.85295, Precision);
        
        sut.Gamma.Should().BeApproximately(0.0655, Precision);
        
        sut.Vega.Should().BeApproximately(12.1055, Precision);
        
        sut.CallRho.Should().BeApproximately(8.90695, Precision);
        sut.PutRho.Should().BeApproximately(-10.34796, Precision);
    }

    [Fact]
    public void CreateEuropeanCalculator_PayloadInCtor_ObserveRuntime()
    {
        //code clean up in the calculator's ctor reduced runtime by 45% !
        for (int i = 0; i < 1_000_000; i++)
        {
            new EuropeanCalculator(40 + i%10, 50, 0.05, 0.2, 20 / 52.0);
        }
    }

    [Fact]
    public void GetPricesAndGreeks_PerfectHedgingExample_MatchManualCalculations()
    {
        const double daysInYear = 252.0;
        var stockPrices = new[]
        {
            55.5625, 55.3750, 55.4375, 56.5625, 59.1250, 60.3125, 61.3125, 60.6250,
            62.6875, 61.25, 63.25, 64.1875, 64.25, 65, 63,
            64.1875, 65.8125, 68.25, 68.125, 68.8125
        };
        stockPrices.Should().HaveCount(20);
        
        var dailyData = stockPrices[1..].Zip(stockPrices[..^1], (lhs, rhs) => Math.Log(lhs/rhs))
            .ToArray();
        var averageDailyData = dailyData.Average();
        var sampleVariance = dailyData.Sum(d => Math.Pow(d - averageDailyData, 2)) / (dailyData.Length - 1);
        var annualStdDev = Math.Sqrt(sampleVariance * daysInYear);
        annualStdDev.Should().BeApproximately(0.317139, Precision);

        var calculators = stockPrices
            .Select((s, i) => new EuropeanCalculator(s, 55, 0, annualStdDev, (20-i)/daysInYear))
            .ToArray();

        var wealth = new double[calculators.Length];
        wealth[0] = calculators[0].CallPrice;
        for (int i = 1; i < calculators.Length; i++)
        {
            wealth[i] = wealth[i - 1] + calculators[i - 1].CallDelta * (calculators[i].S - calculators[i - 1].S);
        }

        _output.WriteLine("replication experiment");
        _output.WriteLine("Time\tStockPrice\tCallPrice\tDelta\t\tWealth");
        for (int i = 0; i < calculators.Length; i++)
        {
            var stat = calculators[i];
            _output.WriteLine($"{i}\t\t{stat.S:N4}\t\t{stat.CallPrice:N4}\t\t{stat.CallDelta:N4}\t\t{wealth[i]:N4}");
        }
    }


    [Fact]
    public void EuropeanCallOption_1mReplicate_2mLiquidate_Observe()
    {
        var steps = new[]
        {
            new EuropeanCalculator(100, 100, 0.05, 0.25, 3.0/12),
            new EuropeanCalculator(98, 100, 0.05, 0.25, 2.0/12),
            new EuropeanCalculator(101, 100, 0.05, 0.25, 1.0/12),
        };

        var initiallyBorrow = steps[0].CallDelta * steps[0].S - 1.08 * steps[0].CallPrice;

        var reBalanceCost = (steps[1].CallDelta - steps[0].CallDelta)*steps[1].S;
        var borrowAfterReBalance = initiallyBorrow * Math.Exp(0.05 / 12) + reBalanceCost;

        var finalBankDept = borrowAfterReBalance * Math.Exp(0.05 / 12);
        //return bank dept + buy back the option + sell stocks (we have delta from 1 month and current prices)
        var pnl = -finalBankDept - steps[2].CallPrice + steps[1].CallDelta * steps[2].S;

        _output.WriteLine(
            $"""
             month 0:
                borrow {initiallyBorrow:N4}
                own stocks {steps[0].CallDelta:N4}
                sold option for {1.08 * steps[0].CallPrice:N4}
             
             month 1:
                owe to bank {borrowAfterReBalance:N4}
                own stocks {steps[1].CallDelta:N4}
             
             month 2:
                owe to bank {finalBankDept:N4}
                own stocks {steps[1].CallDelta:N4} sell at {steps[1].CallDelta * steps[2].S:N4}
                buy the option back {steps[2].CallPrice:N4}
                
             total pnl {pnl:N4}
             """);
    }
}
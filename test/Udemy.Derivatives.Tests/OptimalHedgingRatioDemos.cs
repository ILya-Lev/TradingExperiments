using FluentAssertions;
using FluentAssertions.Execution;
using MathNet.Numerics.Statistics;
using System.Numerics;

namespace Udemy.Derivatives.Tests;

[Trait("Category", "Unit")]
public class OptimalHedgingRatioDemos(ITestOutputHelper output)
{
    [Theory]
    [InlineData(900, 902)]
    [InlineData(950, 952)]
    [InlineData(1000, 1003)]
    [InlineData(1050, 1053)]
    [InlineData(1100, 1103)]
    public void HedgeWithBeta_SystematicRisk_Observe(double index, double futures)
    {
        var portfolioValue = 5_050_000;
        var contractSize = 250;
        var beta = 1.5;
        var riskFreeRate = 0.04;//annually
        var dividendYield = 0.01;//annually
        var period = 3 / 12.0;//3 months
        var initialIndexLevel = 1_000d;
        var initialFuturesPrice = 1_010d;

        var optimalHedgeSize = beta * portfolioValue / contractSize / initialFuturesPrice;

        var futuresPnl = -optimalHedgeSize * contractSize * (futures - initialFuturesPrice);//- as short

        var returnOnIndex = (index / initialIndexLevel - 1) + dividendYield * period;
        var expectedPortfolioReturn = riskFreeRate * period + beta * (returnOnIndex - riskFreeRate * period);

        var portfolioPosition = portfolioValue * (1 + expectedPortfolioReturn);

        var finalValue = portfolioPosition + futuresPnl;

        output.WriteLine($"""
                            index from {initialIndexLevel} to {index} in period {period} Y
                            futures from {initialFuturesPrice} to {futures} over the same period
                            risk free rate {riskFreeRate:P} and dividends {dividendYield:P}
                            beta {beta}
                            estimated risk free rate = {Math.Log(initialFuturesPrice/initialIndexLevel)/period:P4}
                            initial basis = {initialIndexLevel - initialFuturesPrice}
                            final basis = {index - futures}
                            
                            optimal hedge size {optimalHedgeSize:N4}
                            futures PnL {futuresPnl:N4}
                            expected return on portfolio {expectedPortfolioReturn:P4}
                            portfolio position {portfolioPosition:N4}
                            final value {finalValue:N4}
                         """);
    }

    [Fact]
    public void OptimalHedgeRatio_Example33_Observe()
    {
        double[] assetChanges = [.029, .02, -.044, .008, .026, -.019, -.01, -.007, .043, .011, -.036, -.018, .009, -.032, .023];
        double[] futuresChanges = [.021, .035, -.046, .001, .044, -.029, -.026, -.029, .048, -.006, -.036, -.011, .019, -.027, .029];
        double targetAssetQuantity = 2_000_000;
        double contractSize = 42_000;

        var assetStat = new DescriptiveStatistics(assetChanges, true);
        var sigmaS = assetStat.StandardDeviation;

        var futuresStat = new DescriptiveStatistics(futuresChanges, true);
        var sigmaF = Math.Abs(futuresStat.StandardDeviation) < 1e-6 ? 1 : futuresStat.StandardDeviation;

        var correlation = Correlation.Pearson(assetChanges, futuresChanges);

        var optimalHedgeRatio = correlation * sigmaS / sigmaF;
        var optimalContractNumber = optimalHedgeRatio * targetAssetQuantity / contractSize;

        output.WriteLine($"sigma s = {sigmaS:N4} sigma f = {sigmaF:N4} rho {correlation:N4}");
        output.WriteLine($"optimal hedge ratio = {optimalHedgeRatio:N4} optimal contract number = {optimalContractNumber:N4}");

        using var _ = new AssertionScope();
        assetChanges.Length.Should().Be(futuresChanges.Length);
        optimalHedgeRatio.Should().BeApproximately(.78, 1e-2);
        optimalContractNumber.Should().BeApproximately(37, 5e-1);
    }

    [Fact]
    public void OptimalHedgeRatio_Example33_UsingOwnImpl_Observe()
    {
        decimal[] assetChanges = [.029m, .02m, -.044m, .008m, .026m, -.019m, -.01m, -.007m, .043m, .011m, -.036m, -.018m, .009m, -.032m, .023m];
        decimal[] futuresChanges = [.021m, .035m, -.046m, .001m, .044m, -.029m, -.026m, -.029m, .048m, -.006m, -.036m, -.011m, .019m, -.027m, .029m];
        double targetAssetQuantity = 2_000_000;
        double contractSize = 42_000;

        var stat = new PearsonStatistics<decimal>(assetChanges, futuresChanges);

        var sigmaS = Math.Sqrt((double)stat.VarianceLhs);
        var sigmaF = Math.Sqrt((double)stat.VarianceRhs); ;
        var correlation = Math.Sqrt((double)stat.CorrelationSquared);

        var optimalHedgeRatio = correlation * sigmaS / sigmaF;
        var optimalContractNumber = optimalHedgeRatio * targetAssetQuantity / contractSize;

        output.WriteLine($"sigma s = {sigmaS:N4} sigma f = {sigmaF:N4} rho {correlation:N4}");
        output.WriteLine($"optimal hedge ratio = {optimalHedgeRatio:N4} optimal contract number = {optimalContractNumber:N4}");

        using var _ = new AssertionScope();
        assetChanges.Length.Should().Be(futuresChanges.Length);
        optimalHedgeRatio.Should().BeApproximately(.78, 1e-2);
        optimalContractNumber.Should().BeApproximately(37, 5e-1);
    }

    public class PearsonStatistics<T> where T : INumber<T>
    {
        /*
         
           | Variable | Real Role | Value / Formula at Step `k` | Purpose |
           | --- | --- | --- | --- |
           | `num1` | `count` (`n`) | `k` (after `++num1`) | Count of processed pairs; incremented as a side effect inside `num9`'s assignment `[COMPUTED] (HIGH)`. |
           | `num2` | `coMoment` | ∑i=1k(ai−aˉk)(bi−bˉk)\\sum_{i=1}^{k} (a_i - \\bar{a}_k)(b_i - \\bar{b}_k)∑i=1k​(ai​−aˉk​)(bi​−bˉk​) | Running sum of cross-deviations (`(n - 1)` × sample covariance) `[COMPUTED] (HIGH)`. |
           | `num3` | `meanA` | aˉk−1+ak−aˉk−1k\\bar{a}_{k-1} + \\frac{a_k - \\bar{a}_{k-1}}{k}aˉk−1​+kak​−aˉk−1​​ | Running arithmetic mean of `dataA` `[COMPUTED] (HIGH)`. |
           | `num4` | `meanB` | bˉk−1+bk−bˉk−1k\\bar{b}_{k-1} + \\frac{b_k - \\bar{b}_{k-1}}{k}bˉk−1​+kbk​−bˉk−1​​ | Running arithmetic mean of `dataB` `[COMPUTED] (HIGH)`. |
           | `num5` | `m2A` | ∑i=1k(ai−aˉk)2\\sum_{i=1}^{k} (a_i - \\bar{a}_k)^2∑i=1k​(ai​−aˉk​)2 | Running sum of squared deviations of `dataA` (`(n - 1)` × sample variance) `[COMPUTED] (HIGH)`. |
           | `num6` | `m2B` | ∑i=1k(bi−bˉk)2\\sum_{i=1}^{k} (b_i - \\bar{b}_k)^2∑i=1k​(bi​−bˉk​)2 | Running sum of squared deviations of `dataB` (`(n - 1)` × sample variance) `[COMPUTED] (HIGH)`. |
           | `num7` | `prevMeanA` | `num3` (aˉk−1\\bar{a}_{k-1}aˉk−1​) | Redundant temporary copy of `num3`; `current2 - num4` skips creating an equivalent copy `[COMPUTED] (HIGH)`. Why the compiled IL retained this extra local slot cannot be determined from the snippet alone `[INFERRED] (HIGH)`. |
           | `num8` | `deltaA` | ak−aˉk−1a_k - \\bar{a}_{k-1}ak​−aˉk−1​ | Deviation of the new `dataA` item from the old mean `[COMPUTED] (HIGH)`. |
           | `num9` | `scaleDeltaA` | ak−aˉk−1k\\frac{a_k - \\bar{a}_{k-1}}{k}kak​−aˉk−1​​ | Mean-update step for `dataA`; also executes `++num1` `[COMPUTED] (HIGH)`. |
           | `num10` | `deltaB` | bk−bˉk−1b_k - \\bar{b}_{k-1}bk​−bˉk−1​ | Deviation of the new `dataB` item from the old mean (`dataB` twin of `num8`) `[COMPUTED] (HIGH)`. |
           | `num11` | `scaleDeltaB` | bk−bˉk−1k\\frac{b_k - \\bar{b}_{k-1}}{k}kbk​−bˉk−1​​ | Mean-update step for `dataB` using the already-incremented `num1` (`dataB` twin of `num9`) `[COMPUTED] (HIGH)`. |
         
         */

        public PearsonStatistics(IEnumerable<T> lhs, IEnumerable<T> rhs)
        {
            Count = T.Zero;
            Covariance = T.Zero;
            MeanLhs = T.Zero;
            MeanRhs = T.Zero;
            VarianceLhs = T.Zero;
            VarianceRhs = T.Zero;

            using var lhsIterator = lhs.GetEnumerator();
            using var rhsIterator = rhs.GetEnumerator();

            while (lhsIterator.MoveNext())
            {
                if (!rhsIterator.MoveNext())
                    throw new ArgumentOutOfRangeException(nameof(rhs), "The array arguments must have the same length.");

                var previousCount = Count;
                Count++;

                var currentLhs = lhsIterator.Current;
                var num8 = currentLhs - MeanLhs;
                var num9 = num8 / Count;
                MeanLhs += num9;
                VarianceLhs += num9 * num8 * previousCount;

                var currentRhs = rhsIterator.Current;
                var num10 = currentRhs - MeanRhs;
                var num11 = num10 / Count;
                MeanRhs += num11;
                VarianceRhs += num11 * num10 * previousCount;

                Covariance += num8 * num10 * previousCount / Count;
            }

            if (rhsIterator.MoveNext())
                throw new ArgumentOutOfRangeException(nameof(lhs), "The array arguments must have the same length.");

            CorrelationSquared = (Covariance / VarianceLhs) * (Covariance / VarianceRhs);

            var sampleCount = Count;
            sampleCount--;
            VarianceLhs /= sampleCount;
            VarianceRhs /= sampleCount;
            Covariance /= sampleCount;
        }

        public T MeanLhs { get; }
        public T MeanRhs { get; }
        public T VarianceLhs { get; }
        public T VarianceRhs { get; }
        public T Covariance { get; }
        public T CorrelationSquared { get; }
        public T Count { get; }
    }

}
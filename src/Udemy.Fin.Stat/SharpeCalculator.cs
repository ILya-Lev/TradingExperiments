using MathNet.Numerics.Statistics;

namespace Udemy.Fin.Stat;

public static class SharpeCalculator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="prices"></param>
    /// <param name="aggregationWindow"></param>
    /// <param name="riskFreeReturn">here is a default constant = 3%; but should be time-series per quarter or so</param>
    /// <returns></returns>
    public static IEnumerable<decimal> CalculateDailySharpe(IEnumerable<decimal> prices, 
        int aggregationWindow, 
        decimal riskFreeReturn = 0.03m)
    {
        var buffer = new Queue<decimal>();
        using var iterator = prices.GetEnumerator();
        
        while (buffer.Count < aggregationWindow && iterator.MoveNext())
            buffer.Enqueue(iterator.Current);

        while (buffer.Count == aggregationWindow)
        {
            var returns = ReturnsCalculator.CalculateContinuouslyCompoundedDailyReturns(buffer);
            var stat = new DescriptiveStatistics(returns.Select(r => (double)r));

            yield return ((decimal)stat.Mean - riskFreeReturn) / (decimal)stat.StandardDeviation;

            buffer.Dequeue();
            if (iterator.MoveNext())
                buffer.Enqueue(iterator.Current);
        }
    }
}
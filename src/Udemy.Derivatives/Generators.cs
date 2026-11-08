using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using System.Numerics;

namespace Udemy.Derivatives;

public static class Generators
{
    /// <summary>
    /// generates a random walk of the same jump size
    /// if "we flip a coin" and get up to the success probability, we jump +1, else -1
    /// </summary>
    /// <returns>a sequence of distances from the zero position after one more jump every time</returns>
    public static IEnumerable<int> GetStaticJumpRandomWalk(double successProbability = 0.5)
    {
        var distance = 0;
        while (true)
        {
            yield return distance;
            
            //distance += 2 * Bernoulli.Sample(successProbability) - 1;//bernoulli is either +1 (2*1 - 1 = 1) or 0 (2*0 - 1 = -1)

            //to eliminate the dependency on the 3rd party library here,
            //we can generate Bernoulli sample ourselves and avoid the linear transformation
            distance += Random.Shared.NextDouble() <= successProbability ? 1 : -1;
        }
    }

    /// <summary>
    /// generates a random walk of the jump size proportional to the current distance from the zero position
    /// if "we flip a coin" and get up to the success probability, we jump N% upward, else N% downward
    /// </summary>
    /// <returns>a sequence of distances from the zero position after one more jump every time</returns>
    public static IEnumerable<decimal> GetRelativeRandomWalk(double successProbability = 0.5, decimal relativeJumpSize = 0.10m)
    {
        yield return 0m;
        //System.Security.Cryptography.RandomNumberGenerator
        var distance = (SystemRandomSource.Default.NextDouble() <= successProbability ? 1 : -1) * relativeJumpSize;
        yield return distance;

        while (true)
        {
            var jumpDirection = (SystemRandomSource.Default.NextDouble() <= successProbability ? 1 : -1);
            var jumpSize = Math.Max(relativeJumpSize * Math.Abs(distance), relativeJumpSize);
            distance += jumpDirection * jumpSize;
            yield return distance;
        }
    }

    public static IReadOnlyList<decimal> GetScaledRandomWalk(int jumpsNumber, double successProbability = 0.5)
    {
        var scale = (decimal)Math.Sqrt(jumpsNumber);
        
        var distances = new decimal[jumpsNumber];
        distances[0] = 0m;

        for (var i = 1; i<jumpsNumber;i++)
        {
            distances[i] = distances[i-1] + (SystemRandomSource.Default.NextDouble() <= successProbability ? 1 : -1) / scale;
        }

        return distances;
    }

    public static IReadOnlyList<decimal> GetBrownianMotion(decimal mu, decimal sigma, int sampleSize)
    {
        var scale = (decimal)Math.Sqrt(sampleSize);
        var drift = mu / sampleSize;
        var diffusionScaling = sigma / scale;

        var distances = new decimal[sampleSize];
        distances[0] = 0;
        for (var jumpNumber = 1; jumpNumber < sampleSize; jumpNumber++)
        {
            var jump = (decimal)Normal.Sample(0, 1);
            distances[jumpNumber] = distances[jumpNumber - 1] + drift + diffusionScaling * jump;
        }

        return distances;
    }

    /// <summary>
    /// adds up values in a sliding window of the given size
    /// </summary>
    public static IEnumerable<T> Accumulate<T>(this IEnumerable<T> returns, int windowSize = 1)
        where T : INumber<T>
    {
        var ringBuffer = new T[windowSize];
        var total = T.Zero;
        var head = 0;
        var seenNumbers = 0L;

        using var iterator = returns.GetEnumerator();
        while (iterator.MoveNext())
        {
            total -= ringBuffer[head];
            ringBuffer[head] = iterator.Current;
            total += ringBuffer[head];

            head = (head + 1) % ringBuffer.Length;

            if (++seenNumbers >= ringBuffer.Length)
                yield return total;
        }
    }
}
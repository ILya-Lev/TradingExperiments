using System.Buffers;
using System.Numerics.Tensors;

namespace Udemy.Fin.Stat;

public static class HistogramBuilder
{
    public static IEnumerable<(double bucketStart, int count)> GetHistogramDefinition(
        this IReadOnlyCollection<double> sequence,
        double binWidth)
    {
        if (!sequence.Any()) return [];

        var min = sequence.Min();
        var max = sequence.Max();

        var bucketsNumber = (int)Math.Ceiling((max - min) / binWidth);
        var bucketCounters = new int[bucketsNumber];

        foreach (var item in sequence)
        {
            var index = (int)Math.Floor((item - min) / binWidth);
            bucketCounters[index]++;
        }

        return bucketCounters.Select((n, i) => (i * binWidth + min, b: n));
    }

    public static IEnumerable<(double bucketStart, int count)> GetHistogramDefinition(
        this IEnumerable<double> sequence,
        double start,
        double binWidth)
    {
        var bucketCounters = new Dictionary<int, int>();
        var max = double.MinValue;

        foreach (var item in sequence)
        {
            if (item < start)
                throw new InvalidOperationException(
                    $"encountered item {item} which is smaller than the histogram start {start}");

            max = Math.Max(max, item);

            var index = (int)Math.Floor((item - start) / binWidth);
            bucketCounters.TryAdd(index, 0);
            bucketCounters[index]++;
        }

        for (int i = 0; i * binWidth + start < max; i++)
        {
            yield return (i, bucketCounters.GetValueOrDefault(i, 0));
        }
    }

    /// <param name="sequence"></param>
    extension(ReadOnlySpan<double> sequence)
    {
        /// <summary>
        /// the Freedman-Diaconis rule
        /// </summary>
        public double GetBinWidthQuantileRange()
        {
            if (sequence.Length < 2) return 0;

            var buffer = ArrayPool<double>.Shared.Rent(sequence.Length);

            try
            {
                var sorted = buffer.AsSpan(..sequence.Length);
                sequence.CopyTo(sorted);
                sorted.Sort();

                var firstQ = sorted[sequence.Length / 4];
                var thirdQ = sorted[sequence.Length * 3 / 4];//integer division is enough to avoid out of range

                var interQuantileRange = (thirdQ - firstQ);

                return 2 * interQuantileRange / Math.Cbrt(sequence.Length);
            }
            finally
            {
                ArrayPool<double>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Scott's rule
        /// </summary>
        public double GetBinWidthStandardDeviation() => 
            sequence.Length < 2
            ? 0
            : 3.5 * TensorPrimitives.StdDev(sequence) / Math.Cbrt(sequence.Length);
    }
}

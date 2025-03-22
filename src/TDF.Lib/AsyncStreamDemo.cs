using System.Runtime.CompilerServices;

namespace TDF.Lib;

public class AsyncStreamDemo
{
    public static async IAsyncEnumerable<int> GetNumbers([EnumeratorCancellation] CancellationToken stopper = default)
    {
        var page = 0;
        const int size = 3;
        while (!stopper.IsCancellationRequested)
        {
            var numbers = await GetData(page, size);
            foreach (var number in numbers)
            {
                yield return number;
            }
            page++;
        }
    }

    private static async Task<IEnumerable<int>> GetData(int page, int size)
    {
        await Task.Delay(400);
        return Enumerable.Range(page * size, size);
    }

    public static double GetAverage(IReadOnlyCollection<double> source)
    {
        var sum = 0.0;
        var count = 0;
        var mutex = new Lock();

        var result = Parallel.ForEach(source
            , new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 }
            , () => (sum: 0.0, count: 0)
            , (number, state, accumulator) => double.IsFinite(number)
                ? (accumulator.sum + number, accumulator.count + 1)
                : accumulator
            , accumulator =>
            {
                lock (mutex)
                {
                    sum += accumulator.sum;//cannot be done with interlocked, the later supports integer types only
                    count += accumulator.count;//could be done with interlocked
                }
            }
        );
        return result.IsCompleted
            ? sum/count
            : throw new InvalidOperationException(
            $"parallel loop was aborted after {result.LowestBreakIteration} iterations out of {source.Count}");
    }

    public static double GetAveragePlinq(IReadOnlyCollection<double> source) => source
        .AsParallel()
        .WithDegreeOfParallelism(Environment.ProcessorCount - 1)
        .Aggregate(() => (sum: 0.0, count: 0)
            , (accumulator, number) => double.IsFinite(number)
                ? (accumulator.sum + number, accumulator.count + 1)
                : accumulator
            , (lhs, rhs) => (lhs.sum + rhs.sum, lhs.count + rhs.count)
            , accumulator => accumulator.sum / accumulator.count);
}
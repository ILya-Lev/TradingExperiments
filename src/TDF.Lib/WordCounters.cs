using System.Collections.Concurrent;

namespace TDF.Lib;

public interface IWordCounter
{
    Task<IReadOnlyDictionary<string, int>> GetWordsStatistic(IEnumerable<string> fileNames);
}

public class WordCounterLinqAggregate : IWordCounter
{
    public Task<IReadOnlyDictionary<string, int>> GetWordsStatistic(IEnumerable<string> fileNames) 
        => Task.FromResult(DoGetWordsStatistic(fileNames));

    private IReadOnlyDictionary<string, int> DoGetWordsStatistic(IEnumerable<string> fileNames) => fileNames
        .Aggregate(
            seed: new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
            func: (accumulator, fileName) =>
            {
                using var fileStream = File.OpenRead(fileName);
                using var streamReader = new StreamReader(fileStream);

                for (var line = streamReader.ReadLine(); line is not null; line = streamReader.ReadLine())
                {
                    foreach (var word in line.Split(' '))
                    {
                        var wordChars = word.ToCharArray().Where(char.IsLetterOrDigit).ToArray();
                        if (wordChars.Length == 0)
                            continue;

                        var cleanedWord = new string(wordChars);
                        if (!accumulator.TryAdd(cleanedWord, 1))
                            accumulator[cleanedWord]++;
                    }
                }

                return accumulator;
            }
        );
}

public class WordCounterPlinqAggregate : IWordCounter
{
    public Task<IReadOnlyDictionary<string, int>> GetWordsStatistic(IEnumerable<string> fileNames) 
        => Task.FromResult(DoGetWordsStatistic(fileNames));

    private IReadOnlyDictionary<string, int> DoGetWordsStatistic(IEnumerable<string> fileNames) => fileNames
        .AsParallel()
        .WithDegreeOfParallelism(degreeOfParallelism: Environment.ProcessorCount - 1)
        .WithExecutionMode(executionMode: ParallelExecutionMode.ForceParallelism)
        .WithMergeOptions(mergeOptions: ParallelMergeOptions.NotBuffered)
        .Aggregate(
            seedFactory: () => new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
            updateAccumulatorFunc: (accumulator, fileName) =>
            {
                using var fileStream = File.OpenRead(fileName);
                using var streamReader = new StreamReader(fileStream);

                for (var line = streamReader.ReadLine(); line is not null; line = streamReader.ReadLine())
                {
                    foreach (var word in line.Split(' '))
                    {
                        var wordChars = word.ToCharArray().Where(char.IsLetterOrDigit).ToArray();
                        if (wordChars.Length == 0)
                            continue;

                        var cleanedWord = new string(wordChars);
                        if (!accumulator.TryAdd(cleanedWord, 1))
                            accumulator[cleanedWord]++;
                    }
                }

                return accumulator;
            },
            combineAccumulatorsFunc: (lhs, rhs) =>
            {
                foreach(var (word, counter) in rhs)
                    if (!lhs.TryAdd(word, counter))
                        lhs[word] += counter;
                
                return lhs;
            },
            resultSelector: accumulator => accumulator
        );
}

public class WordCounterParallelAsyncEnumerable : IWordCounter
{
    public async Task<IReadOnlyDictionary<string, int>> GetWordsStatistic(IEnumerable<string> fileNames)
    {
        var accumulator = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        await Parallel.ForEachAsync(
            source: ReadLines(fileNames),
            parallelOptions: new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1,
            },
            body: (line, ct) =>
            {
                foreach (var word in line?.Split(' ') ?? [])
                {
                    var wordChars = word.ToCharArray().Where(char.IsLetterOrDigit).ToArray();
                    if (wordChars.Length == 0)
                        continue;

                    var cleanedWord = new string(wordChars);
                    accumulator.AddOrUpdate(cleanedWord, w => 1, (w, counter) => counter + 1);
                }
                return ValueTask.CompletedTask;
            });

        return accumulator;
    }

    private static async IAsyncEnumerable<string?> ReadLines(IEnumerable<string> fileNames)
    {
        foreach (var fileName in fileNames)
        {
            await using var fileStream = File.OpenRead(fileName);
            using var streamReader = new StreamReader(fileStream);

            var line = await streamReader.ReadLineAsync();
            while (line is not null)
            {
                yield return line;
                line = await streamReader.ReadLineAsync();
            }
        }
    }
}


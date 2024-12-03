using FluentAssertions;
using System.Diagnostics;
using System.Text;
using Xunit.Abstractions;

namespace Sudoku.Tests;

[Trait("Category", "Unit")]
public class TrieHelpersTests(ITestOutputHelper output)
{
    [Fact]
    public void Build_GetTotalWords_PrintFrequencyTable()
    {
        const int size = 100_000;

        var beforeWords = Process.GetCurrentProcess().PagedMemorySize64;
        var words = Enumerable.Range(1, size).Select(n => GenerateWord(Random.Shared.Next(1, 10))).ToArray();
        var afterWords = Process.GetCurrentProcess().PagedMemorySize64;
        output.WriteLine($"Words consumed memory {(afterWords-beforeWords)/1024.0/1024:N2} MB");

        var beforeTree = Process.GetCurrentProcess().PagedMemorySize64;
        var frequencyTree = TrieHelpers.BuildWordFrequencyTree(words);
        var afterTree = Process.GetCurrentProcess().PagedMemorySize64;
        output.WriteLine($"Tree consumed memory {(afterTree-beforeTree)/1024.0/1024:N2} MB");

        var beforeDictionary = Process.GetCurrentProcess().PagedMemorySize64;
        var frequencyDictionary = words.GroupBy(w => w).ToDictionary(g => g.Key, g => g.Count());
        var afterDictionary = Process.GetCurrentProcess().PagedMemorySize64;
        output.WriteLine($"Dictionary consumed memory {(afterDictionary-beforeDictionary)/1024.0/1024:N2} MB");
        output.WriteLine($"total words {frequencyDictionary.Values.Sum()}");

        var totalWordsNumber = TrieHelpers.GetTotalWordsCount(frequencyTree);

        totalWordsNumber.Should().Be(size);
        foreach (var word in words.Distinct().OrderByDescending(w => w.Length))
        {
            var count = TrieHelpers.GetWordCount(frequencyTree, word);
            count.Should().Be(frequencyDictionary[word]);
            if (count >= 2)
                output.WriteLine($"'{word}' is met {count} times");
        }
    }

    private static readonly char[] Alphabet = "abcdefghijklmnopqrstuvwxyz".OrderBy(ch => ch).ToArray();

    private static string GenerateWord(int length)
    {
        var sb = new StringBuilder(length);
        
        for (int i = 0; i < length; i++)
            sb.Append(Alphabet[Random.Shared.Next(0, Alphabet.Length)]);

        return sb.ToString();
    }
}
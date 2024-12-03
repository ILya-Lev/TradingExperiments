using System.Buffers;

namespace Sudoku;

/// <summary> aka Lexical prefix tree  </summary>
public class Trie(char value)
{
    //public Trie? Parent { get; init;}
    public Dictionary<char, Trie> Children { get; } = new();
    public char Value { get; } = value;
    public int Counter { get; set; } = 0;
}

public static class TrieHelpers
{
    private static readonly SearchValues<char> _wordEnd = SearchValues.Create(".,:;!?/ \'\"\t\n\r()[]{}");

    public static int GetTotalWordsCount(Trie frequencyTree)
    {
        var total = 0;
    
        var layer = new Queue<Trie>();
        layer.Enqueue(frequencyTree);
        
        while (layer.Any())
        {
            var current = layer.Dequeue();
            total += current.Counter;
        
            foreach (var child in current.Children.Values)
                layer.Enqueue(child);
        }

        return total;
    }

    public static int GetWordCount(Trie frequencyTree, string word)
    {
        var current = frequencyTree;
        foreach (var ch in word)
        {
            if (!current.Children.TryGetValue(ch, out var child))
                return 0;//the word is not in the tree
            current = child;
        }
        return current.Counter;//if 0, the word is not in the tree
    }

    public static Trie BuildWordFrequencyTree(IEnumerable<string> source)
    {
        var root = new Trie(' ');
        foreach (var s in source)
        {
            var word = new List<char>(30);
            foreach (var ch in s)
            {
                if (_wordEnd.Contains(ch))
                {
                    PutWordInTree(root, word.ToArray());
                    word.Clear();
                    continue;
                }
                word.Add(ch);
            }
            if (word.Any())
                PutWordInTree(root, word.ToArray());
        }
        return root;
    }

    private static void PutWordInTree(Trie root, char[] word)
    {
        var current = root;
        for (int i = 0; i < word.Length - 1; i++)
        {
            if (!current.Children.TryGetValue(word[i], out var child))
            {
                child = new Trie(word[i]);
                current.Children.Add(word[i], child);
            }
            current = child;
        }

        if (!current.Children.TryGetValue(word.Last(), out var wordEnd))
        {
            wordEnd = new Trie(word.Last());
            current.Children.Add(word.Last(), wordEnd);
        }
        wordEnd.Counter++;
    }
}
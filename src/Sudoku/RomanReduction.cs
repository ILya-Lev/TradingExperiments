using System.Text;

namespace Sudoku;

public class RomanReduction
{
    private static readonly Dictionary<char, int> _romanToArab = new()
    {
        ['I'] = 1,
        ['V'] = 5,
        ['X'] = 10,
        ['L'] = 50,
        ['C'] = 100,
        ['D'] = 500,
        ['M'] = 1000,
    };

    private static readonly Dictionary<int, string> _arabToRoman = new()
    {
        [1000] = "M",
        [900] = "CM",
        [500] = "D",
        [400] = "CD",
        [100] = "C",
        [90] = "XC",
        [50] = "L",
        [40] = "XL",
        [10] = "X",
        [9] = "IX",
        [5] = "V",
        [4] = "IV",
        [1] = "I",
    };

    public static string Reduce(string messy)
    {
        var messyChars = messy.ToUpperInvariant().ToCharArray();
        
        var number = FromRomanToArab(messyChars);

        return FromArabToRoman(number);
    }

    private static int FromRomanToArab(char[] messyChars)
    {
        var number = 0;
        for (int i = 0; i+1 < messyChars.Length; i++)
        {
            var current = _romanToArab[messyChars[i]];
            if (_romanToArab[messyChars[i+1]] > current)
                number -= current;
            else
                number += current;
        }
        number += _romanToArab[messyChars.Last()];
        return number;
    }

    private static string FromArabToRoman(int number)
    {
        var result = new StringBuilder();
        foreach (var (n, s) in _arabToRoman)
        {
            if (number < n)
                continue;

            foreach (var part in Enumerable.Range(0, number/n).Select(_ => s))
            {
                result.Append(part);
            }

            number %= n;
        }
        return result.ToString();
    }
}
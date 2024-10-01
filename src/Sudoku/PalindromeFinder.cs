using System.Runtime.CompilerServices;

namespace Sudoku;

public class PalindromeFinder
{
    public static int FindLargestPalindrome()
    {
        var max = 0;
        for (int lhs = 999; lhs > 100; lhs--)
            for (int rhs = lhs; rhs > 100; rhs--)
            {
                var current = lhs * rhs;
                if (current > max && IsPalindromeList(current)) 
                    max = current;
            }

        return max;
    }

    public static bool IsPalindromeList(int n)
    {
        List<int> digits = new();
        while (n > 0)
        {
            digits.Add(n % 10);
            n /= 10;
        }

        for (int i = 0; i < digits.Count / 2; i++)
        {
            if (digits[i] != digits[^(i + 1)])
            {
                return false;
            }
        }
        return true;
    }

    public static bool IsPalindromeInPlace(int n)
    {
        var size = (int)Math.Pow(10, (uint)Math.Log10(n));

        while (n > 0)
        {
            var head = n / size;
            var tail = n % 10;

            if (head != tail) return false;

            n -= head * size;
            n /= 10;
            size /= 100;
        }

        return true;
    }

    [SkipLocalsInit]
    public static bool IsPalindromeStackAlloc(int n)//works for 6 digits numbers only
    {
        Span<int> digits = stackalloc int[6];
        var position = 0;
        while (n > 0)
        {
            digits[position++] = n % 10;
            n /= 10;
        }

        for (int i = 0; i < digits.Length / 2; i++)
        {
            if (digits[i] != digits[^(i + 1)])
            {
                return false;
            }
        }
        return true;
    }
}
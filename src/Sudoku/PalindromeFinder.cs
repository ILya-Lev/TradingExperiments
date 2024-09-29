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
                if (current > max && IsPalindrome(current)) 
                    max = current;
            }

        return max;
    }

    [SkipLocalsInit]
    public static bool IsPalindrome(int n)//works for 6 digits numbers only
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
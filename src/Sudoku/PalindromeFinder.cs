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

    public static bool IsPalindrome(int n)
    {
        var digits = new List<int>();
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
}
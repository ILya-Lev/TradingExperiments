namespace Sudoku;

public class PalindromeFinder
{
    public static int FindLargestPalindrome()
    {
        return Enumerable.Range(1, 1000)
            .SelectMany(lhs => Enumerable.Range(lhs, 1000 - lhs)
                .Select(rhs => lhs * rhs))
            .Reverse()
            .Where(IsPalindrome)
            .Max();
    }

    public static bool IsPalindrome(int n)
    {
        var digits = new List<int>();
        while (n > 0)
        {
            digits.Add(n % 10);
            n /= 10;
        }

        for (int i = 0; i < digits.Count/2; i++)
        {
            if (digits[i] != digits[^(i+1)])
            {
                return false;
            }
        }
        return true;
    }
}
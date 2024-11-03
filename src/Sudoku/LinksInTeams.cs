namespace Sudoku;

public static class LinksInTeams
{
    public static IEnumerable<(int n, long nLinks, int m, long mLinks)> GenerateStatistics(int n)
    {
        var nLinks = GetLinks(n);
        for (int m = 1; m <= n; m++)
        {
            var mLinks = GetLinks(m)                           //interactions between m groups
                         + (n % m) * GetLinks(n / m + 1)         //n%m groups contains n/m+1 people
                         + (m - n % m) * GetLinks(n / m);        //m-n%m groups contains n/m people
            
            yield return (n, nLinks, m, mLinks);
        }
    }
    public static long GetLinks(long k) => k * (k - 1) / 2;

}
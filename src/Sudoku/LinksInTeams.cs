using System.Collections.Concurrent;

namespace Sudoku;

public static class LinksInTeams
{

    public static IEnumerable<(int n, long nLinks, int m, long mLinks)> GenerateStatistics(int n)
    {
        var nLinks = GetLinks(n);
        for (int m = 1; m <= n; m++)
        {
            //var floor = n / m;

            //var mLinks = _links.GetOrAdd(m, k => GetLinks(k))
            //             + m * _links.GetOrAdd(floor, k => GetLinks(k))
            //             + floor * (n % m);

            var mLinks = GetLinks(m)                //interactions between m groups
                         + m * GetLinks(n / m)    //all m groups contains floor(n/m) people
                         + (n % m) * (n / m);       //n%m groups contains one more person, which is linked with all other persons in the group => n/m links

            //var mLinks = GetLinks(m)                              
            //             + (n % m) * GetLinks(n / m + 1)          
            //             + (m - n % m) * GetLinks(n / m);         

            yield return (n, nLinks, m, mLinks);
        }
    }

    private static ConcurrentDictionary<int, long> _links = new();
    public static void ClearCache() => _links.Clear();

    public static long GetLinks(long k) => k * (k - 1) / 2;
}
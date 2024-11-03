namespace Sudoku;

public static class LinksInTeams
{
    public static IEnumerable<(int n, int nLinks, int m, int mLinks)> GenerateStatistics(int n)
    {
        var nLinks = GetLinks(n);
        for (int m = 1; m <= n; m++)
        {
            var groups = Enumerable.Range(1, n)
                .Select(i => (groupNumber: i % m, i))
                .GroupBy(e => e.groupNumber, e => e.i)
                .ToDictionary(g => g.Key, g => g.Count());

            var links = groups.Values.Distinct().ToDictionary(gp => gp, GetLinks);

            var mLinks = GetLinks(m) + groups.Sum(p => links[p.Value]);

            yield return (n, nLinks, m, mLinks);
        }
    }
    public static int GetLinks(int k) => k * (k - 1) / 2;

}
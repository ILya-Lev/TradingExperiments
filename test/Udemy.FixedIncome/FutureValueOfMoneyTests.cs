namespace Udemy.FixedIncome.Tests;

public class FutureValueOfMoneyTests(ITestOutputHelper output)
{
    [Fact]
    public void GetFutureValue_ProblemSet_01()
    {
        var compounding = new[] { 1, 2, 12 };
        var maturity = new[] { 60, 30, 15 };

        var futureValues = compounding.SelectMany(c => maturity.Select(m => (c, m)))
            .Select(item =>
                (
                    K: item.c,
                    T: item.m,
                    FV:15_000d.GetFutureValue(3, item.c, item.m)
                )
            )
            .ToArray();

        foreach (var (c, m, fv)  in futureValues)
        {
            output.WriteLine($"for maturity {m}M and compounding frequency {c} future value is {fv:N4}");
        }
    }
}

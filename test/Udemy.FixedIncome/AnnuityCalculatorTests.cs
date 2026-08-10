using FluentAssertions;
using FluentAssertions.Execution;

namespace Udemy.FixedIncome.Tests;

[Trait("Category", "Unit")]
public class AnnuityCalculatorTests
{
    /*
     # 5: Suppose you have recently graduated from college and are making a
       plan for saving for retirement that respects your expected income over the
       course of your career. In the next 10 years you expect to be able to put
       away $5000 per year. In the 15 years following that you expect to making
       more money, and plan to be saving $15,000 per year. Finally, in the last 20
       years of your working life, when you expect to be a high earner, you plan
       to be saving $40,000 per year for retirement. Assuming that you'll earn an
       annually compounded interest rate of 4%, how much money will be in your
       retirement account at the end of your career in 45 years.
     */
    [Fact]
    public void PensionSavingStrategy_3Annuities_Observe()
    {
        var a1 = new Annuity(5_000, 10, 0.04, 1);
        var a2 = new Annuity(15_000, 15, 0.04, 1);
        var a3 = new Annuity(40_000, 20, 0.04, 1);

        var fv1 = a1.GetFutureValue(deferredYears: 0, yearsAfter: 35);
        var fv2 = a2.GetFutureValue(deferredYears: 10, yearsAfter: 20);
        var fv3 = a3.GetFutureValue(deferredYears: 25, yearsAfter: 0);

        using var _ = new AssertionScope();
        fv1.Should().BeApproximately(236_885, 1);
        fv2.Should().BeApproximately(658_112, 1);
        fv3.Should().BeApproximately(1_191_123, 1);
        (fv1 + fv2 + fv3).Should().BeApproximately(2_086_121, 1);
    }
}
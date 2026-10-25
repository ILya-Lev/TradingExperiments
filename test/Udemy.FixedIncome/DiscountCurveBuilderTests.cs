namespace Udemy.FixedIncome.Tests;

[Trait("Category", "Unit")]
public class DiscountCurveBuilderTests(ITestOutputHelper output)
{
    [Fact]
    public void BuildDiscountCurve_Bonds_Observe()
    {
        var bonds = new[]
        {
            new BondWithPrice(0, 1, 0.5, 1000, 985),
            new BondWithPrice(5, 2, 1, 10000, 10124),
            new BondWithPrice(7, 1, 2, 10000, 10507),
        };

        var discountCurve = DiscountCurveBuilder.BuildDiscountCurve(bonds, false);

        
        var coupon = 7;
        var maturity = 2;
        var compoundingFrequency = 2;
        var faceValue = 100_000;

        var price = 0.0;
        var couponPayment = coupon / 100.0 / compoundingFrequency * faceValue;
        var step = 1.0 / compoundingFrequency;
        for (var time = step; time <= maturity; time += step)
        {
            var currentDiscountFactor = discountCurve.GetDiscountFactor(time);
            output.WriteLine($"at {time} discount factor {currentDiscountFactor:N4} and spot yield {currentDiscountFactor.GetSpotYield(time)}");

            var cashFlow = time >= maturity ? (faceValue + couponPayment) : couponPayment;
            price += currentDiscountFactor * cashFlow;
        }

        output.WriteLine($"final bond price is {price:N4}");
    }
}


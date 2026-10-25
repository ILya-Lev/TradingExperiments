using MathNet.Numerics.LinearAlgebra;

namespace Udemy.FixedIncome;

/// <summary>
/// Represents a fixed-rate bond definition and its observed market (dirty) price.
/// </summary>
/// <param name="CouponPercent">Annual coupon rate in percent (e.g., 5.0 for 5%).</param>
/// <param name="CompoundingFrequency">Coupon payments per year (e.g., 1 = annual, 2 = semi-annual, 4 = quarterly, 0 = zero-coupon).</param>
/// <param name="MaturityYears">Time to maturity in years (e.g., 0.5, 1.0, 2.0).</param>
/// <param name="FaceValue">Principal / par value paid at maturity (e.g., 100.0).</param>
/// <param name="Price">Observed market dirty price of the bond.</param>
public readonly record struct BondWithPrice(
    double CouponPercent,
    int CompoundingFrequency,
    double MaturityYears,
    double FaceValue,
    double Price);

/// <summary>
/// A point on the discount factor curve at a specific maturity time.
/// </summary>
public readonly record struct DiscountFactorPoint(
    double MaturityYears,
    double DiscountFactor);

public static class DiscountCurveBuilder
{
    private const int TimePrecisionDigits = 6;

    /// <summary>
    /// Builds a discount factor curve by solving the linear system C * d = p.
    /// </summary>
    /// <param name="bonds">Input bonds with prices.</param>
    /// <param name="interpolateMissingPillars">
    /// If false, uses all unique cash-flow times as matrix columns (requires bonds covering all coupon dates).
    /// If true, uses unique bond maturities as matrix columns and linearly interpolates intermediate coupon DFs.
    /// </param>
    public static IReadOnlyList<DiscountFactorPoint> BuildDiscountCurve(
        ReadOnlySpan<BondWithPrice> bonds,
        bool interpolateMissingPillars = false)
    {
        if (bonds.IsEmpty)
            throw new ArgumentException("At least one bond must be provided.", nameof(bonds));

        // 1. Sort bonds by maturity for a well-structured system
        var sortedBonds = bonds.ToArray();
        Array.Sort(sortedBonds, static (a, b) => a.MaturityYears.CompareTo(b.MaturityYears));

        // 2. Generate cash flows for each bond: List of (Time, CashFlowAmount)
        var bondCashFlows = new List<(double Time, double Amount)>[sortedBonds.Length];
        for (int i = 0; i < sortedBonds.Length; i++)
        {
            bondCashFlows[i] = GenerateCashFlows(sortedBonds[i]);
        }

        // 3. Determine pillar times (columns of the matrix)
        double[] pillarTimes = interpolateMissingPillars
            ? [.. sortedBonds.Select(static b => RoundTime(b.MaturityYears)).Distinct().Order()]
            : [.. bondCashFlows.SelectMany(static cfs => cfs.Select(static cf => cf.Time)).Distinct().Order()];

        int rowCount = sortedBonds.Length;
        int colCount = pillarTimes.Length;

        if (rowCount < colCount)
        {
            throw new InvalidOperationException(
                $"Underdetermined linear system: {rowCount} bonds for {colCount} cash-flow dates. " +
                $"Either provide bonds covering all intermediate coupon dates or set interpolateMissingPillars = true.");
        }

        // 4. Populate cash-flow matrix C and price vector p
        var matrixC = Matrix<double>.Build.Dense(rowCount, colCount);
        var vectorP = Vector<double>.Build.Dense(rowCount);

        for (int i = 0; i < rowCount; i++)
        {
            double rhsPrice = sortedBonds[i].Price;

            foreach (var (time, amount) in bondCashFlows[i])
            {
                int exactIdx = Array.BinarySearch(pillarTimes, time);
                if (exactIdx >= 0)
                {
                    // Cash-flow time matches a pillar time directly
                    matrixC[i, exactIdx] += amount;
                }
                else
                {
                    // Intermediate coupon falls between two pillar times:
                    // DF(t) = (1 - w) * DF(t_left) + w * DF(t_right)
                    int rightIdx = ~exactIdx;
                    if (rightIdx == 0)
                    {
                        // Between t = 0 (where DF(0) = 1.0) and first pillar t_0
                        double tRight = pillarTimes[0];
                        double w = time / tRight;

                        matrixC[i, 0] += amount * w;
                        rhsPrice -= amount * (1.0 - w); // Move known DF(0) = 1.0 term to RHS
                    }
                    else
                    {
                        int leftIdx = rightIdx - 1;
                        double tLeft = pillarTimes[leftIdx];
                        double tRight = pillarTimes[rightIdx];
                        double w = (time - tLeft) / (tRight - tLeft);

                        matrixC[i, leftIdx] += amount * (1.0 - w);
                        matrixC[i, rightIdx] += amount * w;
                    }
                }
            }

            vectorP[i] = rhsPrice;
        }

        // 5. Solve C * d = p (uses LU for square N x N systems, QR least-squares if N > M)
        Vector<double> discountFactors = matrixC.Solve(vectorP);

        // 6. Map solution back to (MaturityYears, DiscountFactor) pairs
        var result = new DiscountFactorPoint[colCount];
        for (int j = 0; j < colCount; j++)
        {
            result[j] = new DiscountFactorPoint(pillarTimes[j], discountFactors[j]);
        }

        return result;
    }

    private static List<(double Time, double Amount)> GenerateCashFlows(BondWithPrice bond)
    {
        if (bond.MaturityYears <= 0)
            throw new ArgumentOutOfRangeException(nameof(bond), "Bond maturity must be positive.");
        if (bond.FaceValue <= 0)
            throw new ArgumentOutOfRangeException(nameof(bond), "Face value must be positive.");

        var cashFlows = new List<(double Time, double Amount)>();

        // Zero-coupon bond
        if (bond.CompoundingFrequency <= 0 || bond.CouponPercent == 0.0)
        {
            cashFlows.Add((RoundTime(bond.MaturityYears), bond.FaceValue));
            return cashFlows;
        }

        double periodLength = 1.0 / bond.CompoundingFrequency;
        double couponPayment = bond.FaceValue * (bond.CouponPercent / 100.0) / bond.CompoundingFrequency;

        // Step backwards from maturity to generate exact coupon schedule
        var times = new List<double>();
        for (int k = 0; ; k++)
        {
            double t = RoundTime(bond.MaturityYears - k * periodLength);
            if (t <= 1e-9)
                break;
            times.Add(t);
        }

        times.Reverse();

        for (int i = 0; i < times.Count; i++)
        {
            bool isMaturity = i == times.Count - 1;
            double amount = isMaturity ? bond.FaceValue + couponPayment : couponPayment;
            cashFlows.Add((times[i], amount));
        }

        return cashFlows;
    }

    private static double RoundTime(double time) =>
        Math.Round(time, TimePrecisionDigits, MidpointRounding.AwayFromZero);
}

/// <summary>
/// C# 14 extension block for evaluating the discount curve at arbitrary maturities.
/// </summary>
public static class DiscountCurveExtensions
{
    private const double TimePrecision = 1e-6;

    extension(IReadOnlyList<DiscountFactorPoint> curve)
    {
        /// <summary>
        /// Evaluates the discount factor at any maturity T using log-linear interpolation.
        /// </summary>
        public double GetDiscountFactor(double maturityYears)
        {
            if (maturityYears <= 0)
                return 1.0;

            if (curve.Any(p => Math.Abs(p.MaturityYears - maturityYears) < TimePrecision))
                return curve.Single(p => Math.Abs(p.MaturityYears - maturityYears) < TimePrecision).DiscountFactor;

            int n = curve.Count;
            if (n == 0)
                throw new InvalidOperationException("Curve is empty.");

            // Before first pillar: log-linear between (0, 1.0) and first point
            if (maturityYears <= curve[0].MaturityYears)
            {
                double w = maturityYears / curve[0].MaturityYears;
                return Math.Pow(curve[0].DiscountFactor, w);
            }

            // After last pillar: flat zero-rate extrapolation
            if (maturityYears >= curve[n - 1].MaturityYears)
            {
                var last = curve[n - 1];
                return Math.Exp(-last.DiscountFactor.GetSpotYield(last.MaturityYears) * maturityYears);
            }

            // Between two pillars
            for (int i = 0; i < n - 1; i++)
            {
                var left = curve[i];
                var right = curve[i + 1];
                if (maturityYears >= left.MaturityYears && maturityYears <= right.MaturityYears)
                {
                    double w = (maturityYears - left.MaturityYears) / (right.MaturityYears - left.MaturityYears);
                    return Math.Pow(left.DiscountFactor, 1.0 - w) * Math.Pow(right.DiscountFactor, w);
                }
            }

            return curve[n - 1].DiscountFactor;
        }
    }

    public static double GetSpotYield(this double d, double time) 
        => d > 0 && time > 0
        ? -Math.Log(d) / time
        : throw new InvalidOperationException($"either discount factor {d:N4} or time {time} is invalid");
}
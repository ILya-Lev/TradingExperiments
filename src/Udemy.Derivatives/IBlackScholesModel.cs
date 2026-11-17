namespace Udemy.Derivatives;

public record Option(double Strike, double MaturityYears);
public record CallOption(double Strike, double MaturityYears) : Option(Strike, MaturityYears);
public record PutOption(double Strike, double MaturityYears) : Option(Strike, MaturityYears);

public interface IBlackScholesModel
{
    /// <summary> actual formula depends on the option type - so far either call or put </summary>
    double GetPrice(Option option, double timeToMaturity, double? spot = null);

    /// <summary> delta is a first partial derivative of the option price by the underlying price </summary>
    double GetDelta(Option option, double timeToMaturity, double? spot = null);

    /// <summary> gamma is the second partial derivative of the option price by the underlying price </summary>
    double GetGamma(Option option, double timeToMaturity, double? spot = null);

    /// <summary>
    /// despite being denoted with nu, vega is the first partial derivative by the volatility (sigma)
    /// is returned in percents (i.e., actual number is 100 times smaller as a decimal fraction)
    /// </summary>
    double GetVega(Option option, double timeToMaturity, double? spot = null);

    /// <summary>
    /// theta is a first partial derivative by time, thus represents time decay.
    /// How much the option price will change (usually drop) in 1 year.
    /// So one needs to rescale if the time period in focus is less than 1 Y.
    /// </summary>
    /// <param name="option"></param>
    /// <param name="timeToMaturity"></param>
    /// <param name="spot"></param>
    /// <returns></returns>
    double GetTheta(Option option, double timeToMaturity, double? spot = null);
}
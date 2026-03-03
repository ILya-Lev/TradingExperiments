namespace Udemy.Fin.Stat;

public record FxRate
{
    public DateOnly Date { get; init; }
    public decimal Rate { get; init; }
}

public record ExIndex
{
    public DateOnly Date { get; init; }
    public decimal Open { get; init; }
    public decimal High { get; init; }
    public decimal Low { get; init; }
    public decimal Close { get; init; }
    public decimal AdjClose { get; init; }
    public decimal Volume { get; init; }
}

public record ExOhlc
{
    public DateOnly Date { get; init; }
    public decimal Open { get; init; }
    public decimal High { get; init; }
    public decimal Low { get; init; }
    public decimal Close { get; init; }
}

public readonly record struct PricesWithReturns(
    DateOnly Date,
    decimal Price,
    decimal DailyGross,
    decimal DailyNet,
    decimal ContinuouslyCompound);


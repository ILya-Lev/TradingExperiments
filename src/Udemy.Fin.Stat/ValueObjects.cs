namespace Udemy.Fin.Stat;

public readonly record struct FxRate(DateOnly Date, decimal Rate);

public readonly record struct ExIndex(
    DateOnly Date,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal AdjClose,
    decimal Volume);

public readonly record struct ExOhlc(
    DateOnly Date,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close);

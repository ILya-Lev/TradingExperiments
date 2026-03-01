namespace Udemy.Fin.Stat;

public static class Utils
{
    extension(decimal nominator)
    {
        public decimal ToBp() => Math.Round(nominator * 10_000, 0);

        public decimal DivideZeroGuarded(decimal denominator)
        {
            if (Math.Abs(denominator) < 1e-10m)
                throw new InvalidOperationException($"current value {nominator}, previous {denominator}");

            return nominator / denominator;
        }
    }

    extension(DateOnly date)
    {
        public int ToStamp() => date.Year * 10_000 + date.Month * 100 + date.Day;
    }
}
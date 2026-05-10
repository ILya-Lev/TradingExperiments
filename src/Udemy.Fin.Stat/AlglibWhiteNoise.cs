namespace Udemy.Fin.Stat;

public static class AlgLibWhiteNoise
{
    public static IEnumerable<double> GenerateWhiteNoise(double mean = 0, double stdDev = 1.0)
    {
        //seeds the generator with the current time/entropy. For determinism in testing use:
        //alglib.hqrndseed(1,2,out var state);
        //hqrnd = high quality random
        alglib.hqrndrandomize(out var state);

        while(true)
        {
            yield return alglib.hqrndnormal(state) * stdDev + mean;
        }
        // ReSharper disable once IteratorNeverReturns
    }

    public static IEnumerable<double> GenerateRandomWalk(IEnumerable<double> whiteNoise)
    {
        var current = 0.0;
        foreach (var step in whiteNoise)
        {
            current += step;
            yield return current;
        }
    }
}

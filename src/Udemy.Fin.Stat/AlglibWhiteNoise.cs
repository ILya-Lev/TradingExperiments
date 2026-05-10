namespace Udemy.Fin.Stat;

public static class AlglibWhiteNoise
{
    public static IEnumerable<double> Generate(double stdDev = 1.0)
    {
        //seeds the generator with the current time/entropy. For determinism in testing use:
        //alglib.hqrndseed(1,2,out var state);
        alglib.hqrndrandomize(out var state);

        while(true)
        {
            yield return alglib.hqrndnormal(state) * stdDev;
        }
        // ReSharper disable once IteratorNeverReturns
    }
}

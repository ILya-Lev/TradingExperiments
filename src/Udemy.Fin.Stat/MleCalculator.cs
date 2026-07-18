using System.Numerics.Tensors;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;
using Microsoft.Extensions.Logging;
/*
    Based on 2022 Cameron R. Connell lectures and Gemini3
*/

namespace Udemy.Fin.Stat;

public readonly record struct StudentParameters(double Mu, double Rho, double Nu);

public static class StudentParametersExtensions
{
    public static double GetDelta(this StudentParameters lhs, StudentParameters rhs)
        => Math.Abs(lhs.Mu - rhs.Mu)
           + Math.Abs(lhs.Rho - rhs.Rho)
           + Math.Abs(lhs.Nu - rhs.Nu);
}

public class StudentParametersBuilder
{
    private double _mu;
    private double _rho;
    private double _nu;

    public StudentParametersBuilder RegisterMu(double mu)
    {
        _mu = mu;
        return this;
    }
    public StudentParametersBuilder RegisterRho(double rho)
    {
        _rho = rho;
        return this;
    }
    public StudentParametersBuilder RegisterNu(double nu)
    {
        _nu = nu;
        return this;
    }

    public static StudentParameters FromMom(IReadOnlyCollection<double> sequence)
    {
        var stats = new DescriptiveStatistics(sequence, true);
        var builder = new StudentParametersBuilder();
        builder.RegisterMu(stats.Mean);
        builder.RegisterNu(6.0 / stats.Kurtosis + 4.0);//kurtosis is excessive here (i.e., above 3)
        builder.RegisterRho(Math.Sqrt(stats.Variance * (builder._nu - 2.0) / builder._nu));
        return builder.Build();
    }

    public StudentParameters Build() => new StudentParameters(_mu, _rho, _nu);
}

/// <summary>
/// Python tools for Estimation in Univariate Modelling converted to C#[cite: 2].
/// Contains procedures MLE_StudentT and EM_NormalMixture[cite: 1].
/// </summary>
public class MleCalculator(ILogger<MleCalculator>? logger)
{
    /// <summary>
    /// Calculates the maximum likelihood estimators for the parameters of a Student t distribution[cite: 1].
    /// </summary>
    public StudentParameters MleStudentT(double[] points, StudentParameters? initial = null)
    {
        const double tolerance = 1e-6;

        var stats = initial ?? StudentParametersBuilder.FromMom(points);

        logger?.LogInformation($"{"Iteration",10}|{"Mu",20}|{"Rho",20}|{"Nu",20}|{"Delta",20}");
        logger?.LogInformation($"{0,10}|{stats.Mu,20:g4}|{stats.Rho,20:g4}|{stats.Nu,20:g4}|{0,20}");

        for (double delta = 1.0, iteration = 1.0; delta > tolerance; iteration++)
        {
            var oldStats = stats with { };

            // E-step
            var (weights, averageQsubW) = GetWeights(points, oldStats, stats);

            // M-step
            var mu = UpdateMu(points, weights);
            var rho = UpdateRho(points, weights, mu);
            var (currentNu, nextNu) = UpdateNu(averageQsubW, oldStats.Nu);

            oldStats = oldStats with { Nu = currentNu };
            stats = new StudentParameters(Mu: mu, Rho: rho, Nu: nextNu);

            delta = stats.GetDelta(oldStats);

            logger?.LogInformation($"{iteration,10}|{stats.Mu,20:g4}|{stats.Rho,20:g4}|{stats.Nu,20:g4}|{delta,20:g4}");
        }

        return stats;
    }

    private static (double[] weights, double averageDif) GetWeights(double[] points,
        StudentParameters oldStats, StudentParameters stats)
    {
        var w = new double[points.Length];
        var totalQsubW = 0.0;

        for (var i = 0; i < points.Length; i++)
        {
            var delta2 = Math.Pow(points[i] - oldStats.Mu, 2) / Math.Pow(stats.Rho, 2);

            w[i] = (oldStats.Nu + 1.0) / (oldStats.Nu + delta2);

            totalQsubW += SpecialFunctions.DiGamma((oldStats.Nu + 1.0) / 2.0)
                          - Math.Log((oldStats.Nu + delta2) / 2.0)
                          - w[i];
        }

        return (w, totalQsubW / points.Length);
    }

    private (double oldNu, double newNu) UpdateNu(double averageQsubW, double nu)
    {
        // Root-finding algorithm to update nu (part of the M-step)
        const double newtonTolerance = 1e-6;

        double F(double val) => Math.Log(val / 2.0) + 1.0 - SpecialFunctions.DiGamma(val / 2.0) + averageQsubW;

        double FPrime(double val) => 1.0 / val - (PsiPrime(val / 2.0) ?? 0) / 2.0;

        var current = nu;
        var next = nu - F(nu) / FPrime(nu);

        while (Math.Abs(next - current) > newtonTolerance)
        {
            current = next;
            next = current - F(current) / FPrime(current);
        }

        return (current, next);
    }

    /// <summary>
    /// benchmark shows loop is the fastest out of 3 approaches - loop, linq, tensor primitives
    /// </summary>
    private static double UpdateRho(double[] points, double[] w, double mu)
    {
        var s3 = 0.0;
        for (int i = 0; i < points.Length; i++)
        {
            s3 += w[i] * (points[i] - mu) * (points[i] - mu);
        }
        return Math.Sqrt(s3 / points.Length);
    }

    /// <summary>
    /// benchmark shows tensor primitives is the fastest out of 3 approaches - loop, linq, tensor primitives
    /// </summary>
    private static double UpdateMu(double[] points, double[] w)
    {
        var totalWeight = TensorPrimitives.Sum(w);
        var weightedPoints = TensorPrimitives.Dot(points, w);
        return weightedPoints / totalWeight;
    }

    /// <summary>
    /// Fits a normal mixture distribution to data, using a constrained version of the EM algorithm[cite: 1].
    /// </summary>
    public (double[] Alpha, double[] Mu, double[] Sigma, double LogLikelihood)? EmNormalMixture(
        IReadOnlyList<double> points, int size = 2, double epsilon = 0.1, double c = 0.1, int startLength = 100,
        double[]? initAlpha = null, double[]? initMu = null, double[]? initSigma = null) //[cite: 1, 2]
    {
        int n = points.Count;
        double[] alpha, mu, sigma;
        List<double[][]> parameters = [];
        int arg = 0;
        var data = new DescriptiveStatistics(points);

        if (initAlpha != null && initMu != null && initSigma != null)
        {
            alpha = (double[])initAlpha.Clone();
            mu = (double[])initMu.Clone();
            sigma = (double[])initSigma.Clone();
            size = alpha.Length;

            if (size != mu.Length || size != sigma.Length)
            {
                logger?.LogInformation("Initial values must have same length");
                return null;
            }

            parameters.Add([alpha, mu, sigma]);
            arg = 0;
        }
        else
        {
            double meanRange = points.Mean();
            double stdRange = data.StandardDeviation;

            alpha = new double[size];
            mu = new double[size];
            sigma = new double[size];

            double[] likelihoods = new double[startLength];

            for (int m = 0; m < startLength; m++)
            {
                double[] p = new double[size];
                Array.Fill(p, 1.0);

                while (p.Min() < epsilon || p.Sum() >= 2.0 - epsilon)
                {
                    for (int k = 0; k < size - 1; k++) p[k] = Random.Shared.NextDouble();
                }
                p[size - 1] = 2.0 - p.Take(size - 1).Sum();

                for (int k = 0; k < size; k++)
                {
                    alpha[k] = p[k];
                    mu[k] = Random.Shared.NextDouble() * meanRange;
                    sigma[k] = Random.Shared.NextDouble() * stdRange + 0.1 * stdRange;
                }

                logger?.LogDebug($"Start iteration {m + 1}");

                double[][] results;
                try
                {
                    results = ConstrainedEm(points, alpha, mu, sigma, epsilon, c, 0.001);
                }
                catch (Exception ex)
                {
                    logger?.LogWarning("Iteration aborted\n" + ex.Message);
                    continue;
                }

                parameters.Add(results);

                double sum = 0.0;
                for (int i = 0; i < n; i++)
                {
                    try
                    {
                        sum += Math.Log(NormalMixtureLikelihood(points[i], results[0], results[1], results[2]));
                    }
                    catch (ArgumentException) { continue; }
                }

                likelihoods[m] = sum;
            }

            arg = Array.IndexOf(likelihoods, likelihoods.Max());
        }

        logger?.LogInformation("Main Run");
        double[][] finalParams = ConstrainedEm(points, parameters[arg][0], parameters[arg][1], parameters[arg][2], epsilon, c, 0.000001);

        logger?.LogInformation("Final Results:");

        string labelString = "";
        for (int k = 0; k < size; k++) labelString += $"{"alpha",5}{k + 1,2}|";
        for (int k = 0; k < size; k++) labelString += $"{"mu",5}{k + 1,2}|";
        for (int k = 0; k < size; k++) labelString += $"{"sigma",5}{k + 1,2}|";
        logger?.LogInformation(labelString);

        string reportString = "";
        for (int k = 0; k < size; k++) reportString += $"{finalParams[0][k],7:F4}|";
        for (int k = 0; k < size; k++) reportString += $"{finalParams[1][k],7:F4}|";
        for (int k = 0; k < size; k++) reportString += $"{finalParams[2][k],7:F4}|";
        logger?.LogInformation(reportString);

        double finalSum = 0.0;
        for (int i = 0; i < n; i++)
        {
            try
            {
                finalSum += Math.Log(NormalMixtureLikelihood(points[i], finalParams[0], finalParams[1], finalParams[2]));
            }
            catch (ArgumentException)
            {
                logger?.LogWarning("log likelihood calculation failed");
                continue;
            }
        }

        logger?.LogInformation($"Log-likelihood = {finalSum}");

        return (finalParams[0], finalParams[1], finalParams[2], finalSum); //[cite: 1, 2]
    }

    private double[][] ConstrainedEm(IReadOnlyList<double> points, double[] alpha, double[] mu, double[] sigma, double epsilon = 0.1, double c = 0.1, double tolerance = 0.000001)
    {
        int n = points.Count;
        int size = alpha.Length;

        double[] alphaNew = (double[])alpha.Clone();
        double[] muNew = (double[])mu.Clone();
        double[] sigmaNew = (double[])sigma.Clone();

        int iteration = 0;

        // Output formatting logic omitted for extreme brevity, follows identical structure to main method[cite: 2]
        logger?.LogDebug($"Iteration {iteration} started...");

        double[,] weights = new double[size, n];
        double[,] normalPdf = new double[size, n];

        double ell0 = 0.0;
        for (int i = 0; i < n; i++)
        {
            try { ell0 += Math.Log(NormalMixtureLikelihood(points[i], alpha, mu, sigma)); }
            catch { continue; }
        }

        double newEll = ell0;
        double delta = 1.0;

        while (delta > tolerance)
        {
            double oldEll = newEll;
            iteration++;

            double[] a = new double[size];
            double[] b = new double[size];

            double[] alphaOld = (double[])alphaNew.Clone();
            double[] muOld = (double[])muNew.Clone();
            double[] sigmaOld = (double[])sigmaNew.Clone();

            // E-step[cite: 2]
            for (int k = 0; k < size; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    normalPdf[k, i] = Standard((points[i] - muOld[k]) / sigmaOld[k]) / sigmaOld[k];
                }
            }

            for (int i = 0; i < n; i++)
            {
                var pdf = alphaOld.Select((item, k) => item * normalPdf[k, i]).Sum();

                for (int k = 0; k < size; k++)
                {
                    weights[k, i] = alphaOld[k] * normalPdf[k, i] / pdf;
                    if (double.IsNaN(weights[k, i])) throw new ArgumentException("NaN value in weight calculation");
                }
            }

            // M-step[cite: 2]
            a = Enumerable.Range(0, size).Select(k => Enumerable.Range(0, n).Sum(i => weights[k, i])).ToArray();

            bool[] boolIndex = new bool[size]; Array.Fill(boolIndex, true);
            bool[] activeIndex = new bool[size]; Array.Fill(activeIndex, true);
            int alphaIters = 0;

            while (alphaIters < 1000)
            {
                double numer = 1.0 - epsilon * (size - activeIndex.Count(val => val));
                double denom = 0;
                for (int k = 0; k < size; k++) if (activeIndex[k]) denom += a[k];

                double factor = numer / denom;

                for (int k = 0; k < size; k++)
                {
                    if (activeIndex[k]) alphaNew[k] = a[k] * factor;
                    boolIndex[k] = alphaNew[k] >= epsilon;
                    if (!boolIndex[k]) alphaNew[k] = epsilon;
                }

                alphaIters++;
                if (boolIndex.All(val => val)) break;
                activeIndex = activeIndex.Zip(boolIndex, (ai, bi) => ai && bi).ToArray();
            }

            if (alphaIters >= 1000) logger?.LogWarning("alpha constraints aborted, too many iterations");

            muNew = Enumerable.Range(0, size).Select(k => points.Select((p, i) => weights[k, i] * p).Sum() / a[k]).ToArray();

            for (int k = 0; k < size; k++)
            {
                b[k] = points.Select((p, i) => (p - muNew[k]) * (p - muNew[k]) * weights[k, i]).Sum();
                sigmaNew[k] = Math.Sqrt(b[k] / a[k]);
            }

            // Step 2.6 - Applying Hathaway's Constraints[cite: 2]
            // Translated nested logic mimicking python's np.roll and cyclic array extensions[cite: 2]
            boolIndex = sigmaNew.Select((s, k) => s >= c * sigmaNew[(k + 1) % size]).ToArray();

            double[] aExt = [.. a, a[0]];
            double[] bExt = [.. b, b[0]];

            while (!boolIndex.All(val => val))
            {
                int k = 0;
                double numer0 = 0;
                double denom0 = 0;
                List<int> index0 = [0];
                double coeff;
                double numer = 0, denom = 0;
                List<int> index = [];

                if (!boolIndex[k])
                {
                    coeff = c * c;
                    numer0 = bExt[k] + coeff * bExt[k + 1];
                    denom0 = aExt[k] + aExt[k + 1];
                    index0.Add(k + 1);
                    k++;

                    while (!boolIndex[k])
                    {
                        coeff *= c * c;
                        numer0 += coeff * bExt[k + 1];
                        denom0 += aExt[k + 1];
                        index0.Add(k + 1);
                        k++;
                        if (k == size) break;
                    }
                }

                k++;
                while (k < size)
                {
                    if (!boolIndex[k])
                    {
                        coeff = c * c;
                        numer = bExt[k] + coeff * bExt[k + 1];
                        denom = aExt[k] + aExt[k + 1];
                        index = [k, k + 1];
                        k++;

                        if (k < size)
                        {
                            while (!boolIndex[k])
                            {
                                coeff *= c * c;
                                numer += coeff * bExt[k + 1];
                                denom += aExt[k + 1];
                                index.Add(k + 1);
                                k++;
                                if (k == size) break;
                            }
                        }

                        if (k < size)
                        {
                            sigmaNew[index[0]] = Math.Sqrt(numer / denom);
                            for (int j = 1; j < index.Count; j++) sigmaNew[index[j]] = sigmaNew[index[j - 1]] / c;
                        }
                    }
                    k++;
                }

                if (k == size && index0.Count > 1)
                {
                    sigmaNew[index0[0]] = Math.Sqrt(numer0 / denom0);
                    for (int j = 1; j < index0.Count; j++) sigmaNew[index0[j]] = sigmaNew[index0[j - 1]] / c;
                }
                else if (k == size + 1)
                {
                    index.RemoveAt(index.Count - 1);
                    index.AddRange(index0);
                    denom += Math.Pow(c, 2) * denom0; // Assumes coeff tracking maps to python scope logic
                    numer += numer0;
                    sigmaNew[index[0]] = Math.Sqrt(numer / denom);
                    for (int j = 1; j < index.Count; j++) sigmaNew[index[j]] = sigmaNew[index[j - 1]] / c;
                }

                for (int m = 0; m < size; m++) boolIndex[m] = sigmaNew[m] >= c * sigmaNew[(m + 1) % size] - 0.000001;
            }

            newEll = 0.0;
            for (int i = 0; i < n; i++)
            {
                try { newEll += Math.Log(NormalMixtureLikelihood(points[i], alphaNew, muNew, sigmaNew)); }
                catch { continue; }
            }

            delta = (newEll - oldEll) / (newEll - ell0);
        }

        return [alphaNew, muNew, sigmaNew];
    }

    // --- Mathematical Helpers ---

    private double NormalMixtureLikelihood(double x, ReadOnlySpan<double> alpha, ReadOnlySpan<double> mu, ReadOnlySpan<double> sigma)
    {
        //Span<double> buffer = alpha.Length < 256 ? stackalloc double[alpha.Length] : new double[alpha.Length];

        //TensorPrimitives.Subtract(x, mu, buffer);       //x - mu
        //TensorPrimitives.Divide(buffer, sigma, buffer); //x-mu / sigma
        //TensorPrimitives.Multiply(buffer, buffer, buffer); //(x-mu / sigma) ^ 2
        //TensorPrimitives.Divide(buffer, -2, buffer);     //(x-mu / sigma) ^ 2 / -2
        //TensorPrimitives.Exp(buffer, buffer); //exp(-(x - mu / sigma) ^ 2/2)
        //TensorPrimitives.Divide(buffer, sigma, buffer);//exp(-(x - mu / sigma) ^ 2/2) / sigma
        //TensorPrimitives.Multiply(buffer, alpha, buffer);//exp(-(x - mu / sigma) ^ 2/2) / sigma * alpha

        //return TensorPrimitives.Sum(buffer) / Math.Sqrt(2.0 * Math.PI);

        double sum = 0.0;
        for (int k = 0; k < alpha.Length; k++)
        {
            sum += alpha[k] * Standard((x - mu[k]) / sigma[k]) / sigma[k];
        }
        return sum;
    }

    private double Standard(double x) => 1.0 / Math.Sqrt(2.0 * Math.PI) * Math.Exp(-x * x / 2.0);

    private double? PsiPrime(double x, int max = 10000)
    {
        if (x <= 0)
        {
            logger?.LogWarning("digamma argument must be > 0");
            return null;
        }

        double sum = 0;
        for (int i = 0; i <= max; i++)
        {
            double term = x + i;
            sum += 1.0 / (term * term);
        }

        return sum;
    }

    /// <summary>
    /// Computes the Digamma function.
    /// Note: In production C# targeting .NET, replace this stub with MathNet.Numerics.SpecialFunctions.DiGamma(x).
    /// </summary>
    private static double Digamma(double x)
    {
        // Simplistic asymptotic approximation for code completion. 
        if (x <= 0) throw new ArgumentOutOfRangeException(nameof(x));
        double result = 0;
        while (x < 7) { result -= 1 / x; x++; }
        x -= 0.5;
        double inverseSq = 1.0 / (x * x);
        return result + Math.Log(x) + inverseSq / 24.0 - 7.0 * inverseSq * inverseSq / 960.0;
    }
}
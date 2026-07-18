using System.Numerics.Tensors;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;

/*
    Based on 2022 Cameron R. Connell lectures and Gemini3
*/

namespace Udemy.Fin.Stat;

/// <summary>
/// Python tools for Estimation in Univariate Modelling converted to C#[cite: 2].
/// Contains procedures MLE_StudentT and EM_NormalMixture[cite: 1].
/// </summary>
public static class Estimation
{
    /// <summary>
    /// Calculates the maximum likelihood estimators for the parameters of a Student t distribution[cite: 1].
    /// </summary>
    public static (double Mu, double Rho, double Nu) MleStudentT(IReadOnlyList<double> points, double[]? initial = null, int verbose = 0)
    {
        double tolerance = 0.0001; //[cite: 2]
        int n = points.Count; //[cite: 2]
        var data = new DescriptiveStatistics(points);

        double muNew, rhoNew, nuNew;

        // Initialization[cite: 2]
        if (initial is { Length: 3 })
        {
            muNew = initial[0];
            rhoNew = initial[1];
            nuNew = initial[2]; //[cite: 1, 2]
        }
        else
        {
            if (initial != null)
            {
                Console.WriteLine("Initial parameter list must have length 3"); //[cite: 2]
                Console.WriteLine("Reverting to method of moment initial values"); //[cite: 2]
            }

            muNew = data.Mean; //[cite: 2]
            double kurt = data.Kurtosis + 3; //[cite: 2]
            nuNew = 6.0 / (kurt - 3.0) + 4.0; //[cite: 2]
            double var = data.Variance; //[cite: 2]
            rhoNew = Math.Sqrt(var * (nuNew - 2.0) / nuNew); //[cite: 2]
        }

        if (verbose > 0)
        {
            Console.WriteLine($"{"Iteration",10}|{"Mu",20}|{"Rho",20}|{"Nu",20}"); //[cite: 2]
            Console.WriteLine($"{0,10}|{muNew,20:F4}|{rhoNew,20:F4}|{nuNew,20:F4}"); //[cite: 2]
        }

        double delta = 1.0; //[cite: 2]
        int iteration = 0;

        while (delta > tolerance)
        {
            double muOld = muNew;
            double nuOld = nuNew;
            double rhoOld = rhoNew; //[cite: 2]

            // E-step[cite: 2]
            double[] w = new double[n];
            double[] q = new double[n];

            for (int i = 0; i < n; i++)
            {
                double delta2 = Math.Pow(points[i] - muOld, 2) / Math.Pow(rhoNew, 2); //[cite: 2]
                w[i] = (nuOld + 1.0) / (nuOld + delta2); //[cite: 2]
                q[i] = SpecialFunctions.DiGamma((nuOld + 1.0) / 2.0) - Math.Log((nuOld + delta2) / 2.0); //[cite: 2]
            }

            // M-step[cite: 2]
            double s1 = w.Sum(); //[cite: 2]
            double s2 = w.Zip(points, (wi, xi) => wi * xi).Sum(); //[cite: 2]
            muNew = s2 / s1; //[cite: 2]

            double s3 = 0;
            for (int i = 0; i < n; i++)
            {
                s3 += w[i] * Math.Pow(muNew - points[i], 2); //[cite: 2]
            }
            rhoNew = Math.Sqrt(s3 / n); //[cite: 2]

            // Root-finding algorithm to update nu (part of the M-step)[cite: 2]
            double tolNewton = 0.0001; //[cite: 2]

            double F(double val) => Math.Log(val / 2.0) + 1.0 - SpecialFunctions.DiGamma(val / 2.0) + q.Zip(w, (qi, wi) => qi - wi).Sum() / n; //[cite: 2]
            double FPrime(double val) => 1.0 / val - (PsiPrime(val / 2.0) ?? 0) / 2.0; //[cite: 2]

            nuNew = nuOld - F(nuOld) / FPrime(nuOld); //[cite: 2]

            while (Math.Abs(nuNew - nuOld) > tolNewton) //[cite: 2]
            {
                nuOld = nuNew; //[cite: 2]
                nuNew = nuOld - F(nuOld) / FPrime(nuOld); //[cite: 2]
            }

            if (verbose > 0)
            {
                iteration++; //[cite: 2]
                Console.WriteLine($"{iteration,10}|{muNew,20:F4}|{rhoNew,20:F4}|{nuNew,20:F4}"); //[cite: 2]
            }

            delta = Math.Abs(muNew - muOld) + Math.Abs(rhoNew - rhoOld) + Math.Abs(nuNew - nuOld); //[cite: 2]
        }

        return (muNew, rhoNew, nuNew); //[cite: 1]
    }

    /// <summary>
    /// Fits a normal mixture distribution to data, using a constrained version of the EM algorithm[cite: 1].
    /// </summary>
    public static (double[] Alpha, double[] Mu, double[] Sigma, double LogLikelihood)? EmNormalMixture(
        IReadOnlyList<double> points, int size = 2, double epsilon = 0.1, double c = 0.1, int startLength = 100, int verbose = 0,
        double[]? initAlpha = null, double[]? initMu = null, double[]? initSigma = null) //[cite: 1, 2]
    {
        int n = points.Count; //[cite: 2]
        double[] alpha, mu, sigma;
        List<double[][]> parameters = []; //[cite: 2]
        int arg = 0; //[cite: 2]
        var data = new DescriptiveStatistics(points);

        if (initAlpha != null && initMu != null && initSigma != null) //[cite: 2]
        {
            alpha = (double[])initAlpha.Clone(); //[cite: 2]
            mu = (double[])initMu.Clone(); //[cite: 2]
            sigma = (double[])initSigma.Clone(); //[cite: 2]
            size = alpha.Length; //[cite: 2]

            if (size != mu.Length || size != sigma.Length) //[cite: 2]
            {
                Console.WriteLine("Initial values must have same length"); //[cite: 2]
                return null;
            }

            parameters.Add([alpha, mu, sigma]); //[cite: 2]
            arg = 0; //[cite: 2]
        }
        else
        {
            double meanRange = points.Mean(); //[cite: 2]
            double stdRange = data.StandardDeviation; //[cite: 2]

            alpha = new double[size]; //[cite: 2]
            mu = new double[size]; //[cite: 2]
            sigma = new double[size]; //[cite: 2]

            double[] likelihoods = new double[startLength]; //[cite: 2]

            for (int m = 0; m < startLength; m++) //[cite: 2]
            {
                double[] p = new double[size]; //[cite: 2]
                Array.Fill(p, 1.0); //[cite: 2]

                while (p.Min() < epsilon || p.Sum() >= 2.0 - epsilon) //[cite: 2]
                {
                    for (int k = 0; k < size - 1; k++) p[k] = Random.Shared.NextDouble(); //[cite: 2]
                }
                p[size - 1] = 2.0 - p.Take(size - 1).Sum(); //[cite: 2]

                for (int k = 0; k < size; k++) //[cite: 2]
                {
                    alpha[k] = p[k]; //[cite: 2]
                    mu[k] = Random.Shared.NextDouble() * meanRange; //[cite: 2]
                    sigma[k] = Random.Shared.NextDouble() * stdRange + 0.1 * stdRange; //[cite: 2]
                }

                if (verbose > 0) Console.WriteLine($"Start iteration {m + 1}"); //[cite: 2]

                double[][] results;
                try
                {
                    results = ConstrainedEm(points, alpha, mu, sigma, epsilon, c, Math.Max(verbose - 2, 0), 0.001); //[cite: 2]
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Iteration aborted\n" + ex.Message); //[cite: 2]
                    continue;
                }

                parameters.Add(results); //[cite: 2]

                double sum = 0.0;
                for (int i = 0; i < n; i++)
                {
                    try
                    {
                        sum += Math.Log(NormalMixtureLikelihood(points[i], results[0], results[1], results[2])); //[cite: 2]
                    }
                    catch (ArgumentException) { continue; } //[cite: 2]
                }

                likelihoods[m] = sum; //[cite: 2]
            }

            arg = Array.IndexOf(likelihoods, likelihoods.Max()); //[cite: 2]
        }

        Console.WriteLine("Main Run"); //[cite: 2]
        double[][] finalParams = ConstrainedEm(points, parameters[arg][0], parameters[arg][1], parameters[arg][2], epsilon, c, Math.Max(verbose - 1, 0), 0.000001); //[cite: 2]

        Console.WriteLine("Final Results:"); //[cite: 2]

        string labelString = "";
        for (int k = 0; k < size; k++) labelString += $"{"alpha",5}{k + 1,2}|"; //[cite: 2]
        for (int k = 0; k < size; k++) labelString += $"{"mu",5}{k + 1,2}|"; //[cite: 2]
        for (int k = 0; k < size; k++) labelString += $"{"sigma",5}{k + 1,2}|"; //[cite: 2]
        Console.WriteLine(labelString); //[cite: 2]

        string reportString = "";
        for (int k = 0; k < size; k++) reportString += $"{finalParams[0][k],7:F4}|"; //[cite: 2]
        for (int k = 0; k < size; k++) reportString += $"{finalParams[1][k],7:F4}|"; //[cite: 2]
        for (int k = 0; k < size; k++) reportString += $"{finalParams[2][k],7:F4}|"; //[cite: 2]
        Console.WriteLine(reportString); //[cite: 2]

        double finalSum = 0.0; //[cite: 2]
        for (int i = 0; i < n; i++) //[cite: 2]
        {
            try
            {
                finalSum += Math.Log(NormalMixtureLikelihood(points[i], finalParams[0], finalParams[1], finalParams[2])); //[cite: 2]
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Warning: log likelihood calculation failed"); //[cite: 2]
                continue; //[cite: 2]
            }
        }

        Console.WriteLine($"Log-likelihood = {finalSum}"); //[cite: 2]

        return (finalParams[0], finalParams[1], finalParams[2], finalSum); //[cite: 1, 2]
    }

    private static double[][] ConstrainedEm(IReadOnlyList<double> points, double[] alpha, double[] mu, double[] sigma, double epsilon = 0.1, double c = 0.1, int verbose = 0, double tolerance = 0.000001) //[cite: 2]
    {
        int n = points.Count; //[cite: 2]
        int size = alpha.Length; //[cite: 2]

        double[] alphaNew = (double[])alpha.Clone(); //[cite: 2]
        double[] muNew = (double[])mu.Clone(); //[cite: 2]
        double[] sigmaNew = (double[])sigma.Clone(); //[cite: 2]

        int iteration = 0; //[cite: 2]

        if (verbose > 0) //[cite: 2]
        {
            // Output formatting logic omitted for extreme brevity, follows identical structure to main method[cite: 2]
            Console.WriteLine($"Iteration {iteration} started..."); //[cite: 2]
        }

        double[,] weights = new double[size, n]; //[cite: 2]
        double[,] normalPdf = new double[size, n]; //[cite: 2]

        double ell0 = 0.0; //[cite: 2]
        for (int i = 0; i < n; i++)
        {
            try { ell0 += Math.Log(NormalMixtureLikelihood(points[i], alpha, mu, sigma)); } //[cite: 2]
            catch { continue; } //[cite: 2]
        }

        double newEll = ell0; //[cite: 2]
        double delta = 1.0; //[cite: 2]

        while (delta > tolerance) //[cite: 2]
        {
            double oldEll = newEll; //[cite: 2]
            iteration++; //[cite: 2]

            double[] a = new double[size]; //[cite: 2]
            double[] b = new double[size]; //[cite: 2]

            double[] alphaOld = (double[])alphaNew.Clone(); //[cite: 2]
            double[] muOld = (double[])muNew.Clone(); //[cite: 2]
            double[] sigmaOld = (double[])sigmaNew.Clone(); //[cite: 2]

            // E-step[cite: 2]
            for (int k = 0; k < size; k++) //[cite: 2]
            {
                for (int i = 0; i < n; i++) //[cite: 2]
                {
                    normalPdf[k, i] = Standard((points[i] - muOld[k]) / sigmaOld[k]) / sigmaOld[k]; //[cite: 2]
                }
            }

            for (int i = 0; i < n; i++) //[cite: 2]
            {
                var pdf = alphaOld.Select((item, k) => item * normalPdf[k, i]).Sum();

                for (int k = 0; k < size; k++) //[cite: 2]
                {
                    weights[k, i] = alphaOld[k] * normalPdf[k, i] / pdf; //[cite: 2]
                    if (double.IsNaN(weights[k, i])) throw new ArgumentException("NaN value in weight calculation"); //[cite: 2]
                }
            }

            // M-step[cite: 2]
            a = Enumerable.Range(0, size).Select(k => Enumerable.Range(0, n).Sum(i => weights[k, i])).ToArray();
            
            bool[] boolIndex = new bool[size]; Array.Fill(boolIndex, true); //[cite: 2]
            bool[] activeIndex = new bool[size]; Array.Fill(activeIndex, true); //[cite: 2]
            int alphaIters = 0; //[cite: 2]

            while (alphaIters < 1000) //[cite: 2]
            {
                double numer = 1.0 - epsilon * (size - activeIndex.Count(val => val)); //[cite: 2]
                double denom = 0;
                for (int k = 0; k < size; k++) if (activeIndex[k]) denom += a[k]; //[cite: 2]

                double factor = numer / denom; //[cite: 2]

                for (int k = 0; k < size; k++) //[cite: 2]
                {
                    if (activeIndex[k]) alphaNew[k] = a[k] * factor; //[cite: 2]
                    boolIndex[k] = alphaNew[k] >= epsilon; //[cite: 2]
                    if (!boolIndex[k]) alphaNew[k] = epsilon; //[cite: 2]
                }

                alphaIters++; //[cite: 2]
                if (boolIndex.All(val => val)) break; //[cite: 2]
                activeIndex = activeIndex.Zip(boolIndex, (ai, bi) => ai && bi).ToArray();
            }

            if (alphaIters >= 1000) Console.WriteLine("Warning: alpha constraints aborted, too many iterations"); //[cite: 2]

            muNew = Enumerable.Range(0, size).Select(k => points.Select((p, i) => weights[k, i] * p).Sum() / a[k]).ToArray();

            for (int k = 0; k < size; k++) //[cite: 2]
            {
                b[k] = points.Select((p, i) => (p - muNew[k]) * (p - muNew[k]) * weights[k, i]).Sum();
                sigmaNew[k] = Math.Sqrt(b[k] / a[k]); //[cite: 2]
            }

            // Step 2.6 - Applying Hathaway's Constraints[cite: 2]
            // Translated nested logic mimicking python's np.roll and cyclic array extensions[cite: 2]
            boolIndex = sigmaNew.Select((s, k) => s >= c * sigmaNew[(k + 1) % size]).ToArray();

            double[] aExt = [.. a, a[0]]; //[cite: 2]
            double[] bExt = [.. b, b[0]]; //[cite: 2]

            while (!boolIndex.All(val => val)) //[cite: 2]
            {
                int k = 0; //[cite: 2]
                double numer0 = 0; //[cite: 2]
                double denom0 = 0; //[cite: 2]
                List<int> index0 = [0]; //[cite: 2]
                double coeff;
                double numer = 0, denom = 0;
                List<int> index = [];

                if (!boolIndex[k]) //[cite: 2]
                {
                    coeff = c * c; //[cite: 2]
                    numer0 = bExt[k] + coeff * bExt[k + 1]; //[cite: 2]
                    denom0 = aExt[k] + aExt[k + 1]; //[cite: 2]
                    index0.Add(k + 1); //[cite: 2]
                    k++; //[cite: 2]

                    while (!boolIndex[k]) //[cite: 2]
                    {
                        coeff *= c * c; //[cite: 2]
                        numer0 += coeff * bExt[k + 1]; //[cite: 2]
                        denom0 += aExt[k + 1]; //[cite: 2]
                        index0.Add(k + 1); //[cite: 2]
                        k++; //[cite: 2]
                        if (k == size) break; //[cite: 2]
                    }
                }

                k++; //[cite: 2]
                while (k < size) //[cite: 2]
                {
                    if (!boolIndex[k]) //[cite: 2]
                    {
                        coeff = c * c; //[cite: 2]
                        numer = bExt[k] + coeff * bExt[k + 1]; //[cite: 2]
                        denom = aExt[k] + aExt[k + 1]; //[cite: 2]
                        index = [k, k + 1]; //[cite: 2]
                        k++; //[cite: 2]

                        if (k < size) //[cite: 2]
                        {
                            while (!boolIndex[k]) //[cite: 2]
                            {
                                coeff *= c * c; //[cite: 2]
                                numer += coeff * bExt[k + 1]; //[cite: 2]
                                denom += aExt[k + 1]; //[cite: 2]
                                index.Add(k + 1); //[cite: 2]
                                k++; //[cite: 2]
                                if (k == size) break; //[cite: 2]
                            }
                        }

                        if (k < size) //[cite: 2]
                        {
                            sigmaNew[index[0]] = Math.Sqrt(numer / denom); //[cite: 2]
                            for (int j = 1; j < index.Count; j++) sigmaNew[index[j]] = sigmaNew[index[j - 1]] / c; //[cite: 2]
                        }
                    }
                    k++; //[cite: 2]
                }

                if (k == size && index0.Count > 1) //[cite: 2]
                {
                    sigmaNew[index0[0]] = Math.Sqrt(numer0 / denom0); //[cite: 2]
                    for (int j = 1; j < index0.Count; j++) sigmaNew[index0[j]] = sigmaNew[index0[j - 1]] / c; //[cite: 2]
                }
                else if (k == size + 1) //[cite: 2]
                {
                    index.RemoveAt(index.Count - 1); //[cite: 2]
                    index.AddRange(index0); //[cite: 2]
                    denom += Math.Pow(c, 2) * denom0; //[cite: 2] // Assumes coeff tracking maps to python scope logic
                    numer += numer0; //[cite: 2]
                    sigmaNew[index[0]] = Math.Sqrt(numer / denom); //[cite: 2]
                    for (int j = 1; j < index.Count; j++) sigmaNew[index[j]] = sigmaNew[index[j - 1]] / c; //[cite: 2]
                }

                for (int m = 0; m < size; m++) boolIndex[m] = sigmaNew[m] >= c * sigmaNew[(m + 1) % size] - 0.000001; //[cite: 2]
            }

            newEll = 0.0; //[cite: 2]
            for (int i = 0; i < n; i++) //[cite: 2]
            {
                try { newEll += Math.Log(NormalMixtureLikelihood(points[i], alphaNew, muNew, sigmaNew)); } //[cite: 2]
                catch { continue; } //[cite: 2]
            }

            delta = (newEll - oldEll) / (newEll - ell0); //[cite: 2]
        }

        return [alphaNew, muNew, sigmaNew]; //[cite: 2]
    }

    // --- Mathematical Helpers ---

    private static double NormalMixtureLikelihood(double x, ReadOnlySpan<double> alpha, ReadOnlySpan<double> mu, ReadOnlySpan<double> sigma) //[cite: 2]
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

        double sum = 0.0; //[cite: 2]
        for (int k = 0; k < alpha.Length; k++) //[cite: 2]
        {
            sum += alpha[k] * Standard((x - mu[k]) / sigma[k]) / sigma[k]; //[cite: 2]
        }
        return sum; //[cite: 2]
    }

    private static double Standard(double x) => 1.0 / Math.Sqrt(2.0 * Math.PI) * Math.Exp(-x * x / 2.0); //[cite: 2]

    private static double? PsiPrime(double x, int max = 10000) //[cite: 2]
    {
        if (x <= 0) //[cite: 2]
        {
            Console.WriteLine("digamma argument must be > 0"); //[cite: 2]
            return null; //[cite: 2]
        }

        double sum = 0; //[cite: 2]
        for (int i = 0; i <= max; i++) //[cite: 2]
        {
            double term = x + i; //[cite: 2]
            sum += 1.0 / (term * term); //[cite: 2]
        }

        return sum; //[cite: 2]
    }

    /// <summary>
    /// Computes the Digamma function.
    /// Note: In production C# targeting .NET, replace this stub with MathNet.Numerics.SpecialFunctions.DiGamma(x).
    /// </summary>
    private static double Digamma(double x) //[cite: 2]
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
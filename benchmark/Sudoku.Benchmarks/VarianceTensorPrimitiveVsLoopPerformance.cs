using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Numerics.Tensors;

namespace Sudoku.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class VarianceTensorPrimitiveVsLoopPerformance
{
    public double[] Points { get; set; }
    public double[] Weights { get; set; }

    [IterationSetup]
    public void GenerateSource()
    {
        Points = Enumerable.Range(1, 100_000_000).Select(n => Random.Shared.NextDouble()).ToArray();
        Weights = Enumerable.Range(1, 100_000_000).Select(n => Random.Shared.NextDouble()).ToArray();
    }

    [Benchmark(Baseline = true)]
    public double GetVarianceLoop()
    {
        var s3 = 0.0;
        const double mu = 0.5;

        for (var i = 0; i < Points.Length; i++)
        {
            var diff = Points[i] - mu;
            s3 += diff * diff * Weights[i];
        }

        return Math.Sqrt(s3 / Points.Length);
    }

    [Benchmark()]
    public double GetVarianceLinq()
    {
        const double mu = 0.5;
        var s3 = Points.Zip(Weights, (p, w) => (p - mu) * (p - mu) * w).Sum();
        return Math.Sqrt(s3 / Points.Length);
    }

    [Benchmark()]
    public double GetVarianceTensorPrimitive()
    {
        const double mu = 0.5;
        Span<double> centered = Points.Length < 256
            ? stackalloc double[Points.Length]
            : new double[Points.Length];

        TensorPrimitives.Subtract(Points, mu, centered);
        TensorPrimitives.Multiply(centered, centered, centered);
        var s3 = TensorPrimitives.Dot(centered, Weights);

        return Math.Sqrt(s3 / Points.Length);
    }

}

/*
   // * Summary *
   
   BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8737/25H2/2025Update/HudsonValley2)
   Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
   .NET SDK 11.0.100-preview.5.26302.115
     [Host]    : .NET 10.0.10 (10.0.10, 10.0.1026.32716), X64 RyuJIT x86-64-v3
     .NET 10.0 : .NET 10.0.10 (10.0.10, 10.0.1026.32716), X64 RyuJIT x86-64-v3
   
   Job=.NET 10.0  Runtime=.NET 10.0  InvocationCount=1
   UnrollFactor=1
   
   | Method                     | Mean     | Error    | StdDev   | Median   | Ratio | RatioSD | Allocated   | Alloc Ratio |
   |--------------------------- |---------:|---------:|---------:|---------:|------:|--------:|------------:|------------:|
   | GetVarianceLoop            | 135.8 ms |  2.65 ms |  5.47 ms | 133.4 ms |  1.00 |    0.06 |           - |          NA |
   | GetVarianceLinq            | 877.2 ms | 17.21 ms | 21.13 ms | 869.9 ms |  6.47 |    0.29 |       160 B |          NA |
   | GetVarianceTensorPrimitive | 237.4 ms |  4.53 ms |  8.16 ms | 235.7 ms |  1.75 |    0.09 | 800000024 B |          NA |
   
   // * Hints *
   Outliers
     VarianceTensorPrimitiveVsLoopPerformance.GetVarianceLoop: .NET 10.0            -> 3 outliers were removed (156.40 ms..162.72 ms)
     VarianceTensorPrimitiveVsLoopPerformance.GetVarianceTensorPrimitive: .NET 10.0 -> 4 outliers were removed (259.15 ms..273.02 ms)
   
 */
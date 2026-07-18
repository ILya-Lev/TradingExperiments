using System.Numerics.Tensors;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Sudoku.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class WeightedMeanTensorPrimitiveVsLoopPerformance
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
    public double GetWeightedMeanLoop()
    {
        var totalWeight = 0.0;
        var weightedPoints = 0.0;

        for (var i = 0; i < Points.Length; i++)
        {
            totalWeight += Weights[i];
            weightedPoints += Points[i] * Weights[i];
        }

        return weightedPoints / totalWeight;
    }

    [Benchmark()]
    public double GetWeightedMeanLinq()
    {
        var weightedPoints = Points.Zip(Weights, (p, w) => p * w).Sum();
        var totalWeight = Weights.Sum();
        return weightedPoints/totalWeight;
    }

    [Benchmark()]
    public double GetWeightedMeanTensorPrimitive()
    {
        var totalWeight = TensorPrimitives.Sum(Weights);
        var weightedPoints = TensorPrimitives.Dot(Points, Weights);
        return weightedPoints / totalWeight;
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
   
   | Method                         | Mean     | Error    | StdDev   | Median   | Ratio | RatioSD | Allocated | Alloc Ratio |
   |------------------------------- |---------:|---------:|---------:|---------:|------:|--------:|----------:|------------:|
   | GetWeightedMeanLoop            | 145.1 ms |  3.60 ms | 10.40 ms | 144.7 ms |  1.00 |    0.10 |         - |          NA |
   | GetWeightedMeanLinq            | 834.0 ms | 13.06 ms | 20.33 ms | 828.7 ms |  5.78 |    0.42 |     160 B |          NA |
   | GetWeightedMeanTensorPrimitive | 113.8 ms |  3.54 ms | 10.32 ms | 116.8 ms |  0.79 |    0.09 |         - |          NA |
   
 */
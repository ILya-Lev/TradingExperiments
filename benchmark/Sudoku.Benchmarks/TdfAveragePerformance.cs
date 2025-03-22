using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using TDF.Lib;

namespace Sudoku.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class TdfAveragePerformance
{
    public double[] Source { get; set; }

    [IterationSetup]
    public void GenerateSource()
    {
        Source = Enumerable.Range(1, 1_000_000_000).Select(n => Random.Shared.NextDouble()).ToArray();
    }

    [Benchmark] public double Sequential() => Source.Average();
    [Benchmark] public double Parallel() => AsyncStreamDemo.GetAverage(Source);
    [Benchmark] public double Plinq() => AsyncStreamDemo.GetAveragePlinq(Source);
}

/*
 // * Summary * 1B
   
   BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3476)
   Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
   .NET SDK 9.0.201
     [Host]   : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2
     .NET 9.0 : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2
   
   Job=.NET 9.0  Runtime=.NET 9.0  InvocationCount=1
   UnrollFactor=1
   
   | Method     | Mean       | Error    | StdDev   | Allocated |
   |----------- |-----------:|---------:|---------:|----------:|
   | Sequential | 2,493.6 ms | 34.85 ms | 29.10 ms |     400 B |
   | Parallel   |   671.5 ms | 13.16 ms | 18.87 ms |    6896 B |
   | Plinq      |   715.3 ms | 13.93 ms | 20.42 ms |    8192 B |
   
   // * Hints *
   Outliers
     TdfAveragePerformance.Sequential: .NET 9.0 -> 2 outliers were removed (2.76 s, 2.85 s)
     TdfAveragePerformance.Parallel: .NET 9.0   -> 4 outliers were removed (846.73 ms..856.34 ms)
   
   // ***** BenchmarkRunner: End *****
   Run time: 00:13:11 (791.5 sec), executed benchmarks: 3

 */
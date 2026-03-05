using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using Udemy.Fin.Stat;

namespace Sudoku.Benchmarks;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.CacheMisses)]
public class CoinTossingStateGeneratorPerformance
{
    [Params(20, 24)]
    public int Size { get; set; }

    [Benchmark] public void GenerateCoinTossRecursively() => CoinTossingStateGenerator.GenerateFairCoinTossesRecursive(Size);
    [Benchmark] public void GenerateCoinTossPreallocated() => CoinTossingStateGenerator.GenerateFairCoinTosses(Size);
}

/*
 
   // * Summary *
   
   BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.7922/25H2/2025Update/HudsonValley2)
   Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
   .NET SDK 10.0.103
     [Host]    : .NET 10.0.3 (10.0.3, 10.0.326.7603), X64 RyuJIT x86-64-v3
     .NET 10.0 : .NET 10.0.3 (10.0.3, 10.0.326.7603), X64 RyuJIT x86-64-v3
   
   Job=.NET 10.0  Runtime=.NET 10.0
   
   | Method                       | Size | Mean       | Error     | StdDev    | CacheMisses/Op | BranchMispredictions/Op | Gen0        | Completed Work Items | Lock Contentions | Gen1        | Gen2       | Allocated  |
   |----------------------------- |----- |-----------:|----------:|----------:|---------------:|------------------------:|------------:|---------------------:|-----------------:|------------:|-----------:|-----------:|
   | GenerateCoinTossPreallocated | 20   |   353.3 ms |   6.88 ms |  11.10 ms |     41,499,810 |               1,203,685 |  25000.0000 |                    - |                - |  15000.0000 |  5000.0000 |  133.87 MB |
   | GenerateCoinTossRecursively  | 20   |   427.4 ms |   7.91 ms |  21.39 ms |     44,423,421 |               1,635,869 |  25000.0000 |                    - |                - |  15000.0000 |  5000.0000 |  149.87 MB |
   | GenerateCoinTossPreallocated | 24   | 6,446.4 ms | 124.38 ms | 122.15 ms |    905,710,080 |              17,022,464 | 391000.0000 |                    - |                - | 202000.0000 | 13000.0000 | 2397.87 MB |
   | GenerateCoinTossRecursively  | 24   | 8,218.0 ms | 160.66 ms | 254.83 ms |    960,272,384 |              34,171,844 | 391000.0000 |                    - |                - | 202000.0000 | 13000.0000 | 2653.87 MB |
   
 */
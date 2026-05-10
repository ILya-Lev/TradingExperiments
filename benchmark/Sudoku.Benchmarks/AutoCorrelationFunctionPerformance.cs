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
public class AutoCorrelationFunctionPerformance
{
    [Params(1000, 10_000)]
    public int Size { get; set; }

    public double[] WhiteNoise { get; set; }

    [GlobalSetup]
    public void GenerateWhiteNoise()
    {
        WhiteNoise = AlgLibWhiteNoise.GenerateWhiteNoise(1.0).Take(Size).ToArray();
    }

    [Benchmark(Baseline = true)] public double[] GenerateInPlainLoop() => WhiteNoise.GetAutoCorrelation(Size/10);
    [Benchmark] public double[] GenerateWithSimd() => WhiteNoise.GetAutoCorrelationSimd(Size/10);
    [Benchmark] public double[] GenerateWithParallelAndSimd() => WhiteNoise.GetAutoCorrelationParallel(Size/10);
}

/*
   // * Summary *
   
   BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8328/25H2/2025Update/HudsonValley2)
   Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
   .NET SDK 10.0.203
     [Host]    : .NET 10.0.7 (10.0.7, 10.0.726.21808), X64 RyuJIT x86-64-v3
     .NET 10.0 : .NET 10.0.7 (10.0.7, 10.0.726.21808), X64 RyuJIT x86-64-v3
   
   Job=.NET 10.0  Runtime=.NET 10.0
   
   | Method                      | Size  | Mean         | Error      | StdDev     | Median       | Ratio | RatioSD | BranchMispredictions/Op | CacheMisses/Op | Gen0   | Completed Work Items | Lock Contentions | Allocated | Alloc Ratio |
   |---------------------------- |------ |-------------:|-----------:|-----------:|-------------:|------:|--------:|------------------------:|---------------:|-------:|---------------------:|-----------------:|----------:|------------:|
   | GenerateWithSimd            | 1000  |     27.51 us |   0.539 us |   0.886 us |     27.36 us |  0.16 |    0.01 |                      98 |             93 | 0.1526 |                    - |                - |     832 B |        1.00 |
  >| GenerateWithParallelAndSimd | 1000  |     28.69 us |   0.567 us |   1.184 us |     29.03 us |  0.17 |    0.01 |                   1,289 |            530 | 0.7935 |               4.5124 |           0.0001 |    3680 B |        4.42 |
   | GenerateInPlainLoop         | 1000  |    171.18 us |   4.594 us |  12.957 us |    169.57 us |  1.01 |    0.11 |                     226 |            444 |      - |                    - |                - |     832 B |        1.00 |
   |                             |       |              |            |            |              |       |         |                         |                |        |                      |                  |           |             |
  >| GenerateWithParallelAndSimd | 10000 |    437.38 us |   7.503 us |   6.651 us |    437.77 us |  0.03 |    0.00 |                  15,050 |          3,725 | 1.9531 |               9.9648 |                - |   11683 B |        1.45 |
   | GenerateWithSimd            | 10000 |  2,977.82 us |  65.701 us | 191.652 us |  2,919.21 us |  0.24 |    0.02 |                   3,452 |         10,308 |      - |                    - |                - |    8032 B |        1.00 |
   | GenerateInPlainLoop         | 10000 | 12,561.44 us | 248.618 us | 697.150 us | 12,445.90 us |  1.00 |    0.08 |                   7,577 |         32,453 |      - |                    - |                - |    8032 B |        1.00 |
   
   // * Warnings *
   MultimodalDistribution
     AutoCorrelationFunctionPerformance.GenerateInPlainLoop: .NET 10.0 -> It seems that the distribution is bimodal (mValue = 3.65)

   // ***** BenchmarkRunner: End *****
   Run time: 00:12:50 (770.44 sec), executed benchmarks: 6
*/
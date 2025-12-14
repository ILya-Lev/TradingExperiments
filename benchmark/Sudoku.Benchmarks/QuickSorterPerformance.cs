using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;

namespace Sudoku.Benchmarks;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.CacheMisses)]
public class QuickSorterPerformance
{
    private int[] _masterSource = [];
    public int[] Source { get; set; }

    [Params(1_000_000, 10_000_000)]
    public int Size { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _masterSource = Enumerable.Range(1, Size).Reverse().ToArray();
        Source = new int[Size];
    }

    [IterationSetup]
    public void GenerateSource()
    {
        Array.Copy(_masterSource, Source, Size);
    }

    [Benchmark] public void SortRecursiveSingleThread() => Source.QuickSort(false, true);
    [Benchmark] public void SortRecursiveMultiThread() => Source.QuickSort(true, true);
    [Benchmark] public void SortRecursiveMultiThreadNew() => QuickSorter001.SortParallel(Source);
    [Benchmark(Baseline = true)] public void SortStackSingleThread() => Source.QuickSort(false, false);
    [Benchmark] public void SortStackSingleThreadNew() => QuickSorter001.Sort(Source);
    [Benchmark] public void SortStackMultiThread() => Source.QuickSort(true, false);
}
/*
  // * Summary *
   
   BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.5011/22H2/2022Update)
   12th Gen Intel Core i7-12800H, 1 CPU, 20 logical and 14 physical cores
   .NET SDK 9.0.100-preview.4.24267.66
     [Host]   : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
     .NET 8.0 : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
   
   Job=.NET 8.0  Runtime=.NET 8.0  InvocationCount=1
   UnrollFactor=1
   
   | Method                    | Mean     | Error    | StdDev   | Gen0       | Gen1      | Allocated    |
   |-------------------------- |---------:|---------:|---------:|-----------:|----------:|-------------:|
   | SortRecursiveSingleThread | 538.3 ms | 14.82 ms | 42.99 ms |  9000.0000 |         - | 117246.33 KB |
   | SortRecursiveMultiThread  | 144.7 ms |  3.40 ms |  9.88 ms | 13000.0000 | 1000.0000 | 170211.27 KB |
   | SortStackSingleThread     | 460.2 ms | 17.13 ms | 50.23 ms |          - |         - |      1.51 KB |
   | SortStackMultiThread      | 824.1 ms | 15.96 ms | 17.08 ms | 27000.0000 |         - | 328932.88 KB |
   
   // * Hints *
   Outliers
     QuickSorterPerformance.SortRecursiveSingleThread: .NET 8.0 -> 3 outliers were removed (652.25 ms..714.20 ms)
     QuickSorterPerformance.SortRecursiveMultiThread: .NET 8.0  -> 3 outliers were removed (181.25 ms..195.27 ms)
     QuickSorterPerformance.SortStackSingleThread: .NET 8.0     -> 1 outlier  was  removed (597.25 ms)
   
// * Summary *
   
   BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4317/23H2/2023Update/SunValley3)
   Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
   .NET SDK 9.0.100-preview.4.24267.66
     [Host]   : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2
     .NET 8.0 : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2
   
   Job=.NET 8.0  Runtime=.NET 8.0  InvocationCount=1
   UnrollFactor=1
   
   | Method                    | Mean     | Error    | StdDev   | Gen0       | Gen1      | Allocated    |
   |-------------------------- |---------:|---------:|---------:|-----------:|----------:|-------------:|
   | SortRecursiveSingleThread | 660.5 ms | 12.82 ms | 14.76 ms | 25000.0000 |         - | 117234.34 KB |
   | SortRecursiveMultiThread  | 208.4 ms |  6.95 ms | 20.49 ms | 25000.0000 | 1000.0000 | 117959.97 KB |
   | SortStackSingleThread     | 536.8 ms | 10.35 ms | 14.85 ms |          - |         - |      1.51 KB |
   | SortStackMultiThread      | 911.6 ms | 20.37 ms | 59.09 ms | 89000.0000 |         - | 405548.34 KB |
   
   // * Warnings *
   MultimodalDistribution
     QuickSorterPerformance.SortRecursiveMultiThread: .NET 8.0 -> It seems that the distribution is bimodal (mValue = 3.56)
   
   // * Hints *
   Outliers
     QuickSorterPerformance.SortStackSingleThread: .NET 8.0 -> 1 outlier  was  removed (583.80 ms)
     QuickSorterPerformance.SortStackMultiThread: .NET 8.0  -> 3 outliers were removed (1.13 s..1.17 s)


   // * Summary *
   
   BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.7462/25H2/2025Update/HudsonValley2)
   Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
   .NET SDK 10.0.101
     [Host]    : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3
     .NET 10.0 : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3
   
   Job=.NET 10.0  Runtime=.NET 10.0  InvocationCount=1
   UnrollFactor=1
   
   | Method                      | Size| Mean        | Error      | StdDev     | Ratio | RatioSD | CacheMisses/Op | BranchMispredictions/Op | Completed Work Items | Lock Contentions | Gen0        | Gen1      | Allocated     | Alloc Ratio  |
   |---------------------------- |-----|------------:|-----------:|-----------:|------:|--------:|---------------:|------------------------:|---------------------:|-----------------:|------------:|----------:|--------------:|-------------:|
   | SortRecursiveMultiThreadNew | 1M  |    21.10 ms |   1.777 ms |   5.210 ms |  0.14 |    0.03 |      1,452,769 |               2,141,880 |            1834.0000 |           3.0000 |   4000.0000 |         - |   22141.31 KB |    19,818.80 |
   | SortStackSingleThreadNew    | 1M  |    87.85 ms |   6.069 ms |  17.893 ms |  0.58 |    0.12 |      5,781,586 |               2,027,725 |                    - |                - |           - |         - |  315337.09 KB |   282,259.78 |
   | SortRecursiveMultiThread    | 1M  |   103.01 ms |   2.735 ms |   7.757 ms |  0.68 |    0.05 |      5,442,314 |               5,801,779 |            1874.0000 |          57.0000 |  25000.0000 | 1000.0000 |  117993.08 KB |   105,616.18 |
   | SortStackSingleThread       | 1M  |   150.97 ms |   2.920 ms |   4.631 ms |  1.00 |    0.04 |        853,636 |               3,770,075 |                    - |                - |           - |         - |       1.12 KB |         1.00 |
   | SortRecursiveSingleThread   | 1M  |   269.45 ms |   5.254 ms |   9.339 ms |  1.79 |    0.08 |      4,684,525 |               4,178,010 |                    - |                - |  25000.0000 |         - |  117351.69 KB |   105,042.07 |
   | SortStackMultiThread        | 1M  |   576.97 ms |  11.393 ms |  26.405 ms |  3.83 |    0.21 |     12,343,754 |              12,576,286 |          373290.0000 |          26.0000 |  84000.0000 |         - |  386715.27 KB |   346,150.73 |
   |                             |     |             |            |            |       |         |                |                         |                      |                  |             |           |               |              |
   | SortRecursiveMultiThreadNew | 10M |   255.62 ms |  20.789 ms |  60.970 ms |  0.16 |    0.04 |     19,769,590 |              20,827,996 |            1937.0000 |                - |  52000.0000 | 1000.0000 |  279097.93 KB |   249,821.92 |
   | SortStackSingleThreadNew    | 10M |   873.03 ms |  58.882 ms | 173.616 ms |  0.53 |    0.11 |     70,695,526 |              17,589,821 |                    - |                - |           - |         - | 3672421.44 KB | 3,287,202.41 |
   | SortRecursiveMultiThread    | 10M |   957.40 ms |  22.061 ms |  64.354 ms |  0.58 |    0.04 |     49,390,510 |              47,013,888 |            2020.0000 |           2.0000 | 256000.0000 | 2000.0000 | 1172710.63 KB | 1,049,699.03 |
   | SortStackSingleThread       | 10M | 1,647.98 ms |  23.719 ms |  18.518 ms |  1.00 |    0.02 |     11,029,777 |              34,908,638 |                    - |                - |           - |         - |       1.12 KB |         1.00 |
   | SortRecursiveSingleThread   | 10M | 2,866.11 ms |  51.070 ms |  45.273 ms |  1.74 |    0.03 |     46,908,894 |              39,439,019 |                    - |                - | 255000.0000 |         - |    1171862 KB | 1,048,939.41 |
   | SortStackMultiThread        | 10M | 6,075.88 ms | 113.082 ms | 150.962 ms |  3.69 |    0.10 |    126,812,615 |             126,899,693 |         3746617.0000 |         248.0000 | 846000.0000 | 8000.0000 | 3867663.34 KB | 3,461,964.39 |
   
 */
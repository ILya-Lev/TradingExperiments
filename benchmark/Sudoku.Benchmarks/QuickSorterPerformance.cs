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
   
   | Method                      | Size| Mean        | Error      | StdDev     | Ratio | RatioSD | Completed Work Items | Lock Contentions | Gen0        | CacheMisses/Op | BranchMispredictions/Op | Gen1      | Allocated     | Alloc Ratio  |
   |---------------------------- |-----|------------:|-----------:|-----------:|------:|--------:|---------------------:|-----------------:|------------:|---------------:|------------------------:|----------:|--------------:|-------------:|
   | SortStackSingleThreadNew    | 1M  |    58.72 ms |   6.024 ms |  17.573 ms |  0.39 |    0.12 |                    - |                - |           - |        733,184 |               1,857,249 |         - |    3906.87 KB |     3,497.06 |
   | SortRecursiveMultiThreadNew | 1M  |    81.07 ms |   8.010 ms |  23.364 ms |  0.54 |    0.15 |                    - |                - |   6000.0000 |      1,536,328 |               2,139,218 |         - |   32906.77 KB |    29,455.01 |
   | SortRecursiveMultiThread    | 1M  |    92.88 ms |   2.848 ms |   8.171 ms |  0.61 |    0.06 |            1503.0000 |          69.0000 |  25000.0000 |      4,548,854 |               5,252,383 |         - |   117904.5 KB |   105,536.90 |
   | SortStackSingleThread       | 1M  |   151.30 ms |   2.911 ms |   2.989 ms |  1.00 |    0.03 |                    - |                - |           - |        858,546 |               3,657,246 |         - |       1.12 KB |         1.00 |
   | SortRecursiveSingleThread   | 1M  |   256.16 ms |   5.000 ms |   6.141 ms |  1.69 |    0.05 |                    - |                - |  25000.0000 |      4,549,231 |               4,017,820 |         - |  117122.31 KB |   104,836.76 |
   | SortStackMultiThread        | 1M  |   553.57 ms |  10.659 ms |  15.954 ms |  3.66 |    0.13 |          377130.0000 |          15.0000 |  84000.0000 |     11,922,926 |              12,268,363 |         - |  387004.52 KB |   346,409.64 |
   |                             |     |             |            |            |       |         |                      |                  |             |                |                         |           |               |              |
   | SortStackSingleThreadNew    | 10M |   782.88 ms |  72.216 ms | 212.929 ms |  0.47 |    0.13 |                    - |                - |           - |     12,059,034 |              18,650,685 |         - |   39063.64 KB |    34,966.06 |
   | SortRecursiveMultiThreadNew | 10M |   836.07 ms |  74.022 ms | 218.255 ms |  0.50 |    0.13 |                    - |                - |  56000.0000 |     20,876,370 |              21,458,903 |         - |  300428.96 KB |   268,915.43 |
   | SortRecursiveMultiThread    | 10M |   936.48 ms |  22.830 ms |  66.234 ms |  0.56 |    0.04 |            1985.0000 |                - | 256000.0000 |     43,665,285 |              45,342,474 | 1000.0000 | 1172992.72 KB | 1,049,951.52 |
   | SortStackSingleThread       | 10M | 1,668.84 ms |  31.097 ms |  51.093 ms |  1.00 |    0.04 |                    - |                - |           - |     11,912,149 |              35,985,742 |         - |       1.12 KB |         1.00 |
   | SortRecursiveSingleThread   | 10M | 2,759.24 ms |  21.333 ms |  18.911 ms |  1.65 |    0.05 |                    - |                - | 255000.0000 |     45,072,930 |              40,691,644 |         - | 1172032.86 KB | 1,049,092.35 |
   | SortStackMultiThread        | 10M | 5,930.85 ms | 118.465 ms | 105.016 ms |  3.56 |    0.12 |         3762108.0000 |         221.0000 | 847000.0000 |    121,200,162 |             123,581,781 | 8000.0000 | 3870535.97 KB | 3,464,535.69 |
   
 */
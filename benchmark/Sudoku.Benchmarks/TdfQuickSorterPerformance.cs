using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using TDF.Lib;

namespace Sudoku.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class TdfQuickSorterPerformance
{
    private static readonly Func<int, int, bool> _isBefore = (a, b) => a < b;
    private static readonly QuickSorter<int> _sequential = new(_isBefore);
    private static readonly QuickSorterAsync<int> _async = new(_isBefore);
    private static readonly QuickSorterParallel<int> _parallel = new(_isBefore);
    private static readonly QuickSorterParallelStack<int> _parallelStack = new(_isBefore);

    public int[] Source { get; set; }

    [IterationSetup]
    public void GenerateSource()
    {
        Source = Enumerable.Range(1, 10_000_000).Reverse().ToArray();
    }

    [Benchmark] public async Task SortSingleThread() => await _sequential.Sort(Source.ToArray());
    [Benchmark] public async Task SortAsync() => await _async.Sort(Source.ToArray());
    [Benchmark] public async Task SortParallel() => await _parallel.Sort(Source.ToArray());
    [Benchmark] public async Task SortParallelStack() => await _parallelStack.Sort(Source.ToArray());
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


   // * Summary * 1M elements
   
   BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3476)
   Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
   .NET SDK 9.0.201
     [Host]   : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2
     .NET 9.0 : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2
   
   Job=.NET 9.0  Runtime=.NET 9.0  InvocationCount=1
   UnrollFactor=1
   
   | Method            | Mean      | Error    | StdDev    | Gen0       | Gen1      | Allocated |
   |------------------ |----------:|---------:|----------:|-----------:|----------:|----------:|
   | SortSingleThread  |  91.99 ms | 4.649 ms | 13.636 ms |          - |         - |   3.82 MB |
   | SortAsync         | 164.34 ms | 7.496 ms | 21.748 ms | 21000.0000 | 1000.0000 |  95.43 MB |//async await
   | SortParallel      |  34.70 ms | 2.329 ms |  6.719 ms |  6000.0000 |         - |   34.4 MB |
   | SortParallelStack |  96.16 ms | 4.432 ms | 12.716 ms | 11000.0000 |         - |  54.68 MB |
   

// * Summary * 10M elements
   
   BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3476)
   Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
   .NET SDK 9.0.201
     [Host]   : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2
     .NET 9.0 : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2
   
   Job=.NET 9.0  Runtime=.NET 9.0  InvocationCount=1
   UnrollFactor=1
   
   | Method            | Mean     | Error    | StdDev   | Gen0        | Gen1      | Allocated |
   |------------------ |---------:|---------:|---------:|------------:|----------:|----------:|
   | SortSingleThread  | 848.6 ms | 26.22 ms | 72.22 ms |           - |         - |  38.15 MB |
   | SortAsync         | 266.0 ms | 13.50 ms | 39.37 ms |  68000.0000 | 2000.0000 | 344.08 MB |//AttachedToParent
   | SortParallel      | 245.2 ms | 10.29 ms | 29.35 ms |  68000.0000 |         - | 343.41 MB |
   | SortParallelStack | 885.0 ms | 17.61 ms | 22.90 ms | 113000.0000 |         - | 546.78 MB |
   
 */
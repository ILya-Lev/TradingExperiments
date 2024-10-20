using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Sudoku.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class QuickSorterPerformance
{
    public int[] Source {get; set; }

    [IterationSetup]
    public void GenerateSource()
    {
        Source = Enumerable.Range(1, 1_000_000).Reverse().ToArray();
    }

    [Benchmark] public void SortRecursiveSingleThread() => Source.QuickSort(false, true);
    [Benchmark] public void SortRecursiveMultiThread() => Source.QuickSort(true, true);
    [Benchmark] public void SortStackSingleThread() => Source.QuickSort(false, false);
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
   
 */
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
   | SortRecursiveSingleThread | 476.9 ms | 15.88 ms | 46.83 ms |  9000.0000 |         - | 117280.55 KB |
   | SortRecursiveMultiThread  | 143.1 ms |  2.84 ms |  8.20 ms | 13000.0000 | 1000.0000 | 165234.57 KB |
   | SortStackSingleThread     | 419.6 ms | 20.49 ms | 60.43 ms |          - |         - |      1.51 KB |
   | SortStackMultiThread      | 923.8 ms |  9.46 ms |  8.38 ms | 32000.0000 |         - | 392136.45 KB |
   
   // * Warnings *
   MultimodalDistribution
     QuickSorterPerformance.SortStackSingleThread: .NET 8.0 -> It seems that the distribution is bimodal (mValue = 3.53)
   
   // * Hints *
   Outliers
     QuickSorterPerformance.SortRecursiveMultiThread: .NET 8.0 -> 3 outliers were removed (167.44 ms..172.59 ms)
     QuickSorterPerformance.SortStackMultiThread: .NET 8.0     -> 1 outlier  was  removed (949.37 ms)
   
 */
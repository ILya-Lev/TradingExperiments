using BenchmarkDotNet.Running;

Console.WriteLine("Hello, World!");
//BenchmarkRunner.Run<PalindromeFinderPerformance>();
//BenchmarkRunner.Run<QuickSorterPerformance>();
BenchmarkRunner.Run<SearchStringPerformance>();
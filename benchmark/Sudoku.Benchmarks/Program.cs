using BenchmarkDotNet.Running;
using Sudoku.Benchmarks;

Console.WriteLine("Hello, World!");
//BenchmarkRunner.Run<PalindromeFinderPerformance>();
BenchmarkRunner.Run<QuickSorterPerformance>();
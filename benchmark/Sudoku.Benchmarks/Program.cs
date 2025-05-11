using BenchmarkDotNet.Running;
using Sudoku.Benchmarks;

Console.WriteLine("Performance test suit");
//BenchmarkRunner.Run<PalindromeFinderPerformance>();
//BenchmarkRunner.Run<QuickSorterPerformance>();
//BenchmarkRunner.Run<SearchStringPerformance>();
//BenchmarkRunner.Run<TdfQuickSorterPerformance>();
//BenchmarkRunner.Run<TdfAveragePerformance>();
BenchmarkRunner.Run<PolynomialCalculatorPerformance>();
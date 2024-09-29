// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Sudoku;

Console.WriteLine("Hello, World!");
BenchmarkRunner.Run<PalindromeFinderPerformance>();

[MemoryDiagnoser]
public class PalindromeFinderPerformance
{
    [Benchmark]
    public int FindPalindrome() => PalindromeFinder.FindLargestPalindrome();
}
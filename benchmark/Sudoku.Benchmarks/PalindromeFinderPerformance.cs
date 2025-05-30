﻿using BenchmarkDotNet.Attributes;

namespace Sudoku.Benchmarks;

[MemoryDiagnoser]
public class PalindromeFinderPerformance
{
    [ParamsSource(nameof(GetRange))]
    public int N { get; set; }

    public IEnumerable<int> GetRange => new[] { 888888, 906609, 999999, 100001 };
    //=> Enumerable.Range(800_000, 200_000-1);

    [Benchmark] public bool IsPalindromeInPlace() => PalindromeFinder.IsPalindromeInPlace(N);
    [Benchmark(Baseline = true)] public bool IsPalindromeList() => PalindromeFinder.IsPalindromeList(N);
    [Benchmark] public bool IsPalindromeStackAlloc() => PalindromeFinder.IsPalindromeStackAlloc(N);
}
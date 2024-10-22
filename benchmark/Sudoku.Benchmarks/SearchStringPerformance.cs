using System.Buffers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class SearchStringPerformance
{
    [Params("1abcd", "abcd1", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa0")]
    public string Input { get; set; }

    private static readonly char[] _validChars
        = "abcdefghjkmnpqrstuvwxABCDEFGHJKMNPQRSTUVWX23456789".ToCharArray();
    private static readonly SearchValues<char> _searchValues
        = SearchValues.Create(_validChars.AsSpan());
    private static readonly HashSet<char> _hashSet = [.._validChars];

    [Benchmark]
    public bool FindAll() => Input.All(c => _validChars.Contains(c));

    [Benchmark]
    public bool FindSearchValue() => !Input.AsSpan().ContainsAnyExcept(_searchValues);

    [Benchmark]
    public bool FindHashSet() => Input.All(c => _hashSet.Contains(c));
}

/*
 
---------------------------------------------------
   
   // * Summary *
   
   System.Runtime.Intrinsics.Vector128.IsHardwareAccelerated -> true

   BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.5011/22H2/2022Update)
   12th Gen Intel Core i7-12800H, 1 CPU, 20 logical and 14 physical cores
   .NET SDK 9.0.100-preview.4.24267.66
     [Host]   : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
     .NET 8.0 : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
   
   Job=.NET 8.0  Runtime=.NET 8.0
   
   | Method          | Input                | Mean       | Error      | StdDev     | Median     | Gen0   | Allocated |
   |---------------- |--------------------- |-----------:|-----------:|-----------:|-----------:|-------:|----------:|
   | FindAll         | 1abcd                |  13.599 ns |  0.6216 ns |  1.8231 ns |  14.065 ns | 0.0025 |      32 B |
   | FindSearchValue | 1abcd                |   3.059 ns |  0.1060 ns |  0.3126 ns |   3.015 ns |      - |         - |
   | FindHashSet     | 1abcd                |   9.256 ns |  0.6841 ns |  2.0170 ns |   9.502 ns | 0.0025 |      32 B |
   | FindAll         | aaaaa(...)aaaa0 [49] | 362.867 ns | 18.2492 ns | 53.8082 ns | 377.884 ns | 0.0024 |      32 B |
   | FindSearchValue | aaaaa(...)aaaa0 [49] |   4.411 ns |  0.4339 ns |  1.2793 ns |   4.602 ns |      - |         - |
   | FindHashSet     | aaaaa(...)aaaa0 [49] | 236.096 ns | 12.7209 ns | 37.5080 ns | 244.378 ns | 0.0024 |      32 B |
   | FindAll         | abcd1                |  42.266 ns |  2.0744 ns |  6.1164 ns |  43.923 ns | 0.0025 |      32 B |
   | FindSearchValue | abcd1                |   5.679 ns |  0.1604 ns |  0.4728 ns |   5.698 ns |      - |         - |
   | FindHashSet     | abcd1                |  28.012 ns |  1.4860 ns |  4.3816 ns |  29.056 ns | 0.0025 |      32 B |
   
   // * Warnings *
   MultimodalDistribution
     SearchStringPerformance.FindSearchValue: .NET 8.0 ->distribution is bimodal (mValue = 4)
     SearchStringPerformance.FindHashSet: .NET 8.0     ->distribution is multimodal (mValue = 4.63)
     SearchStringPerformance.FindSearchValue: .NET 8.0 ->distribution can have several modes (mValue = 2.89)
     SearchStringPerformance.FindSearchValue: .NET 8.0 ->distribution is bimodal (mValue = 4.14)
   
   // * Hints *
   Outliers
     SearchStringPerformance.FindAll: .NET 8.0 -> 1 outlier  was  removed, 5 detected (9.74 ns..10.84 ns, 20.37 ns)
     SearchStringPerformance.FindAll: .NET 8.0 -> 5 outliers were detected (27.12 ns..28.09 ns)
   
// * Summary *
   
   BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4317/23H2/2023Update/SunValley3)
   Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
   .NET SDK 9.0.100-preview.4.24267.66
     [Host]   : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2
     .NET 8.0 : .NET 8.0.10 (8.0.1024.46610), X64 RyuJIT AVX2
   
   Job=.NET 8.0  Runtime=.NET 8.0
   
   | Method          | Input                | Mean       | Error     | StdDev     | Median     | Gen0   | Allocated |
   |---------------- |--------------------- |-----------:|----------:|-----------:|-----------:|-------:|----------:|
   | FindAll         | 1abcd                |  17.782 ns | 0.3804 ns |  0.5693 ns |  17.535 ns | 0.0068 |      32 B |
   | FindSearchValue | 1abcd                |   3.332 ns | 0.0983 ns |  0.1410 ns |   3.273 ns |      - |         - |
   | FindHashSet     | 1abcd                |  13.432 ns | 0.2932 ns |  0.3707 ns |  13.428 ns | 0.0068 |      32 B |
   | FindAll         | aaaaa(...)aaaa0 [49] | 416.405 ns | 8.3258 ns | 20.5793 ns | 414.581 ns | 0.0067 |      32 B |
   | FindSearchValue | aaaaa(...)aaaa0 [49] |   6.098 ns | 0.1544 ns |  0.3085 ns |   6.029 ns |      - |         - |
   | FindHashSet     | aaaaa(...)aaaa0 [49] | 313.272 ns | 6.0918 ns |  6.5182 ns | 311.137 ns | 0.0067 |      32 B |
   | FindAll         | abcd1                |  48.888 ns | 0.9918 ns |  1.5145 ns |  48.549 ns | 0.0068 |      32 B |
   | FindSearchValue | abcd1                |   7.300 ns | 0.1729 ns |  0.4703 ns |   7.163 ns |      - |         - |
   | FindHashSet     | abcd1                |  40.734 ns | 0.8388 ns |  1.8413 ns |  40.470 ns | 0.0068 |      32 B |
   
   // * Warnings *
   MultimodalDistribution
     SearchStringPerformance.FindAll: .NET 8.0 -> distribution is bimodal (mValue = 3.48)
   
   // * Hints *
   Outliers
     SearchStringPerformance.FindAll: .NET 8.0         -> 1 outlier  was  removed (21.14 ns)
     SearchStringPerformance.FindSearchValue: .NET 8.0 -> 3 outliers were removed (5.39 ns..5.51 ns)
     SearchStringPerformance.FindHashSet: .NET 8.0     -> 1 outlier  was  removed (16.39 ns)
     SearchStringPerformance.FindAll: .NET 8.0         -> 3 outliers were removed (490.53 ns..562.08 ns)
     SearchStringPerformance.FindSearchValue: .NET 8.0 -> 7 outliers were removed (10.33 ns..12.51 ns)
     SearchStringPerformance.FindHashSet: .NET 8.0     -> 3 outliers were removed (49.76 ns..55.28 ns)
 */
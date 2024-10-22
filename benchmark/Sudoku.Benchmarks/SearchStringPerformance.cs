using System.Buffers;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public partial class SearchStringPerformance
{
    private const string ValidChars = "abcdefghjkmnpqrstuvwxABCDEFGHJKMNPQRSTUVWX23456789";

    [Params("1abcd", "abcd1", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa0")]
    public string Input { get; set; }

    private static readonly char[] _validChars = ValidChars.ToCharArray();
    private static readonly SearchValues<char> _searchValues = SearchValues.Create(_validChars.AsSpan());
    private static readonly HashSet<char> _hashSet = [.._validChars];

    [GeneratedRegex($"^[{ValidChars}]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant, 1)]
    public static partial Regex RegexSearch();

    [Benchmark]
    public bool FindAll() => Input.All(c => _validChars.Contains(c));

    [Benchmark]
    public bool FindSearchValue() => !Input.AsSpan().ContainsAnyExcept(_searchValues);

    [Benchmark]
    public bool FindHashSet() => Input.All(c => _hashSet.Contains(c));
    
    [Benchmark]
    public bool FindRegex() => RegexSearch().IsMatch(Input.AsSpan());
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
   | FindAll         | 1abcd                |  17.715 ns | 0.3847 ns |  0.7771 ns |  17.347 ns | 0.0068 |      32 B |
   | FindSearchValue | 1abcd                |   3.283 ns | 0.0687 ns |  0.0643 ns |   3.272 ns |      - |         - |
   | FindHashSet     | 1abcd                |  14.604 ns | 0.3877 ns |  1.1061 ns |  14.450 ns | 0.0068 |      32 B |
   | FindRegex       | 1abcd                |  30.411 ns | 0.5980 ns |  1.5112 ns |  30.178 ns |      - |         - |
   | FindAll         | aaaaa(...)aaaa0 [49] | 425.048 ns | 8.2942 ns | 23.6638 ns | 419.627 ns | 0.0067 |      32 B |
   | FindSearchValue | aaaaa(...)aaaa0 [49] |   5.601 ns | 0.1343 ns |  0.1256 ns |   5.581 ns |      - |         - |
   | FindHashSet     | aaaaa(...)aaaa0 [49] | 324.126 ns | 6.4884 ns | 13.2541 ns | 321.273 ns | 0.0067 |      32 B |
   | FindRegex       | aaaaa(...)aaaa0 [49] |  34.307 ns | 0.6447 ns |  0.6030 ns |  34.246 ns |      - |         - |
   | FindAll         | abcd1                |  47.850 ns | 0.9433 ns |  0.9264 ns |  47.700 ns | 0.0068 |      32 B |
   | FindSearchValue | abcd1                |   6.835 ns | 0.0635 ns |  0.0530 ns |   6.844 ns |      - |         - |
   | FindHashSet     | abcd1                |  38.298 ns | 0.7842 ns |  0.8053 ns |  38.188 ns | 0.0068 |      32 B |
   | FindRegex       | abcd1                |  33.237 ns | 0.6971 ns |  0.6521 ns |  33.115 ns |      - |         - |
   
   // * Warnings *
   MultimodalDistribution
     SearchStringPerformance.FindRegex: .NET 8.0 -> It seems that the distribution is bimodal (mValue = 3.28)
   
   // * Hints *
   Outliers
     SearchStringPerformance.FindAll: .NET 8.0         -> 4 outliers were removed (22.80 ns..26.55 ns)
     SearchStringPerformance.FindHashSet: .NET 8.0     -> 6 outliers were removed (19.63 ns..22.79 ns)
     SearchStringPerformance.FindRegex: .NET 8.0       -> 8 outliers were removed (37.55 ns..42.79 ns)
     SearchStringPerformance.FindAll: .NET 8.0         -> 5 outliers were removed (497.57 ns..569.51 ns)
     SearchStringPerformance.FindHashSet: .NET 8.0     -> 1 outlier  was  removed (377.72 ns)
     SearchStringPerformance.FindSearchValue: .NET 8.0 -> 2 outliers were removed, 5 outliers were detected (8.40 ns..8.41 ns, 8.74 ns, 8.96 ns)
     SearchStringPerformance.FindHashSet: .NET 8.0     -> 1 outlier  was  removed (44.70 ns)
     SearchStringPerformance.FindRegex: .NET 8.0       -> 2 outliers were removed (39.27 ns, 39.33 ns)
   
 */
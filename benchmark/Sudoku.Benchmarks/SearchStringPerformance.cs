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
   
   nuvolaris.io - Dino (open risk)
   
   System.Runtime.Intrinsics.Vector128.IsHardwareAccelerated -> true

   
   // * Summary *
   
   BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.5011/22H2/2022Update)
   12th Gen Intel Core i7-12800H, 1 CPU, 20 logical and 14 physical cores
   .NET SDK 9.0.100-preview.4.24267.66
     [Host]   : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
     .NET 8.0 : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
   
   Job=.NET 8.0  Runtime=.NET 8.0
   
   | Method          | Input                | Mean       | Error      | StdDev     | Gen0   | Allocated |
   |---------------- |--------------------- |-----------:|-----------:|-----------:|-------:|----------:|
   | FindAll         | 1abcd                |  16.612 ns |  0.4905 ns |  1.4231 ns | 0.0025 |      32 B |
   | FindSearchValue | 1abcd                |   2.649 ns |  0.2025 ns |  0.5809 ns |      - |         - |
   | FindHashSet     | 1abcd                |  11.147 ns |  0.4659 ns |  1.3738 ns | 0.0025 |      32 B |
   | FindRegex       | 1abcd                |  23.350 ns |  0.6691 ns |  1.9411 ns |      - |         - |
   | FindAll         | aaaaa(...)aaaa0 [49] | 396.498 ns | 23.1844 ns | 68.3598 ns | 0.0024 |      32 B |
   | FindSearchValue | aaaaa(...)aaaa0 [49] |   5.385 ns |  0.3651 ns |  1.0475 ns |      - |         - |
   | FindHashSet     | aaaaa(...)aaaa0 [49] | 260.523 ns |  8.2829 ns | 24.2924 ns | 0.0024 |      32 B |
   | FindRegex       | aaaaa(...)aaaa0 [49] |  26.135 ns |  1.1266 ns |  3.3218 ns |      - |         - |
   | FindAll         | abcd1                |  42.037 ns |  2.3716 ns |  6.9926 ns | 0.0025 |      32 B |
   | FindSearchValue | abcd1                |   5.957 ns |  0.1504 ns |  0.3269 ns |      - |         - |
   | FindHashSet     | abcd1                |  29.993 ns |  1.3050 ns |  3.8478 ns | 0.0025 |      32 B |
   | FindRegex       | abcd1                |  25.030 ns |  0.7441 ns |  2.1940 ns |      - |         - |
   
   // * Warnings *
   MultimodalDistribution
     SearchStringPerformance.FindSearchValue: .NET 8.0 ->  can have several modes (mValue = 3.09)
     SearchStringPerformance.FindHashSet: .NET 8.0     ->  can have several modes (mValue = 3.14)
     SearchStringPerformance.FindRegex: .NET 8.0       ->  is bimodal (mValue = 4.07)
     SearchStringPerformance.FindAll: .NET 8.0         ->  can have several modes (mValue = 3)
     SearchStringPerformance.FindHashSet: .NET 8.0     ->  is bimodal (mValue = 3.3)
   
   // * Hints *
   Outliers
     SearchStringPerformance.FindAll: .NET 8.0         -> 3 outliers were removed, 4 outliers were detected (14.28 ns, 22.35 ns..25.93 ns)
     SearchStringPerformance.FindSearchValue: .NET 8.0 -> 5 outliers were removed (7.10 ns..10.32 ns)
     SearchStringPerformance.FindHashSet: .NET 8.0     -> 1 outlier  was  detected (8.26 ns)
     SearchStringPerformance.FindRegex: .NET 8.0       -> 3 outliers were removed (30.30 ns..30.89 ns)
     SearchStringPerformance.FindSearchValue: .NET 8.0 -> 5 outliers were removed, 8 outliers were detected (4.41 ns..4.66 ns, 10.21 ns..11.56 ns)
     SearchStringPerformance.FindHashSet: .NET 8.0     -> 1 outlier  was  removed, 4 outliers were detected (190.90 ns..197.82 ns, 338.30 ns)
     SearchStringPerformance.FindRegex: .NET 8.0       -> 5 outliers were detected (18.22 ns..19.47 ns)
     SearchStringPerformance.FindRegex: .NET 8.0       -> 5 outliers were detected (19.20 ns..22.26 ns)
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
     SearchStringPerformance.FindRegex: .NET 8.0 ->  is bimodal (mValue = 3.28)
   
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
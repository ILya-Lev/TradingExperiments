using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Sudoku.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class PolynomialCalculatorPerformance
{
    private List<decimal> X { get; set; }
    public decimal[] Coefficients { get; set; }

    [IterationSetup]
    public void GenerateSource()
    {
        Coefficients = Enumerable.Range(1, 100)
            .Select(n => n + (decimal)Random.Shared.NextDouble())
            .ToArray();

        X = Enumerable.Range(1, 100_000)
            .Select(n => 1.0001m + (decimal)Random.Shared.NextDouble()/100)
            .ToList();
    }

    [Benchmark] public void Straightforward() => X.ForEach(x => Coefficients.GetValueStraightforward(x));
    [Benchmark] public void Linq() => X.ForEach(x => Coefficients.GetValueLinq(x));
    [Benchmark] public void Horner() => X.ForEach(x => Coefficients.GetValue(x));
    [Benchmark] public void Plinq() => X.ForEach(x => Coefficients.GetValuePlinq(x));

    [Benchmark]
    public void OuterParallelAndHorner() => Parallel.ForEach
    (
        X,
        new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 },
        x => Coefficients.GetValue(x)
    );
}

/*
  * Summary *
   
   BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3915)
   Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
   .NET SDK 9.0.203
     [Host]   : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
     .NET 9.0 : .NET 9.0.4 (9.0.425.16305), X64 RyuJIT AVX2
   
   Job=.NET 9.0  Runtime=.NET 9.0  InvocationCount=1
   UnrollFactor=1
   
    //25 items

   | Method          | Mean     | Error     | StdDev    | Median   | Allocated |
   |---------------- |---------:|----------:|----------:|---------:|----------:|
   | Straightforward | 6.901 us | 0.6331 us | 1.7959 us | 6.150 us |      64 B |
   | Linq            | 9.715 us | 0.3005 us | 0.7968 us | 9.450 us |     624 B |
   | Horner          | 2.078 us | 0.0503 us | 0.1016 us | 2.100 us |     400 B |

    //50 items 
   
   | Method          | Mean      | Error     | StdDev    | Median    | Allocated |
   |---------------- |----------:|----------:|----------:|----------:|----------:|
   | Straightforward | 15.270 us | 1.3358 us | 3.7676 us | 14.950 us |     112 B |
   | Linq            | 16.315 us | 0.3393 us | 0.9056 us | 16.050 us |     624 B |
   | Horner          |  3.965 us | 0.0846 us | 0.1669 us |  3.900 us |     400 B |

    //75 items
   
   | Method          | Mean      | Error     | StdDev    | Allocated |
   |---------------- |----------:|----------:|----------:|----------:|
   | Straightforward | 14.597 us | 0.2941 us | 0.7215 us |     400 B |
   | Linq            | 22.720 us | 0.4521 us | 0.9635 us |     624 B |
   | Horner          |  5.494 us | 0.1125 us | 0.1577 us |     400 B |
   
    //100 items

   | Method          | Mean      | Error     | StdDev    | Allocated |
   |---------------- |----------:|----------:|----------:|----------:|
   | Straightforward | 18.550 us | 0.3016 us | 0.2355 us |     400 B |
   | Linq            | 28.467 us | 0.5552 us | 0.5941 us |     624 B |
   | Horner          |  6.946 us | 0.1435 us | 0.1198 us |     400 B |

    //100 items with 100k x values
   | Method          | Mean       | Error    | StdDev   | Gen0      | Allocated  |
   |---------------- |-----------:|---------:|---------:|----------:|-----------:|
   | Straightforward | 1,267.8 ms |  4.33 ms |  3.61 ms |         - |      464 B |
   | Linq            | 1,394.6 ms | 15.34 ms | 14.35 ms | 4000.0000 | 22_400_464 B |
   | Horner          |   584.8 ms |  8.29 ms |  7.75 ms |         - |      464 B |

    //100 items with 100k x values; 
    //Plinq on the level of each polynomial
    //outer parallel on the level of X   
   | Method                 | Mean       | Error    | StdDev   | Gen0         | Gen1      | Allocated    |
   |----------------------- |-----------:|---------:|---------:|-------------:|----------:|-------------:|
   | Straightforward        | 1,289.6 ms | 11.65 ms | 10.90 ms |            - |         - |        464 B |
   | Linq                   | 1,368.0 ms | 11.26 ms |  9.41 ms |    4000.0000 |         - |   21.4 MB |
   | Horner                 |   582.2 ms |  5.32 ms |  4.97 ms |            - |         - |        464 B |
   | Plinq                  | 4,054.3 ms | 76.87 ms | 71.90 ms | 1073000.0000 | 2000.0000 | 3.91 GB |
   | OuterParallelAndHorner |   109.2 ms |  2.18 ms |  4.56 ms |            - |         - |       4480 B |
   
 */
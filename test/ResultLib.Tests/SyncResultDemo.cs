using Xunit.Abstractions;

namespace ResultLib.Tests;

public class SyncResultDemo(ITestOutputHelper output)
{
    [Fact]
    public void MakeThreeSteps()
    {
        for (int i = 0; i < 10; i++)
        {
            var c = Demo();
            output.WriteLine(c.Error ?? $"{c.Value}");
        }
    }

    private Result<C> Demo()
    {
        var g = GetA;
        var a = g.Curry(DateTime.Now);
        
        var f = GetC;
        var c = f.Curry(DateTime.Now)!.Curry(a().Value);
        return GetB(DateTime.Now).Transform(c);
    }

    private Result<A> GetA(DateTime dt)
        => dt.Microsecond % 2 == 0
        ? Result.Create(new A(dt.Microsecond))
        : Result.Create<A>(error: $"no A for {dt.Microsecond}");

    private Result<B> GetB(DateTime dt)
        => dt.Microsecond % 2 == 0
        ? Result.Create(new B(dt.Microsecond))
        : Result.Create<B>(error: $"no B for {dt.Microsecond}");

    private Result<C> GetC(DateTime dt, A a, B b)
        => dt.Microsecond % 2 == 0
        ? Result.Create(new C(dt.Microsecond + a.a + b.b))
        : Result.Create<C>(error: $"no C for {dt.Microsecond}");

    private record A(int a);
    private record B(int b);
    private record C(int c);
}
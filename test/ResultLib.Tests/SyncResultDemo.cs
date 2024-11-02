using Xunit.Abstractions;

namespace ResultLib.Tests;

public class SyncResultDemo(ITestOutputHelper output)
{
    [Fact]
    public void MakeThreeSteps()
    {
        for (int i = 0; i < 15; i++)
        {
            var c = Demo();
            output.WriteLine(c.Error ?? $"{c.Value}");
        }
    }

    private Result<C> Demo()
    {
        var a = GetA;
        var b = GetB;
        var c = GetC;

        var dt = DateTime.Now;

        return a(dt).Apply(b(dt).CurryApply(c.Curry(dt)));
    }

    private Result<A> GetA(DateTime dt)
        => dt.Microsecond % 2 == 0
        ? new A(dt.Microsecond)
        : Result<A>.Fail($"no A for {dt.Microsecond}");

    private Result<B> GetB(DateTime dt)
        => dt.Microsecond % 3 == 0
        ? new B(dt.Microsecond)
        : Result<B>.Fail($"no B for {dt.Microsecond}");

    private Result<C> GetC(DateTime dt, A a, B b)
        => dt.Microsecond % 5 == 0
        ? new C(dt.Microsecond + a.a + b.b)
        : Result<C>.Fail(error: $"no C for {dt.Microsecond}");

    private record A(int a);
    private record B(int b);
    private record C(int c);
}
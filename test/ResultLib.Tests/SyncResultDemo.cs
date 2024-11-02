using Xunit.Abstractions;

namespace ResultLib.Tests;

public class SyncResultDemo(ITestOutputHelper output)
{
    [Fact]
    public void MakeThreeSteps()
    {
        for (int i = 0; i < 15; i++)
        {
            var c = Demo1();
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

    private Result<C> Demo1() =>
        from dt in Result<DateTime>.Success(DateTime.Now)
        from a in GetA(dt)
        let b = GetB1(dt)
        from c in GetC(dt, a, b)
        select c;

    private Result<C> Demo2() => Result<DateTime>
        .Success(DateTime.Now)
        .SelectMany(GetA, (dt, a) => (dt, a))
        .SelectMany(dtWa => GetB(dtWa.dt), (dtWa, b) => (dtWa.dt, dtWa.a, b))
        .SelectMany(e => GetC(e.dt, e.a, e.b), (_, c) => c);

    private Result<A> GetA(DateTime dt)
        => dt.Microsecond % 2 == 0
        ? new A(dt.Microsecond)
        : Result<A>.Fail($"no A for {dt.Microsecond}");

    private Result<B> GetB(DateTime dt)
        => dt.Microsecond % 3 == 0
        ? new B(dt.Microsecond)
        : Result<B>.Fail($"no B for {dt.Microsecond}");

    private B GetB1(DateTime dt) => new B(dt.Microsecond);

    private Result<C> GetC(DateTime dt, A a, B b)
        => dt.Microsecond % 5 == 0
        ? new C(dt.Microsecond + a.a + b.b)
        : Result<C>.Fail(error: $"no C for {dt.Microsecond}");

    private record A(int a);
    private record B(int b);
    private record C(int c);
}
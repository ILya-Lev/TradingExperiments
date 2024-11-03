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

    [Fact]
    public async Task MakeThreeStepsAsync()
    {
        for (int i = 0; i < 15; i++)
        {
            var c = await Demo3();
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

    private async Task<Result<C>> Demo2() => await Task
        .FromResult(Result<DateTime>.Success(DateTime.Now))
        .SelectMany(GetA1, (dt, a) => (dt, a))
        .SelectMany(dtWa => GetB2(dtWa.dt), (dtWa, b) => (dtWa.dt, dtWa.a, b))
        .SelectMany(e => GetC1(e.dt, e.a, e.b), (_, c) => c);

    private async Task<Result<C>> Demo3() =>
        await 
        from dt in Task.FromResult(Result<DateTime>.Success(DateTime.Now))
        from a in GetA1(dt)
        from b in GetB2(dt)
        from c in GetC1(dt, a, b)
        select c;

    private async Task<Result<A>> GetA1(DateTime dt) => GetA(dt);
    private async Task<Result<B>> GetB2(DateTime dt) => GetB(dt);
    private async Task<Result<C>> GetC1(DateTime dt, A a, B b) => GetC(dt, a, b);

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
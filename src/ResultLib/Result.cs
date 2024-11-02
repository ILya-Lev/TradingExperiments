namespace ResultLib;

public class Result<T>
{
    public T? Value { get; init; }
    public string? Error { get; init; }
}

public class Result
{
    public static Result<T> Create<T>(T? value = default(T?), string? error = null)
    {
        if (value is null && error is null)
            throw new InvalidOperationException($"it is not allowed to have both value and error null");
        if (value is not null && error is not null)
            throw new InvalidOperationException($"it is not allowed to have both value and error not null");

        return new Result<T> { Value = value, Error = error };
    }
}

public static class ResultExtensions
{
    public static Result<V> Transform<U, V>(this Result<U> current, Func<U, V> projection)
    {
        if (current.Error is not null)
            return Result.Create<V>(error: current.Error);
        try
        {
            return Result.Create(value: projection(current.Value!));
        }
        catch (Exception exc)
        {
            return Result.Create<V>(error: exc.ToString());
        }
    }
    
    public static Result<V> Transform<U, V>(this Result<U> current, Func<U, Result<V>> projection)
    {
        if (current.Error is not null)
            return Result.Create<V>(error: current.Error);
        try
        {
            return projection(current.Value!);
        }
        catch (Exception exc)
        {
            return Result.Create<V>(error: exc.ToString());
        }
    }

    public static Result<T> SideEffect<T>(this Result<T> current, Action<T> sideEffect)
    {
        if (current.Error is not null)
            return current;
        try
        {
            sideEffect(current.Value!);
            return current;
        }
        catch (Exception exc)
        {
            return Result.Create<T>(error: exc.ToString());
        }
    }

    public static async Task<Result<V>> TransformAsync<U, V>(this Result<U> current, Func<U, Task<V>> projection)
    {
        if (current.Error is not null)
            return Result.Create<V>(error: current.Error);
        try
        {
            return Result.Create(value: await projection(current.Value!));
        }
        catch (Exception exc)
        {
            return Result.Create<V>(error: exc.ToString());
        }
    }

    public static async Task<Result<V>> TransformAsync<U, V>(this Task<Result<U>> current, Func<U, Task<V>> projection)
    {
        var c = await current;
        if (c.Error is not null)
            return Result.Create<V>(error: c.Error);
        try
        {
            return Result.Create(value: await projection(c.Value!));
        }
        catch (Exception exc)
        {
            return Result.Create<V>(error: exc.ToString());
        }
    }

    public static async Task<Result<T>> SideEffectAsync<T>(this Result<T> current, Func<T, Task> sideEffect)
    {
        if (current.Error is not null)
            return current;
        try
        {
            await sideEffect(current.Value!);
            return current;
        }
        catch (Exception exc)
        {
            return Result.Create<T>(error: exc.ToString());
        }
    }

    public static async Task<Result<T>> SideEffectAsync<T>(this Task<Result<T>> current, Func<T, Task> sideEffect)
    {
        var c = await current;
        if (c.Error is not null)
            return c;
        try
        {
            await sideEffect(c.Value!);
            return c;
        }
        catch (Exception exc)
        {
            return Result.Create<T>(error: exc.ToString());
        }
    }

    public static Func<R> Curry<T1, R>(this Func<T1, R> f, T1 v) => () => f(v);
    public static Func<T2, R> Curry<T1, T2, R>(this Func<T1, T2, R> f, T1 v) => v2 => f(v, v2);
    public static Func<T2, T3, R> Curry<T1, T2, T3, R>(this Func<T1, T2, T3, R> f, T1 v) => (v2, v3) => f(v, v2, v3);
}
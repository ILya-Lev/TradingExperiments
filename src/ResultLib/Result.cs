namespace ResultLib;

public class Result<T>
{
    public T? Value { get; }
    public string? Error { get; }
    public bool IsSuccess { get; }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Fail(string error) => new(error: error);
    public static implicit operator Result<T>(T value) => Success(value);

    private Result(T? value = default(T?), string? error = null)
    {
        if (value is null && error is null)
            throw new InvalidOperationException($"it is not allowed to have both value and error null");
        if (value is not null && error is not null)
            throw new InvalidOperationException($"it is not allowed to have both value and error not null");

        Value = value;
        Error = error;
        IsSuccess = error is null;
    }
}

public static class ResultExtensions
{
    public static Result<V> Apply<U, V>(this Result<U> current, Func<U, V> projection)
    {
        if (current.Error is not null)
            return Result<V>.Fail(current.Error);
        try
        {
            return projection(current.Value!);
        }
        catch (Exception exc)
        {
            return Result<V>.Fail(exc.ToString());
        }
    }
    
    public static Result<V> Apply<U, V>(this Result<U> current, Func<U, Result<V>> projection)
    {
        if (current.Error is not null)
            return Result<V>.Fail(current.Error);
        try
        {
            return projection(current.Value!);
        }
        catch (Exception exc)
        {
            return Result<V>.Fail(exc.ToString());
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
            return Result<T>.Fail(exc.ToString());
        }
    }

    public static async Task<Result<V>> TransformAsync<U, V>(this Result<U> current, Func<U, Task<V>> projection)
    {
        if (current.Error is not null)
            return Result<V>.Fail(current.Error);
        try
        {
            return await projection(current.Value!);
        }
        catch (Exception exc)
        {
            return Result<V>.Fail(exc.ToString());
        }
    }

    public static async Task<Result<V>> TransformAsync<U, V>(this Task<Result<U>> current, Func<U, Task<V>> projection)
    {
        var c = await current;
        if (c.Error is not null)
            return Result<V>.Fail(c.Error);
        try
        {
            return await projection(c.Value!);
        }
        catch (Exception exc)
        {
            return Result<V>.Fail(exc.ToString());
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
            return Result<T>.Fail(exc.ToString());
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
            return Result<T>.Fail(exc.ToString());
        }
    }

    public static Func<R> Curry<T1, R>(this Func<T1, R> f, T1 v) => () => f(v);
    public static Func<T2, R> Curry<T1, T2, R>(this Func<T1, T2, R> f, T1 v) => v2 => f(v, v2);
    public static Func<T2, T3, R> Curry<T1, T2, T3, R>(this Func<T1, T2, T3, R> f, T1 v) => (v2, v3) => f(v, v2, v3);

    public static Func<U, Result<V>> CurryApply<T, U, V>(this Result<T> current, Func<T, U, Result<V>> projection)
    {
        if (current.Error is not null)
            return (_) => Result<V>.Fail(current.Error);
        try
        {
            return (u) => projection(current.Value!, u);
        }
        catch (Exception exc)
        {
            return (_) => Result<V>.Fail(exc.ToString());
        }
    }

    public static Func<T, Result<V>> CurryApply<T, U, V>(this Result<U> current, Func<T, U, Result<V>> projection)
    {
        if (current.Error is not null)
            return (_) => Result<V>.Fail(current.Error);
        try
        {
            return (u) => projection(u, current.Value!);
        }
        catch (Exception exc)
        {
            return (_) => Result<V>.Fail(exc.ToString());
        }
    }
}
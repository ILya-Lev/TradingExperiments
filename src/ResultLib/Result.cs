namespace ResultLib;

public class Result<T>
{
    public T? Value { get; }
    public string? Error { get; }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Fail(string error) => new(error: error);
    public static implicit operator Result<T>(T value) => Success(value);

    private Result(T? value = default(T?), string? error = null)
    {
        Value = value;
        Error = error;
    }
}

public static class ResultExtensions
{
    public static Result<V> Select<U, V>(this Result<U> current, Func<U, V> projection)
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

    public static async Task<Result<V>> Select<U, V>(this Task<U> current
        , Func<U, Task<V>> projection)
    {
        var c = await current;
        
        try
        {
            return await projection(c);
        }
        catch (Exception exc)
        {
            return Result<V>.Fail(exc.ToString());
        }
    }

    public static Result<R> SelectMany<T, M, R>(this Result<T> current, Func<T, Result<M>> projection, Func<T, M, R> aggregator)
    {
        if (current.Error is not null)
            return Result<R>.Fail(current.Error);
        try
        {
            var m = projection(current.Value!);
            if (m.Error is not null)
                return Result<R>.Fail(m.Error);

            return aggregator(current.Value!, m.Value!);
        }
        catch (Exception exc)
        {
            return Result<R>.Fail(exc.ToString());
        }
    }

    //based on https://andrewlock.net/working-with-the-result-pattern-part-3-adding-more-extensions/
    public static async Task<Result<R>> SelectMany<T, M, R>(this Task<Result<T>> current
        , Func<T, Task<Result<M>>> projection
        , Func<T, M, R> aggregator)
    {
        var c = await current;
        if (c.Error is not null)
            return Result<R>.Fail(c.Error);
        try
        {
            var m = await projection(c.Value!);
            if (m.Error is not null)
                return Result<R>.Fail(m.Error);

            return aggregator(c.Value!, m.Value!);
        }
        catch (Exception exc)
        {
            return Result<R>.Fail(exc.ToString());
        }
    }

    public static Result<IEnumerable<T>> Sequence<T>(this IEnumerable<Result<T>> results)
    {
        var empty = Result<IEnumerable<T>>.Success([]);
        return results.Aggregate(empty,
            (acc, current) =>
                acc.SelectMany(previous => current,
                    (previous, c) => previous.Append(c))
                //from previous in acc
                //from c in current
                //select previous.Append(c)
        );
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
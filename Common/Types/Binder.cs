using System;

namespace Common.Types;

public static class Binder
{
    public static TResult Bind<TValue, TResult>(this TValue value, Func<TValue, TResult> action)
        => action(value);
    public static TResult Maybe<TValue, TResult>(this TValue value, string ifNull, Func<TValue, TResult> action)
        => value == null ?
        throw new ArgumentNullException(ifNull) :
        action(value);
}

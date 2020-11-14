using System;

namespace ParserObjects
{
    public static partial class ParserMethods<TInput>
    {
        public static IParser<TInput, TState> GetData<TState>()
            where TState : class
            => Function<TState>((t, success, fail) => success(t.Data as TState));

        public static IParser<TInput, TValue> GetData<TState, TValue>(Func<TState, TValue> getValue)
            where TState : class
            => Function<TValue>((t, success, fail) => success(getValue(t.Data as TState)));

        public static IParser<TInput, TState> SetData<TState>(Func<TState, TState> set)
            where TState : class
            => Function<TState>((t, success, fail) =>
            {
                t.Data = set(t.Data as TState);
                return success(t.Data as TState);
            });

        public static IParser<TInput, TState> UpdateData<TState>(Action<TState> update)
            where TState : class
            => Function<TState>((t, success, fail) =>
            {
                update(t.Data as TState);
                return success(t.Data as TState);
            });
    }
}

using System;

namespace ParserObjects
{
    public static partial class ParserMethods<TInput>
    {
        public static IParser<TInput, TState> GetData<TState>()
            where TState : class
            => Function(t => t.Success(t.Data as TState));

        public static IParser<TInput, TValue> GetData<TState, TValue>(Func<TState, TValue> getValue)
            where TState : class
            => Function(t => t.Success(getValue(t.Data as TState)));

        public static IParser<TInput, TState> SetData<TState>(Func<TState, TState> set)
            where TState : class
            => Function(t =>
            {
                t.Data = set(t.Data as TState);
                return Result.Success(t.Data as TState);
            });

        public static IParser<TInput, TState> UpdateData<TState>(Action<TState> update)
            where TState : class
            => Function(t =>
            {
                update(t.Data as TState);
                return Result.Success(t.Data as TState);
            });
    }
}

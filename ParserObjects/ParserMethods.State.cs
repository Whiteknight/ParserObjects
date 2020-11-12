using System;

namespace ParserObjects
{
    public static partial class ParserMethods<TInput>
    {
        // TODO: need a parser to push state, so we can recurse, and then pop state again when we 
        // are done.
        // TODO: Would like a mechanism sort of like Examine, but so we can insert callbacks before
        // and after a parse so we can do setup/cleanup work in a way that isn't centered around
        // debugging
        // TODO: Parser wrapper/extension so that the output result of one parser can be stored as
        // data in the state
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

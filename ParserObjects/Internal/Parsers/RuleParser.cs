using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Rule parses several parsers in order, failing if any parser fails. The results of each is
/// stored in an array and passed to a generator function to create the final result value.
/// </summary>
public static class Rule
{
    /* The Create() function helps us with type inference because downstream usages of Rule can
     * be quite messy. The Parser class implements the parser logic.
     */

    public static IParser<TInput, TOutput> Create<TInput, TOutput, TData>(
        IReadOnlyList<IParser<TInput>> parsers,
        TData data,
        Func<TData, IReadOnlyList<object>, TOutput> produce
    ) => new Parser<TInput, TOutput, TData, IParser<TInput>, object>(parsers, data,
        static (state, parser) => parser.Parse(state),
        static (state, parser) => parser.Match(state),
        produce
    );

    public static IParser<TInput, TOutput> CreateTyped<TInput, TItem, TOutput, TData>(
        IReadOnlyList<IParser<TInput, TItem>> parsers,
        TData data,
        Func<TData, IReadOnlyList<TItem>, TOutput> produce
    ) => new Parser<TInput, TOutput, TData, IParser<TInput, TItem>, TItem>(parsers, data,
        static (state, parser) => parser.Parse(state),
        static (state, parser) => parser.Match(state),
        produce
    );

    /// <summary>
    /// Parses a list of steps in sequence and produces a single output as a combination of outputs
    /// from each step. Succeeds or fails as an atomic unit.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TParser"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="Parsers"></param>
    /// <param name="Data"></param>
    /// <param name="ParseItem"></param>
    /// <param name="MatchItem"></param>
    /// <param name="Produce"></param>
    /// <param name="Name"></param>
    public sealed record Parser<TInput, TOutput, TData, TParser, TItem>(
        IReadOnlyList<TParser> Parsers,
        TData Data,
        Func<IParseState<TInput>, TParser, Result<TItem>> ParseItem,
        Func<IParseState<TInput>, TParser, bool> MatchItem,
        Func<TData, IReadOnlyList<TItem>, TOutput> Produce,
        string Name = ""

    ) : IParser<TInput, TOutput>
        where TParser : IParser
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public Result<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.NotNull(state);
            return Parse(state, new TItem[Parsers.Count]);
        }

        Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

        public bool Match(IParseState<TInput> state)
        {
            Assert.NotNull(state);

            var startCheckpoint = state.Input.Checkpoint();

            for (int i = 0; i < Parsers.Count; i++)
            {
                var result = MatchItem(state, Parsers[i]);
                if (result)
                    continue;

                startCheckpoint.Rewind();
                return false;
            }

            // NOTE: It is possible that Produce() would have thrown an exception, in which case
            // we technically should return false. But we can't call Produce without calling .Parse()
            // on all children, which has a bunch of allocations, which goes against what Match() is
            // trying to do. So we return true.

            return true;
        }

        public IEnumerable<IParser> GetChildren() => Parsers.Cast<IParser>();

        public override string ToString() => DefaultStringifier.ToString("Rule", Name, Id);

        public INamed SetName(string name) => this with { Name = name };

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }

        private Result<TOutput> Parse(IParseState<TInput> state, TItem[] outputs)
        {
            var startCheckpoint = state.Input.Checkpoint();
            for (int i = 0; i < Parsers.Count; i++)
            {
                var result = ParseItem(state, Parsers[i]);
                if (result.Success)
                {
                    outputs[i] = result.Value;
                    continue;
                }

                startCheckpoint.Rewind();
                var name = Parsers[i].Name;
                if (string.IsNullOrEmpty(name))
                    name = "(Unnamed)";
                return Result.Fail(this, $"Parser {i} {name} failed");
            }

            var consumed = state.Input.Consumed - startCheckpoint.Consumed;
            var resultValue = Produce(Data, outputs);
            return Result.Ok(this, resultValue, consumed);
        }
    }
}

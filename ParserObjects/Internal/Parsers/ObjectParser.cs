using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

// A wrapper to convert IParser<TInput> to IParser<TInput, object>
public static class Objects<TInput>
{
    public static IParser<TInput, object> AsObject(IParser<TInput> parser)
        => parser is IParser<TInput, object> typed
        ? typed :
        new Parser(parser);

    public sealed class Parser : SimpleParser<TInput, object>
    {
        private readonly IParser<TInput> _inner;

        public Parser(IParser<TInput> inner, string name = "")
            : base(name)
        {
            _inner = inner;
        }

        public override Result<object> Parse(IParseState<TInput> state) => _inner.Parse(state);

        public override INamed SetName(string name) => new Parser(_inner, name);

        public override void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }

        public override string ToString() => DefaultStringifier.ToString("Object", Name, Id);
    }
}

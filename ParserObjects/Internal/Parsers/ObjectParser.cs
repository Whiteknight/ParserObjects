using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

public sealed class ObjectParser<TInput> : SimpleParser<TInput, object>
{
    private readonly IParser<TInput> _inner;

    public ObjectParser(IParser<TInput> inner, string name = "")
        : base(name)
    {
        _inner = inner;
    }

    public override Result<object> Parse(IParseState<TInput> state) => _inner.Parse(state);

    public override INamed SetName(string name) => new ObjectParser<TInput>(_inner, name);

    public override void Visit<TVisitor, TState>(TVisitor visitor, TState state)
    {
        visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
    }

    public override string ToString() => DefaultStringifier.ToString("Object", Name, Id);
}

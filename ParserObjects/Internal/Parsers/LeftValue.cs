using System;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// A constant left value for places where a left value is used to start a parse.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public sealed class LeftValue<TInput, TOutput> : SimpleParser<TInput, TOutput>
{
    public LeftValue(string name)
        : base(name)
    {
    }

    public TOutput? Value { get; set; }

    public override Result<TOutput> Parse(IParseState<TInput> state)
        => Result<TOutput>.Ok(this, Value!, 0);

    public override bool Match(IParseState<TInput> state) => true;

    public override INamed SetName(string name)
        => throw new InvalidOperationException("Cannot rename inner value parser");

    public override void Visit<TVisitor, TState>(TVisitor visitor, TState state)
    {
        // At the moment LeftValue does not participate in visiting operations.
    }

    public override string ToString() => DefaultStringifier.ToString(this);
}

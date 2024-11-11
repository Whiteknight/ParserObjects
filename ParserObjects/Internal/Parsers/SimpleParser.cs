using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Internal.Parsers;

public abstract class SimpleParser<TInput, TOutput> : IParser<TInput, TOutput>
{
    protected SimpleParser(string name)
    {
        Name = name;
    }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Name { get; }

    public virtual IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public virtual bool Match(IParseState<TInput> state) => Parse(state).Success;

    public abstract Result<TOutput> Parse(IParseState<TInput> state);

    public abstract INamed SetName(string name);

    public abstract void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>;

    Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

    public override string ToString() => DefaultStringifier.ToString(this);
}

public abstract record SimpleRecordParser<TInput, TOutput>(
    string Name
) : IParser<TInput, TOutput>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public virtual IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public virtual bool Match(IParseState<TInput> state) => Parse(state).Success;

    public abstract Result<TOutput> Parse(IParseState<TInput> state);

    public INamed SetName(string name) => this with { Name = name };

    public abstract void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>;

    Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();
}

public abstract record SimpleRecordParser<TInput>(
    string Name
) : IParser<TInput, object>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public virtual IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public virtual bool Match(IParseState<TInput> state) => Parse(state).Success;

    public abstract Result<object> Parse(IParseState<TInput> state);

    public INamed SetName(string name) => this with { Name = name };

    public abstract void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>;
}

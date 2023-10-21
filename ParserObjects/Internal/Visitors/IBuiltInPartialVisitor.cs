using ParserObjects.Internal.Parsers;

namespace ParserObjects.Internal.Visitors;

/// <summary>
/// Partial visitor interface for all built-in parser types.
/// </summary>
/// <typeparam name="TState"></typeparam>
public interface IBuiltInPartialVisitor<TState> :
    IRegexPartialVisitor<TState>, IFunctionPartialVisitor<TState>, IListPartialVisitor<TState>,
    ICorePartialVisitor<TState>, IAssociativePartialVisitor<TState>, IMultiPartialVisitor<TState>,
    ILogicalPartialVisitor<TState>, IEarleyPartialVisitor<TState>, IPrattPartialVisitor<TState>,
    IMatchPartialVisitor<TState>, ILookaheadPartialVisitor<TState>
{
}

public interface IRegexPartialVisitor<in TState> : IPartialVisitor<TState>
{
    void Accept(RegexParser p, TState state);
}

public interface IFunctionPartialVisitor<TState> : IPartialVisitor<TState>
{
    void Accept<TInput, TData>(Function<TInput>.Parser<TData> p, TState state);

    void Accept<TInput, TOutput, TData>(Function<TInput, TOutput>.MultiParser<TData> p, TState state);

    void Accept<TInput, TOutput, TData>(Function<TInput, TOutput>.Parser<TData> p, TState state);

    void Accept<TInput, TOutput>(Sequential.Parser<TInput, TOutput> _, TState state);
}

public interface IListPartialVisitor<TState> : IPartialVisitor<TState>
{
    void Accept<TInput, TOutput>(Repetition<TInput>.Parser<TOutput> p, TState state);

    void Accept<TInput>(Repetition<TInput>.Parser p, TState state);

    void Accept<TInput, TItem, TOutput>(NonGreedyListParser<TInput, TItem, TOutput> p, TState state);
}

public interface ICorePartialVisitor<TState> : IPartialVisitor<TState>
{
    void Accept<TInput, TOutput>(Cache<TInput>.Parser<TOutput> p, TState state);

    void Accept<TInput, TOutput>(Cache<TInput>.MultiParser<TOutput> p, TState state);

    void Accept<TInput>(Cache<TInput>.Parser p, TState state);

    void Accept<TInput, TOutput>(CaptureParser<TInput, TOutput> p, TState state);

    void Accept<TInput, TOutput, TData>(Chain<TInput, TOutput>.Parser<TData> p, TState state);

    void Accept<TInput, TOutput, TMiddle, TData>(Chain<TInput, TOutput>.Parser<TMiddle, TData> p, TState state);

    void Accept<TInput, TOutput>(Context<TInput>.MultiParser<TOutput> p, TState state);

    void Accept<TInput, TOutput>(Context<TInput>.Parser<TOutput> p, TState state);

    void Accept<TInput, TOutput>(Create<TInput, TOutput>.MultiParser _, TState state);

    void Accept<TInput, TOutput>(Create<TInput, TOutput>.Parser _, TState state);

    void Accept<TInput, TOutput>(DataFrame<TInput>.Parser<TOutput> p, TState state);

    void Accept<TInput, TOutput>(DataFrame<TInput>.MultiParser<TOutput> p, TState state);

    void Accept<TInput, TOutput>(Deferred<TInput, TOutput>.MultiParser p, TState state);

    void Accept<TInput, TOutput>(Deferred<TInput, TOutput>.Parser p, TState state);

    void Accept<TInput, TOutput>(Examine<TInput, TOutput>.MultiParser p, TState state);

    void Accept<TInput, TOutput>(Examine<TInput, TOutput>.Parser p, TState state);

    void Accept<TInput>(ExamineParser<TInput> p, TState state);

    void Accept<TInput, TOutput>(FailParser<TInput, TOutput> p, TState state);

    void Accept<TInput, TOutput>(FirstParser<TInput>.WithOutput<TOutput> p, TState state);

    void Accept<TInput>(FirstParser<TInput>.WithoutOutput p, TState state);

    void Accept<TInput, TOutput>(Optional<TInput, TOutput>.DefaultValueParser p, TState state);

    void Accept<TInput, TOutput>(Optional<TInput, TOutput>.NoDefaultParser p, TState state);

    void Accept<TInput>(Replaceable<TInput>.SingleParser p, TState state);

    void Accept<TInput, TOutput>(Replaceable<TInput, TOutput>.SingleParser p, TState state);

    void Accept<TInput, TOutput>(Replaceable<TInput, TOutput>.MultiParser p, TState state);

    void Accept<TInput, TOutput, TData>(Rule.Parser<TInput, TOutput, TData> p, TState state);

    void Accept<TInput, TOutput>(SynchronizeParser<TInput, TOutput> p, TState state);

    void Accept<TInput, TMiddle, TOutput>(Transform<TInput, TMiddle, TOutput>.MultiParser p, TState state);

    void Accept<TInput, TMiddle, TOutput>(Transform<TInput, TMiddle, TOutput>.Parser p, TState state);

    void Accept<TInput, TOutput>(TryParser<TInput>.Parser<TOutput> p, TState state);

    void Accept<TInput>(TryParser<TInput>.Parser p, TState state);

    void Accept<TInput, TOutput>(TryParser<TInput>.MultiParser<TOutput> p, TState state);
}

public interface IAssociativePartialVisitor<TState> : IPartialVisitor<TState>
{
    void Accept<TInput, TMiddle, TOutput>(RightApplyParser<TInput, TMiddle, TOutput> p, TState state);

    void Accept<TInput, TOutput>(LeftApplyParser<TInput, TOutput> p, TState state);
}

public interface IMultiPartialVisitor<TState> : IPartialVisitor<TState>
{
    void Accept<TInput, TMulti, TOutput>(ContinueWith<TInput, TMulti, TOutput>.MultiParser p, TState state);

    void Accept<TInput, TMulti, TOutput>(ContinueWith<TInput, TMulti, TOutput>.SingleParser p, TState state);

    void Accept<TInput, TOutput>(EachParser<TInput, TOutput> p, TState state);

    void Accept<TInput, TOutput>(SelectParser<TInput, TOutput> p, TState state);
}

public interface ILogicalPartialVisitor<TState> : IPartialVisitor<TState>
{
    void Accept<TInput>(AndParser<TInput> p, TState state);

    void Accept<TInput, TOutput>(IfParser<TInput, TOutput> p, TState state);
}

public interface IEarleyPartialVisitor<TState> : IPartialVisitor<TState>
{
    void Accept<TInput, TOutput>(Earley<TInput, TOutput>.Parser p, TState state);
}

public interface IPrattPartialVisitor<TState> : IPartialVisitor<TState>
{
    void Accept<TInput, TOutput>(PrattParser<TInput, TOutput> p, TState state);
}

public interface IMatchPartialVisitor<TState> : IPartialVisitor<TState>
{
    void Accept<TInput>(AnyParser<TInput> _, TState state);

    void Accept<TInput>(EmptyParser<TInput> _, TState state);

    void Accept<TInput>(EndParser<TInput> _, TState state);

    void Accept<TInput>(MatchItemParser<TInput> p, TState state);

    void Accept<TInput>(MatchPatternParser<TInput> p, TState state);

    void Accept<TInput, TData>(MatchPredicateParser<TInput, TData> _, TState state);

    void Accept<TInput>(PeekParser<TInput> _, TState state);

    void Accept<TInput, TOutput>(TrieParser<TInput, TOutput> p, TState state);
}

public interface ILookaheadPartialVisitor<TState> : IPartialVisitor<TState>
{
    void Accept<TInput>(NegativeLookaheadParser<TInput> p, TState state);

    void Accept<TInput>(PositiveLookaheadParser<TInput> p, TState state);
}

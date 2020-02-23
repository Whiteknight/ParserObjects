using System.Collections.Generic;

namespace ParserObjects.Parsers
{
    public interface ICoreVisitorDispatcher
    {
        IParser<TInput, TInput> VisitAny<TInput>(AnyParser<TInput> p);
        IParser<TInput, TOutput> VisitDeferred<TInput, TOutput>(DeferredParser<TInput, TOutput> p);
        IParser<TInput, object> VisitEmpty<TInput>(EmptyParser<TInput> p);
        IParser<TInput, bool> VisitEnd<TInput>(EndParser<TInput> p);
        IParser<TInput, TOutput> VisitFail<TInput, TOutput>(FailParser<TInput, TOutput> p);

        IParser<TInput, TOutput> VisitFirst<TInput, TOutput>(FirstParser<TInput, TOutput> p);

        IParser<TInput, TOutput> VisitFlatten<TInput, TCollection, TOutput>(FlattenParser<TInput, TCollection, TOutput> p)
            where TCollection : IEnumerable<TOutput>;
        IParser<TInput, TOutput> VisitLeftApplyZeroOrMore<TInput, TOutput>(LeftApplyZeroOrMoreParser<TInput, TOutput> p);

        IParser<TInput, IEnumerable<TOutput>> VisitList<TInput, TOutput>(ListParser<TInput, TOutput> p);

        IParser<TInput, TInput> VisitMatchPredicate<TInput>(MatchPredicateParser<TInput> p);
        IParser<TInput, IReadOnlyList<TInput>> VisitMatchSequence<TInput>(MatchSequenceParser<TInput> p);
        IParser<TInput, bool> VisitNegativeLookahead<TInput>(NegativeLookaheadParser<TInput> p);
        IParser<TInput, bool> VisitPositiveLookahead<TInput>(PositiveLookaheadParser<TInput> p);
        IParser<TInput, TOutput> VisitProduce<TInput, TOutput>(ProduceParser<TInput, TOutput> p);
        IParser<TInput, TOutput> VisitReplaceable<TInput, TOutput>(ReplaceableParser<TInput, TOutput> p);
        IParser<TInput, TOutput> VisitRightApplyZeroOrMore<TInput, TMiddle, TOutput>(RightApplyZeroOrMoreParser<TInput, TMiddle, TOutput> p);

        IParser<TInput, TOutput> VisitRule<TInput, TOutput>(RuleParser<TInput, TOutput> p);

        IParser<TInput, TOutput> VisitTransform<TInput, TMiddle, TOutput>(TransformParser<TInput, TMiddle, TOutput> p);
        IParser<TInput, TOutput> VisitTrie<TInput, TOutput>(TrieParser<TInput, TOutput> p);
    }
}
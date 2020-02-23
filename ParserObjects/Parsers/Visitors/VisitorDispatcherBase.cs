using System.Collections.Generic;
using ParserObjects.Parsers.Logical;

namespace ParserObjects.Parsers.Visitors
{
    public abstract class VisitorDispatcherBase : ICoreVisitorDispatcher, ILogicalVisitorDispatcher
    {

        public virtual IParser<TInput, TInput> VisitAny<TInput>(AnyParser<TInput> p) => p;

        public virtual IParser<TInput, TOutput> VisitDeferred<TInput, TOutput>(DeferredParser<TInput, TOutput> p) => p;

        public virtual IParser<TInput, object> VisitEmpty<TInput>(EmptyParser<TInput> p) => p;

        public virtual IParser<TInput, bool> VisitEnd<TInput>(EndParser<TInput> p) => p;

        public virtual IParser<TInput, TOutput> VisitFail<TInput, TOutput>(FailParser<TInput, TOutput> p) => p;

        public virtual IParser<TInput, TOutput> VisitFirst<TInput, TOutput>(FirstParser<TInput, TOutput> p) => p;

        public virtual IParser<TInput, TOutput> VisitFlatten<TInput, TCollection, TOutput>(FlattenParser<TInput, TCollection, TOutput> p)
            where TCollection : IEnumerable<TOutput>
            => p;

        public virtual IParser<TInput, TOutput> VisitLeftApplyZeroOrMore<TInput, TOutput>(LeftApplyZeroOrMoreParser<TInput, TOutput> p) => p;

        public virtual IParser<TInput, IEnumerable<TOutput>> VisitList<TInput, TOutput>(ListParser<TInput, TOutput> p) => p;

        public virtual IParser<TInput, TInput> VisitMatchPredicate<TInput>(MatchPredicateParser<TInput> p) => p;

        public virtual IParser<TInput, IReadOnlyList<TInput>> VisitMatchSequence<TInput>(MatchSequenceParser<TInput> p) => p;

        public virtual IParser<TInput, bool> VisitNegativeLookahead<TInput>(NegativeLookaheadParser<TInput> p) => p;

        public virtual IParser<TInput, bool> VisitPositiveLookahead<TInput>(PositiveLookaheadParser<TInput> p) => p;

        public virtual IParser<TInput, TOutput> VisitProduce<TInput, TOutput>(ProduceParser<TInput, TOutput> p) => p;

        public virtual IParser<TInput, TOutput> VisitReplaceable<TInput, TOutput>(ReplaceableParser<TInput, TOutput> p) => p;

        public virtual IParser<TInput, TOutput> VisitRightApplyZeroOrMore<TInput, TMiddle, TOutput>(RightApplyZeroOrMoreParser<TInput, TMiddle, TOutput> p) => p;

        public virtual IParser<TInput, TOutput> VisitRule<TInput, TOutput>(RuleParser<TInput, TOutput> p) => p;

        public virtual IParser<TInput, TOutput> VisitTransform<TInput, TMiddle, TOutput>(TransformParser<TInput, TMiddle, TOutput> p) => p;

        public virtual IParser<TInput, TOutput> VisitTrie<TInput, TOutput>(TrieParser<TInput, TOutput> p) => p;

        public virtual IParser<TInput, bool> VisitAnd<TInput>(AndParser<TInput> p) => p;

        public virtual IParser<TInput, TOutput> VisitIf<TInput, TOutput>(IfParser<TInput, TOutput> p) => p;

        public virtual IParser<TInput, bool> VisitNot<TInput>(NotParser<TInput> p) => p;

        public virtual IParser<TInput, bool> VisitOr<TInput>(OrParser<TInput> p) => p;
    }
}
namespace ParserObjects.Parsers.Logical
{
    public interface ILogicalVisitorDispatcher
    {
        IParser<TInput, bool> VisitAnd<TInput>(AndParser<TInput> p);
        IParser<TInput, TOutput> VisitIf<TInput, TOutput>(IfParser<TInput, TOutput> p);
        IParser<TInput, bool> VisitNot<TInput>(NotParser<TInput> p);
        IParser<TInput, bool> VisitOr<TInput>(OrParser<TInput> p);

    }
}
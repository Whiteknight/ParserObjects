namespace ParserObjects;

public delegate IParser<TInput, TOutput> GetParserFromParser<TInput, TOutput>(IParser<TInput, TOutput> p);

public delegate IParser<TInput, TOutput> GetParserFromParser<TInput, TMiddle, TOutput>(IParser<TInput, TMiddle> p);

public delegate IMultiParser<TInput, TOutput> GetMultiParserFromParser<TInput, TMiddle, TOutput>(IParser<TInput, TMiddle> p);

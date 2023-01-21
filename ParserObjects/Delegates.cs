namespace ParserObjects;

public delegate IParser<TInput, TOutput> GetParserFromParser<TInput, TOutput>(IParser<TInput, TOutput> p);

public delegate IParser<TInput, TOutput> GetParserFromParser<TInput, TMiddle, TOutput>(IParser<TInput, TMiddle> p);

public delegate IMultiParser<TInput, TOutput> GetMultiParserFromParser<TInput, TMiddle, TOutput>(IParser<TInput, TMiddle> p);

public delegate IMultiParser<TInput, TOutput> GetMultiParserFromMultiParser<TInput, TOutput>(IMultiParser<TInput, TOutput> p);

public delegate IParser<TInput, TOutput> GetParserFromResult<TInput, TOutput>(IResult result);

public delegate IParser<TInput, TOutput> GetParserFromResult<TInput, TMiddle, TOutput>(IResult<TMiddle> result);

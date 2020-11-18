namespace ParserObjects.Parsers
{
    public class ExamineParseState<TInput, TOutput>
    {
        public ExamineParseState(IParser<TInput, TOutput> parser, ISequence<TInput> input, IResult<TOutput> result)
        {
            Parser = parser;
            Input = input;
            Result = result;
        }

        public IParser<TInput, TOutput> Parser { get; }
        public ISequence<TInput> Input { get; }
        public IResult<TOutput> Result { get; }
    }
}

namespace ParserObjects.Parsers
{
    public static partial class Pratt<TInput, TOutput>
    {
        public delegate TOutput NudFunc<TValue>(IParseContext context, IToken<TValue> value);
        public delegate TOutput LedFunc<TValue>(IParseContext context, IToken<TOutput> left, IToken<TValue> value);
    }
}

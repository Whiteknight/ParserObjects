using ParserObjects.Parsers;

namespace ParserObjects
{
    public static partial class TupleExtensions
    {
        public static IParser<TInput, TOutput> First<TInput, TOutput>(this (IParser<TInput, TOutput>, IParser<TInput, TOutput>) parsers)
        {
            return new FirstParser<TInput, TOutput>(parsers.Item1, parsers.Item2);
        }

        public static IParser<TInput, TOutput> First<TInput, TOutput>(this (IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>) parsers)
        {
            return new FirstParser<TInput, TOutput>(parsers.Item1, parsers.Item2, parsers.Item3);
        }

        public static IParser<TInput, TOutput> First<TInput, TOutput>(this (IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>) parsers)
        {
            return new FirstParser<TInput, TOutput>(parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4);
        }

        public static IParser<TInput, TOutput> First<TInput, TOutput>(this (IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>) parsers)
        {
            return new FirstParser<TInput, TOutput>(parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5);
        }

        public static IParser<TInput, TOutput> First<TInput, TOutput>(this (IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>) parsers)
        {
            return new FirstParser<TInput, TOutput>(parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6);
        }

        public static IParser<TInput, TOutput> First<TInput, TOutput>(this (IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>) parsers)
        {
            return new FirstParser<TInput, TOutput>(parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6, parsers.Item7);
        }

        public static IParser<TInput, TOutput> First<TInput, TOutput>(this (IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>) parsers)
        {
            return new FirstParser<TInput, TOutput>(parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6, parsers.Item7, parsers.Item8);
        }

        public static IParser<TInput, TOutput> First<TInput, TOutput>(this (IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>, IParser<TInput, TOutput>) parsers)
        {
            return new FirstParser<TInput, TOutput>(parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6, parsers.Item7, parsers.Item8, parsers.Item9);
        }
    }
}

﻿using System;
using ParserObjects.Parsers;

namespace ParserObjects
{
    public static partial class TupleExtensions
    {
        public static IParser<TInput, TOutput> Produce<TInput, T1, T2, TOutput>(this (IParser<TInput, T1>, IParser<TInput, T2>) parsers, Func<T1, T2, TOutput> produce)
        {
            return new RuleParser<TInput, TOutput>(
                new IParser<TInput>[] { parsers.Item1, parsers.Item2 },
                list => produce((T1)list[0], (T2)list[1]));
        }

        public static IParser<TInput, TOutput> Produce<TInput, T1, T2, T3, TOutput>(this (IParser<TInput, T1>, IParser<TInput, T2>, IParser<TInput, T3>) parsers, Func<T1, T2, T3, TOutput> produce)
        {
            return new RuleParser<TInput, TOutput>(
                new IParser<TInput>[] { parsers.Item1, parsers.Item2, parsers.Item3 },
                list => produce((T1)list[0], (T2)list[1], (T3)list[2]));
        }

        public static IParser<TInput, TOutput> Produce<TInput, T1, T2, T3, T4, TOutput>(this (IParser<TInput, T1>, IParser<TInput, T2>, IParser<TInput, T3>, IParser<TInput, T4>) parsers, Func<T1, T2, T3, T4, TOutput> produce)
        {
            return new RuleParser<TInput, TOutput>(
                new IParser<TInput>[] { parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4 },
                list => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3]));
        }

        public static IParser<TInput, TOutput> Produce<TInput, T1, T2, T3, T4, T5, TOutput>(this (IParser<TInput, T1>, IParser<TInput, T2>, IParser<TInput, T3>, IParser<TInput, T4>, IParser<TInput, T5>) parsers, Func<T1, T2, T3, T4, T5, TOutput> produce)
        {
            return new RuleParser<TInput, TOutput>(
                new IParser<TInput>[] { parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5 },
                list => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4]));
        }

        public static IParser<TInput, TOutput> Produce<TInput, T1, T2, T3, T4, T5, T6, TOutput>(this (IParser<TInput, T1>, IParser<TInput, T2>, IParser<TInput, T3>, IParser<TInput, T4>, IParser<TInput, T5>, IParser<TInput, T6>) parsers, Func<T1, T2, T3, T4, T5, T6, TOutput> produce)
        {
            return new RuleParser<TInput, TOutput>(
                new IParser<TInput>[] { parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6 },
                list => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4], (T6)list[5]));
        }

        public static IParser<TInput, TOutput> Produce<TInput, T1, T2, T3, T4, T5, T6, T7, TOutput>(this (IParser<TInput, T1>, IParser<TInput, T2>, IParser<TInput, T3>, IParser<TInput, T4>, IParser<TInput, T5>, IParser<TInput, T6>, IParser<TInput, T7>) parsers, Func<T1, T2, T3, T4, T5, T6, T7, TOutput> produce)
        {
            return new RuleParser<TInput, TOutput>(
                new IParser<TInput>[] { parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6, parsers.Item7 },
                list => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4], (T6)list[5], (T7)list[6]));
        }

        public static IParser<TInput, TOutput> Produce<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TOutput>(this (IParser<TInput, T1>, IParser<TInput, T2>, IParser<TInput, T3>, IParser<TInput, T4>, IParser<TInput, T5>, IParser<TInput, T6>, IParser<TInput, T7>, IParser<TInput, T8>) parsers, Func<T1, T2, T3, T4, T5, T6, T7, T8, TOutput> produce)
        {
            return new RuleParser<TInput, TOutput>(
                new IParser<TInput>[] { parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6, parsers.Item7, parsers.Item8 },
                list => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4], (T6)list[5], (T7)list[6], (T8)list[7]));
        }

        public static IParser<TInput, TOutput> Produce<TInput, T1, T2, T3, T4, T5, T6, T7, T8, T9, TOutput>(this (IParser<TInput, T1>, IParser<TInput, T2>, IParser<TInput, T3>, IParser<TInput, T4>, IParser<TInput, T5>, IParser<TInput, T6>, IParser<TInput, T7>, IParser<TInput, T8>, IParser<TInput, T9>) parsers, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOutput> produce)
        {
            return new RuleParser<TInput, TOutput>(
                new IParser<TInput>[] { parsers.Item1, parsers.Item2, parsers.Item3, parsers.Item4, parsers.Item5, parsers.Item6, parsers.Item7, parsers.Item8, parsers.Item9 },
                list => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4], (T6)list[5], (T7)list[6], (T8)list[7], (T9)list[8]));
        }
    }
}
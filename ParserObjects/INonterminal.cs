using System;
using System.Collections.Generic;
using ParserObjects.Earley;

namespace ParserObjects
{
    public interface INonterminal : ISymbol
    {
        IReadOnlyCollection<IProduction> Productions { get; }

        void AddProductionObj(IProduction p);

        bool Contains(IProduction p);
    }

    public interface INonterminal<TInput> : INonterminal
    {
    }

    public interface INonterminal<TInput, TOutput> : ISymbol<TOutput>, INonterminal<TInput>
    {
    }

    public static class NonterminalExtensions
    {
        public static INonterminal<TInput, TOutput> AddProduction<TInput, TOutput>(this INonterminal<TInput, TOutput> lhs, ISymbol<TOutput> s1)
            => lhs.AddProduction(s1, v => v);

        public static INonterminal<TInput, TOutput> AddProduction<TInput, TOutput, T1>(this INonterminal<TInput, TOutput> lhs, ISymbol<T1> s1, Func<T1, TOutput> func)
        {
            var p = new Production<TOutput>(lhs, args => func((T1)args[0]), s1);
            lhs.AddProductionObj(p);
            return lhs;
        }

        public static INonterminal<TInput, TOutput> AddProduction<TInput, TOutput, T1, T2>(this INonterminal<TInput, TOutput> lhs, ISymbol<T1> s1, ISymbol<T2> s2, Func<T1, T2, TOutput> func)
        {
            var p = new Production<TOutput>(lhs, args => func((T1)args[0], (T2)args[1]), s1, s2);
            lhs.AddProductionObj(p);
            return lhs;
        }

        public static INonterminal<TInput, TOutput> AddProduction<TInput, TOutput, T1, T2, T3>(this INonterminal<TInput, TOutput> lhs, ISymbol<T1> s1, ISymbol<T2> s2, ISymbol<T3> s3, Func<T1, T2, T3, TOutput> func)
        {
            var p = new Production<TOutput>(lhs, args => func((T1)args[0], (T2)args[1], (T3)args[2]), s1, s2, s3);
            lhs.AddProductionObj(p);
            return lhs;
        }

        public static INonterminal<TInput, TOutput> AddProduction<TInput, TOutput, T1, T2, T3, T4>(this INonterminal<TInput, TOutput> lhs, ISymbol<T1> s1, ISymbol<T2> s2, ISymbol<T3> s3, ISymbol<T4> s4, Func<T1, T2, T3, T4, TOutput> func)
        {
            var p = new Production<TOutput>(lhs, args => func((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3]), s1, s2, s3, s4);
            lhs.AddProductionObj(p);
            return lhs;
        }

        public static INonterminal<TInput, TOutput> AddProduction<TInput, TOutput, T1, T2, T3, T4, T5>(this INonterminal<TInput, TOutput> lhs, ISymbol<T1> s1, ISymbol<T2> s2, ISymbol<T3> s3, ISymbol<T4> s4, ISymbol<T5> s5, Func<T1, T2, T3, T4, T5, TOutput> func)
        {
            var p = new Production<TOutput>(lhs, args => func((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4]), s1, s2, s3, s4, s5);
            lhs.AddProductionObj(p);
            return lhs;
        }

        public static INonterminal<TInput, TOutput> AddProduction<TInput, TOutput, T1, T2, T3, T4, T5, T6>(this INonterminal<TInput, TOutput> lhs, ISymbol<T1> s1, ISymbol<T2> s2, ISymbol<T3> s3, ISymbol<T4> s4, ISymbol<T5> s5, ISymbol<T6> s6, Func<T1, T2, T3, T4, T5, T6, TOutput> func)
        {
            var p = new Production<TOutput>(lhs, args => func((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5]), s1, s2, s3, s4, s5, s6);
            lhs.AddProductionObj(p);
            return lhs;
        }

        public static INonterminal<TInput, TOutput> AddProduction<TInput, TOutput, T1, T2, T3, T4, T5, T6, T7>(this INonterminal<TInput, TOutput> lhs, ISymbol<T1> s1, ISymbol<T2> s2, ISymbol<T3> s3, ISymbol<T4> s4, ISymbol<T5> s5, ISymbol<T6> s6, ISymbol<T7> s7, Func<T1, T2, T3, T4, T5, T6, T7, TOutput> func)
        {
            var p = new Production<TOutput>(lhs, args => func((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6]), s1, s2, s3, s4, s5, s6, s7);
            lhs.AddProductionObj(p);
            return lhs;
        }

        public static INonterminal<TInput, TOutput> AddProduction<TInput, TOutput, T1, T2, T3, T4, T5, T6, T7, T8>(this INonterminal<TInput, TOutput> lhs, ISymbol<T1> s1, ISymbol<T2> s2, ISymbol<T3> s3, ISymbol<T4> s4, ISymbol<T5> s5, ISymbol<T6> s6, ISymbol<T7> s7, ISymbol<T8> s8, Func<T1, T2, T3, T4, T5, T6, T7, T8, TOutput> func)
        {
            var p = new Production<TOutput>(lhs, args => func((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6], (T8)args[7]), s1, s2, s3, s4, s5, s6, s7, s8);
            lhs.AddProductionObj(p);
            return lhs;
        }

        public static INonterminal<TInput, TOutput> AddProduction<TInput, TOutput, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this INonterminal<TInput, TOutput> lhs, ISymbol<T1> s1, ISymbol<T2> s2, ISymbol<T3> s3, ISymbol<T4> s4, ISymbol<T5> s5, ISymbol<T6> s6, ISymbol<T7> s7, ISymbol<T8> s8, ISymbol<T9> s9, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOutput> func)
        {
            var p = new Production<TOutput>(lhs, args => func((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6], (T8)args[7], (T9)args[8]), s1, s2, s3, s4, s5, s6, s7, s8, s9);
            lhs.AddProductionObj(p);
            return lhs;
        }
    }
}

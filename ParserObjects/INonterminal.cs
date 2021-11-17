using System;
using System.Collections.Generic;
using ParserObjects.Earley;

namespace ParserObjects
{
    /// <summary>
    /// A symbol which references sequences of other symbols.
    /// </summary>
    public interface INonterminal : ISymbol
    {
        /// <summary>
        /// Gets a list of possible production rules for this symbol.
        /// </summary>
        IReadOnlyCollection<IProduction> Productions { get; }

        /// <summary>
        /// Add a new production rule to this symbol.
        /// </summary>
        /// <param name="p"></param>
        void Add(IProduction p);

        /// <summary>
        /// Returns true if this nonterminal already contains the given production rule.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        bool Contains(IProduction p);
    }

    /// <summary>
    /// A symbol which references sequences of other symbols, all of which have the same output
    /// type.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public interface INonterminal<TInput, out TOutput> : ISymbol<TOutput>, INonterminal
    {
    }

    public static class NonterminalExtensions
    {
        /// <summary>
        /// Add a new production rule as a sequence of the given symbols.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="lhs"></param>
        /// <param name="s1"></param>
        /// <returns></returns>
        public static INonterminal<TInput, TOutput> Rule<TInput, TOutput>(this INonterminal<TInput, TOutput> lhs, ISymbol<TOutput> s1)
            => lhs.Rule(s1, v => v);

        /// <summary>
        /// Add a new production rule as a sequence of the given symbols.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="lhs"></param>
        /// <param name="s1"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static INonterminal<TInput, TOutput> Rule<TInput, TOutput, T1>(this INonterminal<TInput, TOutput> lhs, ISymbol<T1> s1, Func<T1, TOutput> func)
        {
            var p = new Production<TOutput>(lhs, args => func((T1)args[0]), s1);
            lhs.Add(p);
            return lhs;
        }

        /// <summary>
        /// Add a new production rule as a sequence of the given symbols.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="lhs"></param>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static INonterminal<TInput, TOutput> Rule<TInput, TOutput, T1, T2>(this INonterminal<TInput, TOutput> lhs, ISymbol<T1> s1, ISymbol<T2> s2, Func<T1, T2, TOutput> func)
        {
            var p = new Production<TOutput>(lhs, args => func((T1)args[0], (T2)args[1]), s1, s2);
            lhs.Add(p);
            return lhs;
        }

        /// <summary>
        /// Add a new production rule as a sequence of the given symbols.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="lhs"></param>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="s3"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static INonterminal<TInput, TOutput> Rule<TInput, TOutput, T1, T2, T3>(this INonterminal<TInput, TOutput> lhs, ISymbol<T1> s1, ISymbol<T2> s2, ISymbol<T3> s3, Func<T1, T2, T3, TOutput> func)
        {
            var p = new Production<TOutput>(lhs, args => func((T1)args[0], (T2)args[1], (T3)args[2]), s1, s2, s3);
            lhs.Add(p);
            return lhs;
        }

        /// <summary>
        /// Add a new production rule as a sequence of the given symbols.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="lhs"></param>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="s3"></param>
        /// <param name="s4"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static INonterminal<TInput, TOutput> Rule<TInput, TOutput, T1, T2, T3, T4>(this INonterminal<TInput, TOutput> lhs, ISymbol<T1> s1, ISymbol<T2> s2, ISymbol<T3> s3, ISymbol<T4> s4, Func<T1, T2, T3, T4, TOutput> func)
        {
            var p = new Production<TOutput>(lhs, args => func((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3]), s1, s2, s3, s4);
            lhs.Add(p);
            return lhs;
        }

        /// <summary>
        /// Add a new production rule as a sequence of the given symbols.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="lhs"></param>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="s3"></param>
        /// <param name="s4"></param>
        /// <param name="s5"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static INonterminal<TInput, TOutput> Rule<TInput, TOutput, T1, T2, T3, T4, T5>(this INonterminal<TInput, TOutput> lhs, ISymbol<T1> s1, ISymbol<T2> s2, ISymbol<T3> s3, ISymbol<T4> s4, ISymbol<T5> s5, Func<T1, T2, T3, T4, T5, TOutput> func)
        {
            var p = new Production<TOutput>(lhs, args => func((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4]), s1, s2, s3, s4, s5);
            lhs.Add(p);
            return lhs;
        }

        /// <summary>
        /// Add a new production rule as a sequence of the given symbols.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="lhs"></param>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="s3"></param>
        /// <param name="s4"></param>
        /// <param name="s5"></param>
        /// <param name="s6"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static INonterminal<TInput, TOutput> Rule<TInput, TOutput, T1, T2, T3, T4, T5, T6>(this INonterminal<TInput, TOutput> lhs, ISymbol<T1> s1, ISymbol<T2> s2, ISymbol<T3> s3, ISymbol<T4> s4, ISymbol<T5> s5, ISymbol<T6> s6, Func<T1, T2, T3, T4, T5, T6, TOutput> func)
        {
            var p = new Production<TOutput>(lhs, args => func((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5]), s1, s2, s3, s4, s5, s6);
            lhs.Add(p);
            return lhs;
        }

        /// <summary>
        /// Add a new production rule as a sequence of the given symbols.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="lhs"></param>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="s3"></param>
        /// <param name="s4"></param>
        /// <param name="s5"></param>
        /// <param name="s6"></param>
        /// <param name="s7"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static INonterminal<TInput, TOutput> Rule<TInput, TOutput, T1, T2, T3, T4, T5, T6, T7>(this INonterminal<TInput, TOutput> lhs, ISymbol<T1> s1, ISymbol<T2> s2, ISymbol<T3> s3, ISymbol<T4> s4, ISymbol<T5> s5, ISymbol<T6> s6, ISymbol<T7> s7, Func<T1, T2, T3, T4, T5, T6, T7, TOutput> func)
        {
            var p = new Production<TOutput>(lhs, args => func((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6]), s1, s2, s3, s4, s5, s6, s7);
            lhs.Add(p);
            return lhs;
        }

        /// <summary>
        /// Add a new production rule as a sequence of the given symbols.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="lhs"></param>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="s3"></param>
        /// <param name="s4"></param>
        /// <param name="s5"></param>
        /// <param name="s6"></param>
        /// <param name="s7"></param>
        /// <param name="s8"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static INonterminal<TInput, TOutput> Rule<TInput, TOutput, T1, T2, T3, T4, T5, T6, T7, T8>(this INonterminal<TInput, TOutput> lhs, ISymbol<T1> s1, ISymbol<T2> s2, ISymbol<T3> s3, ISymbol<T4> s4, ISymbol<T5> s5, ISymbol<T6> s6, ISymbol<T7> s7, ISymbol<T8> s8, Func<T1, T2, T3, T4, T5, T6, T7, T8, TOutput> func)
        {
            var p = new Production<TOutput>(lhs, args => func((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6], (T8)args[7]), s1, s2, s3, s4, s5, s6, s7, s8);
            lhs.Add(p);
            return lhs;
        }

        /// <summary>
        /// Add a new production rule as a sequence of the given symbols.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="lhs"></param>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="s3"></param>
        /// <param name="s4"></param>
        /// <param name="s5"></param>
        /// <param name="s6"></param>
        /// <param name="s7"></param>
        /// <param name="s8"></param>
        /// <param name="s9"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static INonterminal<TInput, TOutput> Rule<TInput, TOutput, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this INonterminal<TInput, TOutput> lhs, ISymbol<T1> s1, ISymbol<T2> s2, ISymbol<T3> s3, ISymbol<T4> s4, ISymbol<T5> s5, ISymbol<T6> s6, ISymbol<T7> s7, ISymbol<T8> s8, ISymbol<T9> s9, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOutput> func)
        {
            var p = new Production<TOutput>(lhs, args => func((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6], (T8)args[7], (T9)args[8]), s1, s2, s3, s4, s5, s6, s7, s8, s9);
            lhs.Add(p);
            return lhs;
        }
    }
}

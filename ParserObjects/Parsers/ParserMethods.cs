using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Parser methods for building combinators using declarative syntax
    /// </summary>
    public static class ParserMethods
    {
        /// <summary>
        /// Matches anywhere in the sequence except at the end, and consumes 1 token of input
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IParser<T, T> Any<T>() => new AnyParser<T>();

        /// <summary>
        /// Get a reference to a parser dynamically. Avoids circular dependencies in the grammar
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="getParser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Deferred<TInput, TOutput>(Func<IParser<TInput, TOutput>> getParser) 
            => new DeferredParser<TInput, TOutput>(getParser);

        /// <summary>
        /// Matches affirmatively at the end of the input, fails everywhere else.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <returns></returns>
        public static IParser<TInput, object> End<TInput>()
            => new EndParser<TInput>();

        /// <summary>
        /// Return the result of the first parser which succeeds
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="parsers"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> First<TInput, TOutput>(params IParser<TInput, TOutput>[] parsers) 
            => new FirstParser<TInput, TOutput>(parsers);

        /// <summary>
        /// Flattens the result of a parser 
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TCollection"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Flatten<TInput, TCollection, TOutput>(IParser<TInput, TCollection> parser)
            where TCollection : IEnumerable<TOutput>
            => new FlattenParser<TInput, TCollection, TOutput>(parser);

        /// <summary>
        /// Parse a list of zero or more items.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="p"></param>
        /// <param name="atLeastOne"></param>
        /// <returns></returns>
        public static IParser<TInput, IEnumerable<TOutput>> List<TInput, TOutput>(IParser<TInput, TOutput> p, bool atLeastOne = false) 
            => new ListParser<TInput, TOutput>(p, atLeastOne);

        /// <summary>
        /// Test the next input value and return it, if it matches the predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IParser<T, T> Match<T>(Func<T, bool> predicate) 
            => new PredicateParser<T>(predicate);

        /// <summary>
        /// Get the next input value and return it if it is equal to the given value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static IParser<T, T> Match<T>(T pattern)
            => Match<T>(s => s.Equals(pattern));

        /// <summary>
        /// Get the next few input values and compare them one-by-one against an ordered sequence of test
        /// values. If every value in the sequence matches, return the sequence as a list.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static IParser<TInput, IReadOnlyList<TInput>> Match<TInput>(IEnumerable<TInput> pattern)
            => new MatchSequenceParser<TInput>(pattern);

        /// <summary>
        /// Zero-length assertion that the given pattern does not match from the current position. No
        /// input is consumed
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TMatch"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public static IParser<TInput, bool> NegativeLookahead<TInput, TMatch>(IParser<TInput, TMatch> p)
            => new NegativeLookaheadParser<TInput, TMatch>(p);

        /// <summary>
        /// Attempt to parse an item and return a default value otherwise
        /// </summary>
        /// <param name="p"></param>
        /// <param name="getDefault"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Optional<TInput, TOutput>(IParser<TInput, TOutput> p, Func<TOutput> getDefault = null) 
            => new OptionalParser<TInput, TOutput>(p, getDefault);

        /// <summary>
        /// Zero-length assertion that the given pattern matches from the current position. No input is
        /// consumed.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TMatch"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public static IParser<TInput, bool> PositiveLookahead<TInput, TMatch>(IParser<TInput, TMatch> p)
            => new PositiveLookaheadParser<TInput, TMatch>(p);

        /// <summary>
        /// Produce an empty or default node without consuming anything out of the tokenizer
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Produce<TInput, TOutput>(Func<TOutput> produce) 
            => new ProduceParser<TInput, TOutput>(t => produce());

        /// <summary>
        /// Unconditionally produce a value given the input sequence at it's current location
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Produce<TInput, TOutput>(Func<ISequence<TInput>, TOutput> produce) 
            => new ProduceParser<TInput, TOutput>(produce);

        /// <summary>
        /// Serves as a placeholder in the parser tree where a replacement can be made without rewriting
        /// the whole tree
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="defaultParser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Replaceable<TInput, TOutput>(IParser<TInput, TOutput> defaultParser = null) 
            => new ReplaceableParser<TInput, TOutput>(defaultParser ?? new FailParser<TInput, TOutput>());

        /// <summary>
        /// Parse a required item. If the parse fails, produce a default version and attach a
        /// diagnostic error message
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="p"></param>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Required<TInput, TOutput>(IParser<TInput, TOutput> p, Func<ISequence<TInput>, TOutput> produce) 
            => new RequiredParser<TInput, TOutput>(p, produce);

        /// <summary>
        /// Parse a list of items separated by some separator
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TSeparator"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="p"></param>
        /// <param name="separator"></param>
        /// <param name="produce"></param>
        /// <param name="atLeastOne"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> SeparatedList<TInput, TItem, TSeparator, TOutput>(IParser<TInput, TItem> p, IParser<TInput, TSeparator> separator, Func<IReadOnlyList<TItem>, TOutput> produce, bool atLeastOne = false)
        {
            if (atLeastOne)
            {
                return Rule(
                    p,
                    List(
                        Rule(
                            separator,
                            p,
                            (s, item) => item
                        )
                    ),
                    (first, rest) => produce(new[] { first }.Concat(rest).ToList())
                );
            }

            return First(
                Rule(
                    p,
                    List(
                        Rule(
                            separator,
                            p,
                            (s, item) => item
                        )
                    ),
                    (first, rest) => produce(new[] { first }.Concat(rest).ToList())
                ),
                Produce<TInput, TOutput>(() => produce(new List<TItem>()))
            );
        }

        /// <summary>
        /// Parse a sequence of productions and reduce them into a single output. If any item fails, rollback all and
        /// return failure
        /// </summary>
        /// <typeparam name="TInput, T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Rule<TInput, T1, T2, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, Func<T1, T2, TOutput> produce)
        {
            return new RuleParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2 },
                (list) => produce((T1)list[0], (T2)list[1]));
        }

        /// <summary>
        /// Parse a sequence of productions and reduce them into a single output. If any item fails, rollback all and
        /// return failure
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, Func<T1, T2, T3, TOutput> produce)
        {
            return new RuleParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2]));
        }

        /// <summary>
        /// Parse a sequence of productions and reduce them into a single output. If any item fails, rollback all and
        /// return failure
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, T4, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, IParser<TInput, T4> p4, Func<T1, T2, T3, T4, TOutput> produce)
        {
            return new RuleParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3, p4 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3]));
        }

        /// <summary>
        /// Parse a sequence of productions and reduce them into a single output. If any item fails, rollback all and
        /// return failure
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, T4, T5, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, IParser<TInput, T4> p4, IParser<TInput, T5> p5, Func<T1, T2, T3, T4, T5, TOutput> produce)
        {
            return new RuleParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3, p4, p5 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4]));
        }

        /// <summary>
        /// Parse a sequence of productions and reduce them into a single output. If any item fails, rollback all and
        /// return failure
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="p6"></param>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, T4, T5, T6, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, IParser<TInput, T4> p4, IParser<TInput, T5> p5, IParser<TInput, T6> p6, Func<T1, T2, T3, T4, T5, T6, TOutput> produce)
        {
            return new RuleParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3, p4, p5, p6 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4], (T6)list[5]));
        }

        /// <summary>
        /// Parse a sequence of productions and reduce them into a single output. If any item fails, rollback all and
        /// return failure
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="p6"></param>
        /// <param name="p7"></param>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, T4, T5, T6, T7, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, IParser<TInput, T4> p4, IParser<TInput, T5> p5, IParser<TInput, T6> p6, IParser<TInput, T7> p7, Func<T1, T2, T3, T4, T5, T6, T7, TOutput> produce)
        {
            return new RuleParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3, p4, p5, p6, p7 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4], (T6)list[5], (T7)list[6]));
        }

        /// <summary>
        /// Parse a sequence of productions and reduce them into a single output. If any item fails, rollback all and
        /// return failure
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="p6"></param>
        /// <param name="p7"></param>
        /// <param name="p8"></param>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, IParser<TInput, T4> p4, IParser<TInput, T5> p5, IParser<TInput, T6> p6, IParser<TInput, T7> p7, IParser<TInput, T8> p8, Func<T1, T2, T3, T4, T5, T6, T7, T8, TOutput> produce)
        {
            return new RuleParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3, p4, p5, p6, p7, p8 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4], (T6)list[5], (T7)list[6], (T8)list[7]));
        }

        /// <summary>
        /// Parse a sequence of productions and reduce them into a single output. If any item fails, rollback all and
        /// return failure
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="p6"></param>
        /// <param name="p7"></param>
        /// <param name="p8"></param>
        /// <param name="p9"></param>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Rule<TInput, T1, T2, T3, T4, T5, T6, T7, T8, T9, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, IParser<TInput, T4> p4, IParser<TInput, T5> p5, IParser<TInput, T6> p6, IParser<TInput, T7> p7, IParser<TInput, T8> p8, IParser<TInput, T9> p9, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOutput> produce)
        {
            return new RuleParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3, p4, p5, p6, p7, p8, p9 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4], (T6)list[5], (T7)list[6], (T8)list[7], (T9)list[8]));
        }

        public static IParser<char, string> StringTrie(IEnumerable<string> patterns)
        {
            var trie = new InsertOnlyTrie<char, string>();
            foreach (var pattern in patterns)
                trie.Add(pattern, pattern);
            return new TrieParser<char, string>(trie);
        }

        /// <summary>
        /// Transform one node into another node to fit into the grammar
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TMiddle"></typeparam>
        /// <param name="parser"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Transform<TInput, TMiddle, TOutput>(IParser<TInput, TMiddle> parser, Func<TMiddle, TOutput> transform) 
            => new TransformParser<TInput, TMiddle, TOutput>(parser, transform);

        /// <summary>
        /// Look up sequences of inputs in an ITrie to greedily find the longest matching sequence
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="trie"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Trie<TInput, TOutput>(ITrie<TInput, TOutput> trie)
            => new TrieParser<TInput, TOutput>(trie);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Pratt
{
    /// <summary>
    /// Configuration class for setting up a parselet.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class ParseletConfiguration<TInput, TValue, TOutput> : IParseletConfiguration<TInput, TValue, TOutput>
    {
        private readonly List<Func<IParser<TInput, TValue>, int, string, IParselet<TInput, TOutput>>> _getParselets;
        private readonly IParser<TInput, TValue> _matcher;

        private int _typeId;

        public ParseletConfiguration(IParser<TInput, TValue> matcher)
        {
            Assert.ArgumentNotNull(matcher, nameof(matcher));
            _matcher = matcher;
            _getParselets = new List<Func<IParser<TInput, TValue>, int, string, IParselet<TInput, TOutput>>>();
            Name = string.Empty;
        }

        public string Name { get; set; }

        public IEnumerable<IParselet<TInput, TOutput>> Build()
        {
            var parselets = _getParselets.Select(f => f(_matcher, _typeId, Name)).ToList();
            _getParselets.Clear();
            return parselets;
        }

        public IParseletConfiguration<TInput, TValue, TOutput> TypeId(int id)
        {
            _typeId = id;
            return this;
        }

        public IParseletConfiguration<TInput, TValue, TOutput> ProduceRight(NudFunc<TInput, TValue, TOutput> getNud)
            => ProduceRight(0, getNud);

        public IParseletConfiguration<TInput, TValue, TOutput> ProduceRight(int rbp, NudFunc<TInput, TValue, TOutput> getNud)
        {
            Assert.ArgumentNotNull(getNud, nameof(getNud));
            _getParselets.Add((m, tid, n) => new Parselet<TInput, TValue, TOutput>(
                tid,
                m,
                getNud,
                null,
                rbp,
                rbp,
                n
            ));
            return this;
        }

        public IParseletConfiguration<TInput, TValue, TOutput> ProduceLeft(int lbp, LedFunc<TInput, TValue, TOutput> getLed)
            => ProduceLeft(lbp, lbp + 1, getLed);

        public IParseletConfiguration<TInput, TValue, TOutput> ProduceLeft(int lbp, int rbp, LedFunc<TInput, TValue, TOutput> getLed)
        {
            Assert.ArgumentNotNull(getLed, nameof(getLed));
            _getParselets.Add((m, tid, n) => new Parselet<TInput, TValue, TOutput>(
                tid,
                m,
                null,
                getLed,
                lbp,
                rbp,
                n
            ));
            return this;
        }
    }
}

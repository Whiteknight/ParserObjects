﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Pratt
{
    public class Configuration<TInput, TOutput> : IConfiguration<TInput, TOutput>
    {
        private readonly List<IParser> _references;

        public Configuration()
        {
            Parselets = new List<IParselet<TInput, TOutput>>();
            _references = new List<IParser>();
        }

        public List<IParselet<TInput, TOutput>> Parselets { get; }

        public IConfiguration<TInput, TOutput> Add<TValue>(IParser<TInput, TValue> matcher, Action<IParseletConfiguration<TInput, TValue, TOutput>> setup)
        {
            var parseletConfig = new ParseletConfiguration<TInput, TValue, TOutput>(matcher);
            setup(parseletConfig);
            var parselets = parseletConfig.Build();
            Parselets.AddRange(parselets);
            return this;
        }

        public IConfiguration<TInput, TOutput> Add(IParser<TInput, TOutput> matcher)
            => Add(matcher, p => p.ProduceRight(0, (ctx, v) => v.Value));

        public IConfiguration<TInput, TOutput> Reference(IParser parser)
        {
            _references.Add(parser);
            return this;
        }

        public IEnumerable<IParser> GetParsers() => Parselets.Select(p => p.Parser).Concat(_references);
    }
}

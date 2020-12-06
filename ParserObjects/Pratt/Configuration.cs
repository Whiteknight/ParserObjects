using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Pratt
{
    public class Configuration<TInput, TOutput> : IConfiguration<TInput, TOutput>
    {
        public Configuration()
        {
            Parselets = new List<IParselet<TInput, TOutput>>();
        }

        public List<IParselet<TInput, TOutput>> Parselets { get; }

        public IConfiguration<TInput, TOutput> Add<TValue>(IParser<TInput, TValue> matcher, Action<IParseletConfiguration<TInput, TValue, TOutput>> setup)
        {
            var parseletConfig = new ParseletConfiguration<TInput, TValue, TOutput>(matcher);
            setup(parseletConfig);
            var parselet = parseletConfig.Build();
            Parselets.Add(parselet);
            return this;
        }

        public IConfiguration<TInput, TOutput> Add(IParser<TInput, TOutput> matcher) => Add(matcher, p => p.ProduceRight((ctx, v) => v.Value));

        public IEnumerable<IParser> GetParsers() => Parselets.Select(p => p.Parser);
    }
}

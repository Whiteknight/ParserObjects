﻿using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    public class LimitedListParser<TInput, TOutput> : IParser<TInput, IEnumerable<TOutput>>
    {
        private readonly IParser<TInput, TOutput> _parser;

        public LimitedListParser(IParser<TInput, TOutput> parser, int minimum, int? maximum)
        {
            Minimum = minimum < 0 ? 0 : minimum;
            Maximum = maximum;
            if (Maximum.HasValue)
            {
                if (Maximum < Minimum)
                    Maximum = Minimum;
            }
            Assert.ArgumentNotNull(parser, nameof(parser));
            _parser = parser;
        }

        public IParseResult<IEnumerable<TOutput>> Parse(ISequence<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            var window = t.Window();
            var location = t.CurrentLocation;
            var items = new List<TOutput>();
            while (Maximum == null || items.Count < Maximum)
            {
                var result = _parser.Parse(window);
                if (!result.Success)
                    break;
                items.Add(result.Value);
            }

            if (items.Count < Minimum)
            {
                window.Rewind();
                return new FailResult<IEnumerable<TOutput>>(location);
            }

            return new SuccessResult<IEnumerable<TOutput>>(items, location);
        }

        public int Minimum { get; }
        public int? Maximum { get; }

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => new[] { _parser };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_parser == find && replace is IParser<TInput, TOutput> realReplace)
                return new LimitedListParser<TInput, TOutput>(realReplace, Minimum, Maximum);
            return this;
        }

        public override string ToString()
        {
            var typeName = GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}
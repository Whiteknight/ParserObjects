using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    public class CreateParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly Func<ParseState<TInput>, IParser<TInput, TOutput>> _getParser;

        public CreateParser(Func<ParseState<TInput>, IParser<TInput, TOutput>> getParser)
        {
            Assert.ArgumentNotNull(getParser, nameof(getParser));
            _getParser = getParser;
        }

        public string Name { get; set; }

        public Result<TOutput> Parse(ParseState<TInput> t)
        {
            var parser = _getParser(t);
            if (parser == null)
                throw new InvalidOperationException("Create parser value must not be null");
            return parser.Parse(t);
        }

        Result<object> IParser<TInput>.ParseUntyped(ParseState<TInput> t) => Parse(t).Untype();

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}

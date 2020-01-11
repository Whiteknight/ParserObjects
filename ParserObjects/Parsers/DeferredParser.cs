using System;
using System.Collections.Generic;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Looks up a parser at Parse() time, to avoid circular references in the grammar
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    public class DeferredParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly Func<IParser<TInput, TOutput>> _getParser;

        public DeferredParser(Func<IParser<TInput, TOutput>> getParser)
        {
            _getParser = getParser;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t) => _getParser().Parse(t);

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) 
            => _getParser().Parse(t).Untype();

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => new IParser[] { _getParser() };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _getParser() && replace is IParser<TInput, TOutput> realReplace)
                return realReplace;
            return this;
        }

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Takes a list of parsers and attempts each one in order. Returns as soon as the first parser
    /// succeeds
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    public class FirstParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IReadOnlyList<IParser<TInput, TOutput>> _parsers;

        public FirstParser(params IParser<TInput, TOutput>[] parsers)
        {
            _parsers = parsers;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            foreach (var parser in _parsers)
            {
                var result = parser.Parse(t);
                if (result.Success)
                    return result;
            }

            return new FailResult<TOutput>(t.CurrentLocation);
        }

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => _parsers;

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (!_parsers.Contains(find) || !(replace is IParser<TInput, TOutput> realReplace))
                return this;
            var newList = new IParser<TInput, TOutput>[_parsers.Count];
            for (int i = 0; i < _parsers.Count; i++)
            {
                var child = _parsers[i];
                newList[i] = child == find ? realReplace : child;
            }

            return new FirstParser<TInput, TOutput>(newList);
        }

        public override string ToString()
        {
            var typeName = GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Returns unconditional failure
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class FailParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        public FailParser(string errorMessage = null)
        {
            ErrorMessage = errorMessage ?? "Guaranteed fail";
        }

        public string Name { get; set; }

        public string ErrorMessage { get; }

        public Result<TOutput> Parse(ParseState<TInput> t)
            => t.Fail(this, ErrorMessage);

        Result<object> IParser<TInput>.ParseUntyped(ParseState<TInput> t)
            => t.FailUntyped(this, ErrorMessage);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public override string ToString()
        {
            var typeName = GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}
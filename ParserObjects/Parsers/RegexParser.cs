using System.Collections.Generic;
using System.Linq;
using ParserObjects.Regexes;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Uses limited Regular Expression syntax to match a pattern of characters. 
    /// </summary>
    public class RegexParser : IParser<char, string>
    {
        private readonly Regex _regex;
        private readonly RegexEngine _engine;

        public RegexParser(Regex regex, string describe)
        {
            Assert.ArgumentNotNull(regex, nameof(regex));
            _regex = regex;
            _engine = new RegexEngine();
            if (!string.IsNullOrEmpty(describe))
            {
                Name = describe;
                Pattern = describe;
            }
        }

        public string Pattern { get; }

        public string Name { get; set; }

        public IResult<string> Parse(ParseState<char> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var startLocation = state.Input.CurrentLocation;
            var (matches, str, pos) = _engine.GetMatch(state.Input, _regex);
            if (matches)
                return state.Success(this, str, startLocation);
            return state.Fail(this, $"Pattern failed at position {pos}");
        }

        IResult IParser<char>.Parse(ParseState<char> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();
    }
}

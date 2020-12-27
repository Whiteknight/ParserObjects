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
        private readonly Engine _engine;

        public RegexParser(Regex regex, string describe)
        {
            Assert.ArgumentNotNull(regex, nameof(regex));
            _regex = regex;
            _engine = new Engine();
            Name = $"/{describe}/";
            Pattern = describe;
        }

        public string Pattern { get; }

        public string Name { get; set; }

        public IResult<string> Parse(IParseState<char> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var startCp = state.Input.Checkpoint();
            var result = _engine.GetMatch(state.Input, _regex);
            if (!result.Success)
                startCp.Rewind();
            return state.Result(this, result);
        }

        IResult IParser<char>.Parse(IParseState<char> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}

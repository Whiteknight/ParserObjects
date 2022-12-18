using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

public record CaptureParser<TInput>(
    IReadOnlyList<IParser<TInput>> Parsers,
    string Name = ""
) : IParser<TInput, TInput[]>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public IResult<TInput[]> Parse(IParseState<TInput> state)
    {
        var startCp = state.Input.Checkpoint();
        for (int i = 0; i < Parsers.Count; i++)
        {
            var parser = Parsers[i];
            var result = parser.Match(state);
            if (!result)
            {
                startCp.Rewind();
                return state.Fail(this, "Inner parser failed");
            }
        }

        var endCp = state.Input.Checkpoint();
        var contents = state.Input.GetBetween(startCp, endCp);
        return state.Success(this, contents, endCp.Consumed - startCp.Consumed, startCp.Location);
    }

    IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

    public bool Match(IParseState<TInput> state)
    {
        var startCp = state.Input.Checkpoint();
        for (int i = 0; i < Parsers.Count; i++)
        {
            var parser = Parsers[i];
            var result = parser.Match(state);
            if (!result)
            {
                startCp.Rewind();
                return false;
            }
        }

        return true;
    }

    public IEnumerable<IParser> GetChildren() => Parsers;

    public INamed SetName(string name) => this with { Name = name };
}

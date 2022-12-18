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
        int consumed = 0;
        var results = new List<IResult>();
        for (int i = 0; i < Parsers.Count; i++)
        {
            var parser = Parsers[i];
            var result = parser.Parse(state);
            if (!result.Success)
            {
                startCp.Rewind();
                return state.Fail(this, "Inner parser failed", new object[] { result });
            }

            consumed += result.Consumed;
            results.Add(result);
        }

        var endCp = state.Input.Checkpoint();
        var contents = state.Input.GetBetween(startCp, endCp);
        return state.Success(this, contents, consumed, startCp.Location, new object[] { results });
    }

    IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

    public bool Match(IParseState<TInput> state) => Parse(state).Success;

    public IEnumerable<IParser> GetChildren() => Parsers;

    public INamed SetName(string name) => this with { Name = name };
}

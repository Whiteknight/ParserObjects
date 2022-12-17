using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

public record CaptureParser<TInput>(
    IParser<TInput> Inner,
    string Name = ""
) : IParser<TInput, TInput[]>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public IResult<TInput[]> Parse(IParseState<TInput> state)
    {
        var startCp = state.Input.Checkpoint();
        var result = Inner.Parse(state);
        if (!result.Success)
            return state.Fail(this, "Inner parser failed", new object[] { result });
        var endCp = state.Input.Checkpoint();
        var contents = state.Input.GetBetween(startCp, endCp);
        return state.Success(this, contents, result.Consumed, startCp.Location, new object[] { result });
    }

    IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

    public IEnumerable<IParser> GetChildren() => new[] { Inner };

    public INamed SetName(string name) => this with { Name = name };
}

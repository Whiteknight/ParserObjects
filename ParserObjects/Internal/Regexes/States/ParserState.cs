using System;
using ParserObjects.Internal.Regexes.Execution;

namespace ParserObjects.Internal.Regexes.States;

internal class ParserState : IState
{
    private readonly IParser<char> _parser;

    public ParserState(IParser<char> parser)
    {
        _parser = parser;
    }

    public Quantifier Quantifier { get; set; }

    public int Maximum { get; set; }

    public string Name => _parser.Name;

    public IState Clone() => new ParserState(_parser)
    {
        Quantifier = Quantifier,
        Maximum = Maximum
    };

    public bool Match(RegexContext context, SequenceCheckpoint beforeMatch, TestFunc test)
    {
        return _parser.Match(context);
    }

    public INamed SetName(string name) => throw new NotImplementedException();
}

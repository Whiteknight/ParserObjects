using ParserObjects.Internal.Regexes.Execution;

namespace ParserObjects.Internal.Regexes;

public interface IState
{
    /// <summary>
    /// Gets or sets the quantifier that controls how many times this state should match.
    /// </summary>
    public Quantifier Quantifier { get; set; }

    int Maximum { get; set; }

    IState Clone();

    bool Match(RegexContext context, SequenceCheckpoint beforeMatch, TestFunc test);
}

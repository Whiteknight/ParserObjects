using System.Collections.Generic;

namespace ParserObjects.Internal.Regexes;

public sealed class AlternationState : IState
{
    public AlternationState(string name, List<List<IState>> alternations)
    {
        Name = name;
        Alternations = alternations;
    }

    public Quantifier Quantifier { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of times this state can match, if it supports more than
    /// one. Used only with Range quantifiers.
    /// </summary>
    public int Maximum { get; set; }

    public string Name { get; }

    /// <summary>
    /// Gets or sets all possibilities in an alternation.
    /// </summary>
    public List<List<IState>> Alternations { get; set; }

    public INamed SetName(string name) => Clone(name);

    public IState Clone() => Clone(Name);

    private AlternationState Clone(string name)
        => new AlternationState(name, Alternations)
        {
            Quantifier = Quantifier,
            Maximum = Maximum
        };

    public override string ToString() => $"{State.QuantifierToString(Quantifier, Maximum)} {Name}";

    public bool Match(RegexContext context, SequenceCheckpoint beforeMatch, TestFunc test)
    {
        if (context.Input.IsAtEnd)
            return false;

        foreach (var substate in Alternations)
        {
            var matches = test(context.Captures, substate, context.Input, true);
            if (matches)
                return true;
        }

        return false;
    }
}

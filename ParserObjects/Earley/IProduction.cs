using System.Collections.Generic;

namespace ParserObjects.Earley
{
    // A production is the set of grammar elements which constitute a single rule. This would be
    // of the form "A := XYZ". The production also has a callback which is invoked when the rule
    // matches, to produce the output value.
    // Production is an ordered list of symbols and a callback.
    public interface IProduction
    {
        INonterminal LeftHandSide { get; }
        IReadOnlyList<ISymbol> Symbols { get; }

        IOption<object> Apply(object[] argsList);
    }
}

using System.Collections.Generic;

namespace ParserObjects.Earley;

/// <summary>
/// A sequence of grammar symbols and a rule to convert the produced values of each symbol into
/// a single result value. A production is a rule in the form "A := XYZ".
/// </summary>
public interface IProduction
{
    INonterminal LeftHandSide { get; }
    IReadOnlyList<ISymbol> Symbols { get; }

    IOption<object> Apply(object[] argsList);
}

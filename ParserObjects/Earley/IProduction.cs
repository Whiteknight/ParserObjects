using System.Collections.Generic;

#pragma warning disable S2326

namespace ParserObjects.Earley;

/// <summary>
/// A sequence of grammar symbols and a rule to convert the produced values of each symbol into
/// a single result value. A production is a rule in the form "A := XYZ".
/// </summary>
public interface IProduction
{
    /// <summary>
    /// Gets the left-hand side of the symbol. "A" in the rule "A := XYZ".
    /// </summary>
    INonterminal LeftHandSide { get; }

    /// <summary>
    /// Gets the right-hand side of the symbol. "XYZ" in the rule "A := XYZ".
    /// </summary>
    IReadOnlyList<ISymbol> Symbols { get; }

    /// <summary>
    /// Apply the production rule to the list of symbols and attempt to return a result.
    /// </summary>
    /// <param name="argsList"></param>
    /// <returns></returns>
    Option<object> Apply(object[] argsList);
}

public interface IProduction<TOutput> : IProduction
{
}

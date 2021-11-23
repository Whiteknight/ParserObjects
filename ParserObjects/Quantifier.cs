namespace ParserObjects;

/// <summary>
/// Flag to tell how many items a parser should match, if the parser is capable of matching
/// more than one item. Not all parsers with repetition will support all Quantifiers.
/// </summary>
public enum Quantifier
{
    /// <summary>
    /// The parser should match exactly one item. The parser will return an error if there are
    /// no matching items, and will ignore additional matches
    /// </summary>
    ExactlyOne,

    /// <summary>
    /// The parser should match one optional item. The parser will return success whether it
    /// matches an item or not, and will ignore any potential matches after the first one.
    /// </summary>
    ZeroOrOne,

    /// <summary>
    /// The parser should match any number of items. The parser will return success if there
    /// are no matches. If there are matches, the parser will attempt to match as many as
    /// there are.
    /// </summary>
    ZeroOrMore,

    /// <summary>
    /// The parser expects to match a number of items in a given range. The bounds of the range
    /// are passed separately
    /// </summary>
    Range
}

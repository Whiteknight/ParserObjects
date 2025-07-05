using System;

namespace ParserObjects.Earley;

/// <summary>
/// An exception thrown when there is a problem with the Earley grammar.
/// </summary>
public class GrammarException(string message) : Exception(message);

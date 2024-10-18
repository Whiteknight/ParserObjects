using System;

namespace ParserObjects.Regexes;

/// <summary>
/// Exception thrown during Regex pattern parsing and regex engine execution.
/// </summary>
public class RegexException : Exception
{
    public RegexException(string message) : base(message)
    {
    }
}

using System;

namespace ParserObjects.Regexes;

/// <summary>
/// Exception thrown during Regex pattern parsing and regex engine execution.
/// </summary>
public class RegexException(string message) : Exception(message);

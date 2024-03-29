﻿using System.Collections.Generic;

namespace ParserObjects.Regexes;

/// <summary>
/// A match object which includes the overall matched string and the contents of all capturing
/// groups. Group 0 is the overall match of the regex. Subsequence groupings are numbered based on
/// the ordering of the opening parenthesis for the grouping in the regex.
/// </summary>
public class RegexMatch : Dictionary<int, IReadOnlyList<string>>
{
}

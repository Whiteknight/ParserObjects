# Parsers index

This is an alphabetical list of all parsers from the ParserObjects library:

**Notice**: Parsers are named after the public factory methods which are used to create them. The actual classes which implement the behavior may be named other things, and a single implementation may be used in several of these named parsers:

* `And()` Attempt to match several parsers and returns success if they all succeed.
* `Any()` Matches any input item, fails at end of input.
* `Bool()` Returns success, with a boolean value containing the result success flag of the inner parser.
* `C.Character()` Match a C-style character literal.
* `C.Comment()` Match a C-style comment with `/* */` delimiters.
* `C.DoubleString()` Match a C-style double literal and return it as a `string`.
* `C.HexadecimalInteger()` Match a C-style hexadecimal literal which starts with `0x`. Returns the result as an `int`.
* `C.HexadecimalString()` Match a C-style hexadecimal literal which starts with `0x`. Returns the result as a `string`.
* `C.Identifier()` Match a C-style identifier and return it as a `string.
* `C.IntegerString()` Match a C-style integer literal and return a `string`.
* `C.Integer()` Match a C-style integer and return an `int`.
* `C.LongIntegerString()` Match a C-style long integer literal and return a `string`.
* `C.LongInteger()` Match a C-style long integer and return a `long`.
* `C.String()` Match a C-style string literal.
* `C.StrippedCharacter()` Match and decode a C-style char literal.
* `C.StrippedString()` Match and decode a C-style string literal.
* `C.UnsignedIntegerString()` Match a C-style unsigned integer and return a `string`.
* `C.UnsignedInteger()` Match a C-style unsigned integer literal and return an `uint`.
* `C.UnsignedLongIntegerString()` Match a C-style unsigned long integer and return a `string`.
* `C.UnsignedLongInteger()` Match a C-style unsigned long integer literal and return an `ulong`.
* `Cache()` Cache the result of a parser at the given input location and return it immediately if the same parser is invoked at the same position again.
* `CamelCase()` Match a series of characters in UpperCamelCaseFormat or lowerCamelCaseFormat.
* `Capture()` Matches several parsers in series and returns a list of all matched input items.
* `CaptureString()` matches several parsers in series and returns all matched input chars as a `string`.
* `Chain()` executes a parser, and then uses the result value to select the next parser.
* `ChainWith()` like `Chain` but with a fluent interface to select the next parser.
* `CharacterString()` Matches a literal string of characters and returns the matched string.
* `Choose()` Test a parser without consuming input, and then select the next parser based on the result.
* `Combine()` Executes several parsers in series and on success returns a list with all results.
* `Context()` Modify the current parse context before and after a parse.
* `ContinueWith()` Continue a multi-parse with each possibility in parallel.
* `ContinueWithEach()` Continue a multi-parse with each possibility in parallel using parsers from a callback.
* `Cpp.Comment()` Match a C++-style Comment with `//`.
* `Create()` Create a new parser at runtime using contextual data from the parse.
* `CreateMulti()` Create a new multi-parser at runtime using contextual data from the parse.
* `DataContext()` Create a new contextual data frame to hold contextual data.
* `Date()` Match a date value with a custom format string.
* `DateAndTime()` Match a date and time value with a custom format string.
* `Deferred()` Maintains an indirect reference to a parser, to support circular references.
* `DelimitedStringWithEscapedDelimiters()` Match a string with custom quote and escape characters.
* `Digit()` Matches a digit character.
* `DigitsAsInteger()` Matches of a string of subsequent digits and returns the value as a parsed `int`.
* `DigitString()` Matches a string of subsequent digits.
* `DoubleQuotedString()` Match a string using double quotes.
* `Each()` Executes several parsers at the current position and returns all results as a multi-result.
* `Earley` Allows construction of an Earley parser for recursive or ambiguous grammars.
* `Empty()` Matches at any position and returns no value.
* `End()` Matches successfully at the end of input, fails everywhere else, and returns no value.
* `Examine()` Allows inserting user-defined callbacks before and after a parser, for debugging purposes.
* `Fail()` Returns a failure result.
* `FailMulti()` Returns a failure multi-result.
* `First()` Tests several parsers in series, returns the first successful result.
* `FirstResult()` Take the first success from a multi-result only.
* `FollowedBy()` Match a value but only if it is followed by another matched value. Return only the first match.
* `Function()` Executes an arbitrary, unstructured user callback.
* `GetData()` Retrieve contextual data from the current data frame by name.
* `GuidB()` Parse a `Guid` in B format.
* `GuidD()` Parse a `Guid` in D format.
* `GuidN()` Parse a `Guid` in N format.
* `GuidP()` Parse a `Guid` in P format.
* `HexadecimalDigit()` Matches any hexadecimal digit `'a'-'f'`, `'A'-'F'`, `'0'-'9'`.
* `HexadecimalString()` Matches a series of hexadecimal digits. Returns a `string`.
* `If()` Match a parser and execute a different parser depending on success or failure.
* `IsEnd()` Matches successfully at any position but returns a boolean true at end of input and false elsewhere.
* `JS.Number()` Match a JavaScript-style Number literal, returned as a `double`.
* `JS.NumberString()` Match a JavaScript-style Number literal, returned as a `string`.
* `JS.String()` Match a JavaScript-style string literal.
* `JS.StrippedString()` Match and decode a JavaScript-style string literal.
* `Letter()` Matches any letter character.
* `Line()` Match a whole line of text and return it as a string.
* `List()` Match a parser multiple times and return all results as a list.
* `ListCharToString()` Match several characters and return them as a string.
* `ListStringsToString()` Match several strings and return them as a single string.
* `Longest()` Same as `LongestResult()`.
* `LongestResult()` Take the result from the multi-result which consumed the most input.
* `LowerCamelCase()` Matches a series of characters in lowerCamelCaseFormat.
* `LowerCase()` Match any lower-case letter.
* `Map()` Same as `Transform()`
* `Match` Attempts to match a single input item, or a pattern of several input items.
* `MatchAny()` Match one of several characters or character patterns.
* `MatchChar()` Match a character. An optimization over `Match()`
* `MatchChars()` Match several characters and return the result as a string. An optimization over `Match()`.
* `MatchItem()` Attempts to match an item but will not match at end of input.
* `NegativeLookahead()` Returns success if the given parser would not succeed from the current position.
* `None()` Executes a parser but ensures that no input is consumed.
* `NonGreedyList()` Match a parser the smallest number of times in order to continue the parse.
* `NonZeroDigit()` Matches any digit character except `'0'`.
* `Not()` Inverts the success result of the parser.
* `NotFollowedBy()` Match a value but only if it would not be followed by another value.
* `NotMatchAny()` Match a character which does not exist in the given collection.
* `Optional()` Executes a parser but always returns success.
* `OptionalWhitespace()` Match zero or more whitespace characters.
* `Or()` Attempts several parsers and returns success as soon as any one of them succeed.
* `Peek()` Return the next item of input but do not consume it.
* `PositiveLookahead()` Returns success if the given parser would succeed from the current position.
* `Pratt()` Create a Pratt Parser for operator-climbing and operator-precedence scenarios.
* `Predict()` Same as `ChainWith`.
* `PrefixedLine()` Parse a whole line of text that starts with a given prefix.
* `Produce()` Return a value from a user callback and consume no input.
* `ProduceMulti()` Return a multi-value from a user callback and consume no input.
* `Regex()` Match a pattern defined by a regular expression.
* `RegexPattern()` Parse a regular expression into a `Regex` object.
* `Replaceable()` Allows a parser reference to be changed after build-up.
* `Rule()` Execute several parsers and combine their results using a custom callback.
* `ScreamingSnakeCase()` Match a series of characters in SCREAMING_SNAKE_CASE_FORMAT.
* `ScreamingSpinalCase()` Match a series of characters in SCREAMING-SPINAL-CASE-FORMAT.
* `Select()` Same as `SelectResult()`.
* `SelectResult()` Invoke a callback to select the best result from a multi-result.
* `Sequential()` Executes an arbitrary structured user callback.
* `SetData()` Set a data value into the current contextual data frame by name.
* `SetResultData()` Save the result of a parser into the current contextual data frame.
* `SingleQuotedString()` Match a string using single quotes.
* `Single()` Same as `SingleResult()`.
* `SingleResult()` Expect and return a single successful result from a multi-result or return failure.
* `SnakeCase()` Match a series of characters in Snake_Case_Format.
* `SpinalCase()` Match a series of characters in Spinal-Case-Format
* `Sql.Comment()` Match an SQL-style Comment with `--`.
* `Sql.Identifier()` Match an SQL-style identifier.
* `Sql.QualifiedIdentifier()` Match an SQL-style multipart identifier.
* `Start()` Returns success at the start of input, failure otherwise
* `StartOfLine()` Returns success at the start of a new line (at `Start()` and immediately after `\n`) for character sequences only.
* `Stringify()` Convert an `IReadOnlyList<char>` result into a `string` result.
* `StrippedDoubleQuotedString()` Match and decode a string using double quotes.
* `StrippedDelimitedStringWithEscapedDelimiters()` Match and decode a string with custom quote and escape characters.
* `StrippedSingleQuotedString()` Match and decode a string using single quotes.
* `Symbol()` Match any symbol character.
* `Synchronize()` Attempt the parse. If it fails, consume input until we can get back to a known good state and continue.
* `Then()` Same as `If()`.
* `Time()` Match a time value with a custom format string.
* `Transform()` Modify the output value of a parser.
* `Trie()` Use a prefix tree to efficiently match one of several patterns and return the longest result.
* `TrieMulti()` Use a prefix tree to efficiently match several patterns and return all matches as a multi-result.
* `Try()` Catch and handle exceptions thrown during a parse.
* `UpperCamelCase()` Match a series of characters in UpperCamelCaseFormat
* `UpperCase()` Match any upper-case letter.
* `Whitespace()` Match several characters of whitespace and return them all as a string.
* `WhitespaceCharacter()` Match a single character of whitespace.
* `WithDataContext()` Same as `DataContext()`.
* `Word()` Match a series of `Letter` characters and returns the result as a `string`.


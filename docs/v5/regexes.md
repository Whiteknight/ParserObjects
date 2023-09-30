# Regexes

The Regular Expression engine provided by ParserObjects has fewer features than an ordinary Regular Expression engine, such as the one provided by the `System.Text.Regex` class. The following features are supported:

1. Literal characters `"abc"`
2. Wildcards `.`
3. Basic Quantifiers
   1. Zero or One `?`
   2. Zero or More `*`
   3. One or More `+`
4. Range quantifiers
   1. `{3}` exactly three
   2. `{3,}` at least three
   3. `{,5}` at most five
   4. `{3,5}` between three and five
5. Capturing Groups `( )`
   1. Backreferences `\1`
6. Non-Capturing Cloisters `(?: )`
7. Alternations `|`
8. Character Classes
   1. `[abc]` literal characters
   2. `[a-cA-C]` character ranges
   3. `[^abc]` and `[^a-c]` inverted ranges
9. Built-In Character Classes
   1. `\s` and `\S` whitespace and non-whitespace
   2. `\w` and `\W` word and non-word
   3. `\d` and `\D` digit and non-digit
10. `$` End anchor

The Regex parser always attempts to match the pattern starting at the current position of the input sequence, and will not look forward to try and find a match which begins later. Because of this, the Regex parser does not support the beginning of input anchor (`^`) in patterns. However, it does support the end of input anchor (`$`) to match the end of input of the sequence.
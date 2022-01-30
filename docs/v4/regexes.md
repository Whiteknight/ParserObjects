# Regexes

The Regular Expression engine provided by ParserObjects has fewer features than an ordinary Regular Expression engine, such as the one provided by the `System.Text.Regex` class. The following features are supported:

1. Literal characters `"abc"`
2. Basic Quantifiers
   1. Zero or One `?`
   2. Zero or More `*`
   3. One or More `+`
3. Range quantifiers
   1. `{3}` exactly three
   2. `{3,}` at least three
   3. `{,3}` at most three
   4. `{3,5}` between three and five
4. Groups `( )`
5. Alternations `|`
6. Character Classes
   1. `[abc]` literal characters
   2. `[a-cA-C]` character ranges
   3. `[^abc]` and `[^a-c]` inverted ranges
7. Built-In Character Classes
   1. `\s` and `\S` whitespace and non-whitespace
   2. `\w` and `\W` word and non-word
   3. `\d` and `\D` digit and non-digit
8. `$` End anchor

The Regex parser always attempts to match the pattern starting at the current position of the input sequence, and will not look forward to try and match later in the sequence. Because of this, the Regex parser does not support the beginning of input anchor (`^`) in patterns. However, it does support the end of input anchor (`$`).
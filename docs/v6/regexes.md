# Regular Expressions

The Regular Expression engine provided by ParserObjects has fewer features than an ordinary Regular Expression engine, such as the one provided by the `System.Text.Regex` class or the popular PCRE library. 

An **atom** is a piece of input which can be matched. A **quantifier** modifies the atom to repeat a certain number of times.

## Basic Atoms

### Literal Characters

Any literal character such as `a` will be `5` matched literally. The exceptions to this rule are the *special characters* which need to be escaped with `\`. The list of special characters includes:

   \ ( ) $ | [ ] . ? + * { }

### Wildcards

The character `'.'` will match any single input character. It will fail at end of input.

### Character Classes

The `[ ]` define a character class. A character class matches any single character which fits in that class.
* `[abc]` literal characters `'a'`, `'b'` or `'c'`
* `[a-cA-C]` character ranges. Matches `'a'`, `'b'`, `'c'`, `'A'`, `'B'` or `'C'`
* `[^abc]` inverted classes and `[^a-c]` inverted ranges. Matches the opposite of the range.

### Built-in Character Classes

Some common character classes are already pre-defined with special names:
1. `\s` matches any whitespace (space, tab, newline, etc)
2. `\S` matches any non-whitespace (letters, digits, symbols, etc)
3. `\w` matches any word character (letters and digits)
4. `\W` matches any non-word character (symbols, whitespace, etc)
5. `\d` matches any digit. Same as `[0-9]`
6. `\D` matches any non-digit. Same as `[^0-9]`

## Quantifiers

### Basic Quantifiers

Quantifiers include:

* `'?'` which matches the atom zero or one times. In other words, the atom is optional.
* `'*'` which matches the atom zero or more times.
* `'+'` which matches the atom one or more times.

For example the pattern `"ab?a"` will match `"aa"` or `"aba"` because the `'b'` is optional.

### Range Quantifiers

The `{ }` braces define a range. The range may have one or two numbers, separated by a comma, to define how many copies are expected:

1. `{3}` exactly three
2. `{3,}` at least three
3. `{,5}` at most five
4. `{3,5}` between three and five

## Groups

### Capturing Groups

The `( )` define a capturing group. A capturing group matches whatever is inside the parenthesis and captures the value of it in the output.

You can access the captured groups in `RegexMatch` result object from the `RegexMatch()` parser:

```csharp
var parser = RegexMatch("a(..)d");
var result = parser.Parse("abcd");
var captured = result.Value[1][0]; // "bc"
```

Notice that `result[0][0]` is always the total overall match. Groups are numbered, starting with `1`, from left to right. Groups may be nested:

```csharp
var parser = RegexMatch("a(.(..).)f");
var result = parser.Parse("abcdef");
var capture0 = result.Value[0][0]; // "abcdef"
var capture1 = result.Value[1][0]; // "bcde"
var capture2 = result.Value[2][0]; // "cd"
```

If a group is quantified, every instance of a match will be captured:

```csharp
var parser = RegexMatch("a(..)*z");
var result = parser.Parse("abcdefgz");
var capture1 = result.Value[1][0]; // "bc"
var capture1 = result.Value[1][1]; // "de"
var capture1 = result.Value[1][2]; // "fg"
```

### Back-references

The syntax `\1` or other digit from 1 - 9, defines a back-reference. A back-reference matches a value previously captured by the numbered group.

```csharp
var parser = RegexMatch(@"a(..)d\1");
var result = parser.Parse("abcdbc");
var captured = result.Value[1][0]; // "bc"
```

### Non-Capturing Cloisters

A non-capturing cloister, or non-capturing group, is defined with `(?: )`. These operate the same as capturing groups except they do not capture the results and cannot be used with back-references. These are useful for when you want to apply a quantifier to an entire group but do not need to capture the value.

```csharp
var parser = RegexMatch("a(?:..)?d");
var result = parser.Parse("abcd");
var numberOfGroups = result.Count; // Just 1, for the overall match
```

### Zero-Width Lookahead

The regex engine can lookahead and match additional characters, but not include those as part of the match. The Positive lookahead `(?= )` matches true if the match succeeds but does not include that value in the overall match. The Negative lookahead `(?! )` matches true if the match does not succeed and will not include any values in the match.

```csharp
var parser = Regex("a(?=b)");
var result = parser.Parse("ab");
var captured = result.Value; // "a"
```

```csharp
var parser = Regex("a(?!b)");
var result = parser.Parse("ac");
var captured = result.Value; // "a"
```

## Alternations

The `'|'` character can be used to make alternations, which allow you to check multiple patterns at the current location and return a successful match as soon as the first one succeeds. 

```csharp
var parser = Regex("a|b|c");
var result = parser.Parse("b");
var captured = result.Value; // "b"
```

## Anchors

### End anchor

The `'$'` character is the end anchor. It only matches true at the end of the input sequence and consumes no input.

## Parsers

There are two parsers for regex. `Regex()` returns a string result, though it does include the `RegexMatch` object in the result data:

```csharp
var parser = Regex("abc");
var result = parser.Parse("abc");
var matchObject = result.Data<RegexMatch>();
var overallMatchString = result.Value;
```

The `RegexMatch()` parser returns the `RegexMatch` object as the result.

```csharp
var parser = RegexMatch("abc");
var result = parser.Parse("abc");
var matchObject = result.Value;
var overallMatchString = result.Value[0][0];
```

Which of the two parsers to use is dependent on what you want to do with the result you obtain.

## Behaviors

The Regex parser always attempts to match the pattern starting at the current position of the input sequence. It will not skip over input characters looking for a match which occurs later. Because of this the Regex parser does not support the beginning of input anchor (`^`) in patterns. It also does not support the `\b` word-boundary because it cannot look backwards in the input sequence to determine if the previous character was a word or non-word character. 

The Regex parser is greedy. It will attempt to match as much input as it can and return the first match which satisfies the entire pattern.

The regex parser does backtrack. This means quantified atoms will attempt to match as much as possible and, if subsequent matches fail the quantified atoms will "return" some matched inputs and try again. This can lead to pathological behavior where multiple quantified atoms are forced to try large numbers of permutations before a successful match can be found. This page will not attempt to explain the variety of cases where a regex may be driven to pathological behavior.
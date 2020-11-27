# Pratt Parsers

Pratt parsers are a special type of [Operator Precidence Parser](https://en.wikipedia.org/wiki/Operator-precedence_parser#Pratt_parsing) which can dramatically simplify some expression-parsing scenarios. Setting up a Pratt parser is a little different from other parsers because you have to know the type of operator you are parsing, and explicitly set precidence strenght values for both left and right association.

## Basic Operation

```csharp
using static ParserObjects.ParserMethods<char>;
```

```csharp
var target = Pratt(config => {
    ...
});
```

## Operator Types

### Infix

Infix operators are operators which go between two operands. For example, the `+` in `"1 + 2"`. By common convention, for mathematical formulas, `+` and `-` operators are low precidence while `*` and `/` are higher precidence. 

Infix operations can be inserted by calling `.AddInfixOperator` with a parser to match the required operators. Infix operations take two precidence or "binding strength" values, one for the left side and one for the right side. These values generally should be consecutive integers, and should not overlap with binding strength numbers from other operators. If the left value is higher than the right value, the operator will be left-associative. Otherwise it will be right-associative.

```csharp
config
    .AddInfixOperator(Match('+'), 1, 2, (left, op, right) => left + right)
    ;
```

### Prefix

Prefix operators are operators which go before a single operand, such as the `-` sign in `"-1"`.

Prefix operators take a single binding strength value, for how strongly it binds to the right-hand operand. This value should not overlap with a range from other operators which also affect operands on the right.

```csharp
config
    .AddPrefixOperator(Match('-'), 3, (op, right) => -right)
    ;
```

### Postfix

Postfix operators are operators which go after a single operand, such as the `!` factorial sign in `"5!"`.

Postfix operators take a single binding strength parameter for how tightly they bind to the operand on the left. This value should not overlap with a range from other operators which also affect operands on the left.

```csharp
config
    .AddPostfixOperator(Match('!'), 6, (left, op) => Factorial(left))
    ;
```

### Circumfix

Circumfix operators are operators which surround an operand, such as the parenthesis `(` and `)` in `"(1 + 2)"`

Circumfix operators do not have a precidence or binding strength parameter, because the internal contents are clearly delimited without any concern for ordering. Circumfix operators require two parsers, one for the starting or opening operator and one for the ending or closing operator.

```csharp
config
    .AddCircumfixOperator(Match('('), Match(')'), (open, contents, close) => contents)
    ;
```

### Postcircumfix

Postcircumfix operators are a special case combined from postfix and circumfix. A good example is the common array-indexing syntax of `[` and `]` in `"a[b]"`

Postcircumfix operators take two parsers for opening and closing symbols, like circumfix parsers do, and they also take a left associativity value like postfix parsers do. This binding strength value should not overlap with any other binding strength ranges for other operators which bind to operands on the left.

```csharp
config
    .AddPostcircumfixOperator(Match('['), Match(']'), 8, (left, open, right, close) => GetArrayIndex(left, right))
    ;
```

### Unsupported Types

The Pratt parser implementation in ParserObjects does not currently support other types of operator constructs. For example, "precircumfix" types (like the cast in `"(int)value"` from C-like languages), or operators with more than three operands (like the ternary operator `?` `:` in `"ok ? 5 : 7"`). These things could be added in future releases if needed, or they can be implemented by the user.

## Associativity Strength

The difficulty in using Pratt parsers comes from having to manually setup and keep track of associativity values for all of your precidence levels. By general convention, the left and right precidence values should be consecutive values if possible, and values should not generally be reused. While most operator types take only a single associativity value, the infix operators take two. You should come up with a convention for your own code, such as every parser should use two consecutive numbers, with the even number being lower and the odd number being higher. `(1, 2)` or `(2, 1)` would be the first range (depending on direction), `(3, 4)` or `(4, 3)` would be the second range, etc. If the operator in question doesn't use both values from the given range, silently ignore the unused value and use the next range for the next operator level. 

```csharp
config
    .AddInfixOperator(Match('+'), 1, 2, (left, op, right) => left + right)
    .AddInfixOperator(Match('='), 4, 3, (left, op, right) => left + right)
    .AddPrefixOperator(Match('-'), 5, (op, right) => -right)               
    .AddPostfixOperator(Match('!'), 7, (left, op) => Factorial(left))
    ;
```

Notice in this example that the first parser, `+`, uses range `(1, 2)` and is right associative. The second parser `=` uses range `(3, 4)` and is left associative. The third parser `-` uses range `(5, 6)` though the 6 is not used, and is right associative. The fourth parser `!` uses range `(7, 8)` though the 8 is not used and is left associative. The more you can do to keep track of precidence ranges, the more maintainable and modifiable your parser will be.
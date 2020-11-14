# Reverse Polish Notation Parser

The Reverse Polish Notation (RPN) parser demonstrates the use of the `FunctionParser` to parse a grammar which is not easily amenable to the "basic" set of parser combinators.

The grammar for RPN looks something like this:

```
<op>                ::= '+' | '-' | '*' | '/'
<operation>         ::= <numberOrOperation> <numberOrOperation> <op>
<numberOrOperation> ::= <operation> | <number>
```

ParserObjects provides parsers to help with left and right recursion with a particular focus in helping to parse infix expressions, but this grammar possibly recurses in both the left and middle positions, so those existing `LeftApply` and `RightApply` parsers don't really help.

By using the `FunctionParser` this example can instead convert to the more traditional stack-based algorithm for reading RPN, and wrap that algorithm up into a form that is compatible with other parser combinators.
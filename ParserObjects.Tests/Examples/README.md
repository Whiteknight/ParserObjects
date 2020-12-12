# Examples

The examples in this directory serve two purposes: First, to be a set of end-to-end functional tests for the ParserObjects library to ensure that key workflows operate as expected and Second, to serve as examples to the user for some possibilities and techniques involved in parsing. Examples are not generally picked to be the absolute best way to solve a particular problem, but as a showcase for some of the features which may be hardest to understand or leverage correctly.

These examples try to show different techniques, including use of different parser types and also strategies for handling common parsing tasks: Whether to use a scanner-based or scannerless parser, how to handle whitespace and comments, how to detect and handle parse errors, whether to produce an output value directly or create an Abstract Syntax Tree (AST) to hold an intermediate representation. 

Each example comes with it's own README file which goes over the example in more detail and points out interesting parts from each.

### Overview

#### Classes

The **Classes** example uses a `Pratt` parser to parse pseudo-C# `class` and `interface` declarations. This is a demonstration of the Pratt technique, and also shows that Pratt can be used with other parsing tasks besides just operator precidence expression parsing. This parser is a single-phase scannerless design with whitespace handling explicitly built directly into the grammar.

#### ExprCalculator 

The **ExprCalculator** example demonstrates the use of the `LeftApply` parser to parse mathematical expressions with a few simple operators and precidence levels. This is a two-phase parser with separate lexical analysis and parsing phases. It only supports four of the basic infix operators and does not handle negative numbers, prefixes, parenthesis, etc. Whitespace is ignored in the lexer. Any syntax errors cause the entire parse to fail and there is no attempt at recovery. The parser calculates a result directly and does not create an AST.

#### Parens

The **Parens** example demonstrates the use of the `Pratt` parser again to parse a mathematical expression with levels of precidence, and produces a string where the equation is properly parenthesized. This parser uses a single-phase scannerless approach where whitespace is explicitly handled by the Pratt parser and errors cause the entire parser to fail. 

#### PN

The **PN** example parses Polish Notation expressions. It shows the use of the `Chain` parser to select strategies to use at each point in the parse. This is a scannerless design with explicit whitespace handling and no error correction.

#### RPN

The **RPN** example parses Reverse Polish Notation expressions. It shows the use of the `Function` parser to parse the input using a stack-based algorithm. This is a two-phase design with separate lexical analysis and parsing phases, though everything is organized into a single class. Whitespace is handled explicitly in the lexer rules, though some cases are omitted.

#### SExpr

The **SExpr** example parses a simple dialect of S-Expressions and returns an AST. This is a two-phase design where whitespace is parsed by the lexer and returned as whitespace tokens, which are filtered out of the stream prior to the parser. Some error-handling is done to handle some situations of mismatched parentheses. In case of an error, AST nodes are still created, but they contain error messages which can be detected by calling code.

#### XML

The **XML** example parses a small subset of the XML language with opening and closing tags only. It shows the use of contextual state mechanisms so that closing tags can be expected to match the names of the respective opening tags. This is a single-phase design where whitespace is not tolerated at all and there is no error handling.
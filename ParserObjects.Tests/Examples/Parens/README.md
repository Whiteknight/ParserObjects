# Expression Parenthesizer

This example uses the `Pratt` parser to parse an expression with multiple levels of precidence, and produce a parenthesized output which removes ambiguity in the order of operations. It is worth noting that this example is significantly smaller than some other expression-parsing examples in this suite, though it does not handle some features like insignificant whitespace or error diagnostics. This example can be modified to tolerate whitespace or give useful error-messages, which will be left as an exercise for the reader.

One interesting feature which this parser handles and other expression parsers in this suite do not, is the "implied multiplication" of the form `"x(y)"`. This notation is common in hand-written mathematics but rarely used in programming languages. This construct translates to `"x * (y)"` though it has a slightly higher precidence than normal multiplication and division. The close contact between the two terms signifies that they are tightly linked, and should not be separated due to normal order-of-operations rules. The expression `"4*3(2)"` is parenthesized as `"(4*(3*(2)))"` instead of `"((4*3)*(2))"` which might be the case if all multiplications were strictly evaluated from left-to-right at a single level of precidence. This rule is implemented using the postcircumfix operator in the Pratt parser. It's worth noting that the reverse notation is not implemented: `"(4)2"` does not parse.




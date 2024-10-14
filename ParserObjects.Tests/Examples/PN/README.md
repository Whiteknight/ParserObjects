# Polish Notation Parser

The example parser for Polish Notation (PN) demonstrates the use of the `ChainParser` to parse and evaluate PN in a single step.

Of particular interest is this declaration:

```csharp
var operation = operators.Chain(r =>
{
    if (!r.Success)
        return Fail<int>("Unrecognized operator");
    var op = r.Value;
    if (op == '+')
        return operands.Transform(v => v.Item1 + v.Item2);
    if (op == '-')
        return operands.Transform(v => v.Item1 - v.Item2);
    if (op == '*')
        return operands.Transform(v => v.Item1 * v.Item2);
    if (op == '/')
        return operands.Transform(v => v.Item1 / v.Item2);
    return Fail<int>("Unrecognized operator");
});
```

When we parse the operator, we then parse the `operands` and then choose which transformation to apply to it.

Notice that there are other ways to accomplish this same goal. We could, for example, parse the PN to an abstract syntax tree, and then traverse the tree depth-first to evaluate the expression. This approach would be "pure" in the sense that all parsers are constructed at initialization time and the parsers only depend on the input sequence and not on any external logic or data. However, that approach would also require two stages instead of one, and would require more allocations for all the nodes in the AST.

Another approach is that we could have used the `First` parser, followed by individual `Rule` parsers for each operator, operands, and the associated transformation. This approach is more memory-friendly than the AST approach but the parser itself is going to be larger because of all the additional rules to attempt.
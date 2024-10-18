namespace ParserObjects.Tests.Examples.SExpr;

public enum ValueType
{
    Operator,
    Number,
    Symbol,
    QuotedString,
    Whitespace,
    OpenParen,
    CloseParen,
    End
}

namespace ParserObjects.Tests.Examples.PrattCalculator
{
    public enum TokenType
    {
        Error,
        Addition,
        Division,
        Multiplication,
        Subtraction,
        Exponentiation,
        Number,
        OpenParen,
        CloseParen,
        End
    }
}
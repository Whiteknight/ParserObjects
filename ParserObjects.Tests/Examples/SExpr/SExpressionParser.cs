using ParserObjects.Sequences;

namespace ParserObjects.Tests.Examples.SExpr
{
    public class SExpressionParser
    {
        public INode Parse(string expr)
        {
            var lexer = LexicalGrammar.CreateParser();
            var chars = expr.ToCharacterSequence();
            var tokens = lexer
                .ToSequence(chars)
                .Select(r => r.Value)
                .Where(t => t.Type != ValueType.Whitespace);
            var parser = SExpressionGrammar.CreateParser();
            var result = parser.Parse(tokens);
            return result.Value;
        }
    }
}

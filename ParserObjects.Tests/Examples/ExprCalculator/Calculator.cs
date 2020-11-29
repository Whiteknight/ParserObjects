using ParserObjects.Sequences;
using System.Diagnostics;

namespace ParserObjects.Tests.Examples.ExprCalculator
{
    public class Calculator
    {
        public int Calculate(string equation)
        {
            // Turn the input string into a sequence of characters
            var characterSequence = new StringCharacterSequence(equation);

            // Get the lexical grammar, and use it to create a sequence of tokens
            var lexicalParser = LexicalGrammar.CreateParser();
            var tokenSequence = lexicalParser.ToSequence(characterSequence).Select(r =>
            {
                if (r.Success)
                    return r.Value;
                return new Token(TokenType.Error, r.ToString());
            });

            // Get the expression grammar and parse the token sequence into a result
            var expressionParser = ExpressionGrammar.CreateParser();
            var result = expressionParser.Parse(tokenSequence, s => Debug.WriteLine(s));

            // Get the value of the result
            return result.Value;
        }
    }
}

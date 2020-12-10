using System.Collections.Generic;
using System.Linq;
using static ParserObjects.ParserMethods<ParserObjects.Tests.Examples.SExpr.Token>;
using static ParserObjects.Tests.Examples.SExpr.TokenParserMethods;

namespace ParserObjects.Tests.Examples.SExpr
{
    public static class SExpressionGrammar
    {
        public static IParser<Token, INode> CreateParser()
        {
            var number = Token(ValueType.Number).Transform(o => new AtomNode { Type = ValueType.Number, Value = o.Value, Location = o.Location });
            var str = Token(ValueType.QuotedString).Transform(o => new AtomNode { Type = ValueType.QuotedString, Value = o.Value, Location = o.Location });
            var symbol = Token(ValueType.Symbol).Transform(o => new AtomNode { Type = ValueType.Symbol, Value = o.Value, Location = o.Location });
            var oper = Token(ValueType.Operator).Transform(o => new AtomNode { Type = ValueType.Operator, Value = o.Value, Location = o.Location });

            var atom = First(
                number,
                str,
                symbol,
                oper
            );

            IParser<Token, INode> listInternal = null;
            var list = Deferred(() => listInternal);

            var listElement = First(
                atom,
                list
            );

            var requiredCloseParen = First(
                Token(ValueType.CloseParen),
                Produce((t, d) => new Token
                {
                    Location = t.CurrentLocation,
                    Type = ValueType.CloseParen,
                    Value = ")",
                    Diagnostics = new List<string>
                    {
                        "Missing close parenthesis"
                    }
                })
            );

            var listContents = listElement.List();
            listInternal = Rule(
                Token(ValueType.OpenParen),
                listContents,
                requiredCloseParen,
                (open, contents, close) =>
                {
                    var node = new ExpressionNode { Children = contents.Cast<INode>().ToList() };
                    if (close.Diagnostics?.Count > 0)
                        node.Diagnostics = close.Diagnostics;
                    return node;
                }
            );

            return First(
                atom,
                list
            );
        }
    }
}

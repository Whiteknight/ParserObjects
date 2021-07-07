using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Earley;
using static ParserObjects.CStyleParserMethods;
using static ParserObjects.ParserMethods;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class EarleyParserTests
    {
        [Test]
        public void SingleTerminal_Test()
        {
            var target = Earley<int>(symbols => symbols
                .New("S")
                .AddProduction(UnsignedInteger(), n => n)
            );

            var result = target.Parse("4");
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(1);
            result.Results[0].Value.Should().Be(4);
        }

        [Test]
        public void BasicExpression_Test()
        {
            var target = Earley<int>(symbols =>
            {
                var plus = Match('+').Named("plus");
                var star = Match('*').Named("star");

                var expr = symbols.New("Expr")
                    .AddProduction(UnsignedInteger().Named("literal"), n => n);

                expr.AddProduction(expr, plus, expr, (l, _, r) => l + r);
                expr.AddProduction(expr, star, expr, (l, _, r) => l * r);

                return expr;
            });

            var result = target.Parse("4*5+6");
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(4);

            var values = result.Results.Where(r => r.Success).Select(r => r.Value).ToList();

            // We haven't put in any precidence rules, so we're getting all possibilities
            values.Should().Contain(4);  // 4 = 4
            values.Should().Contain(20); // 4*5 = 20
            values.Should().Contain(26); // (4*5)+6 = 26
            values.Should().Contain(44); // 4*(5+6) = 44
        }

        [Test]
        public void BasicExpression_EOF()
        {
            var target = Earley<int>(symbols =>
            {
                var plus = Match('+').Named("'+'");
                var star = Match('*').Named("'*'");

                var expr = symbols.New<int>("Expr")
                    .AddProduction(UnsignedInteger().Named("literal"), n => n);

                expr.AddProduction(expr, plus, expr, (l, _, r) => l + r);
                expr.AddProduction(expr, star, expr, (l, _, r) => l * r);

                var eof = If(End(), Produce(() => true));

                return symbols.New("S")
                    .AddProduction(expr, eof, (e, _) => e);
            });

            var result = target.Parse("4*5+6");
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(2);
            var values = result.Results.Where(r => r.Success).Select(r => r.Value).ToList();

            // We haven't put in any precidence rules, so we're getting all possibilities
            // We don't see partial results "4" or "4*5" because the start rule requires EOF
            values.Should().Contain(26); // (4*5)+6 = 26
            values.Should().Contain(44); // 4*(5+6) = 44
        }

        [Test]
        public void BasicExpression_MultiCharItems()
        {
            var target = Earley<int>(symbols =>
            {
                var plus = CharacterString("+++").Named("plus");
                var star = CharacterString("**").Named("star");

                var expr = symbols.New("Expr")
                    .AddProduction(UnsignedInteger().Named("literal"), n => n);

                expr.AddProduction(expr, plus, expr, (l, _, r) => l + r);
                expr.AddProduction(expr, star, expr, (l, _, r) => l * r);
                return expr;
            });

            var result = target.Parse("11+++25**30");
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(4);
            var values = result.Results.Where(r => r.Success).Select(r => r.Value).ToList();

            // We haven't put in any precidence rules, so we're getting all possibilities
            values.Should().Contain(11);   // 11 = 11
            values.Should().Contain(36);   // 11 + 25 = 36
            values.Should().Contain(1080); // (11+25)*30 = 1080
            values.Should().Contain(761);  // 11+(25*30) = 761
        }

        [Test]
        public void BasicExpression_Select()
        {
            var parser = Earley<int>(symbols =>
            {
                var plus = Match('+').Named("plus");
                var star = Match('*').Named("star");

                var expr = symbols.New("Expr")
                    .AddProduction(UnsignedInteger().Named("literal"), n => n);

                expr.AddProduction(expr, plus, expr, (l, _, r) => l + r);
                expr.AddProduction(expr, star, expr, (l, _, r) => l * r);
                return expr;
            });

            var target = parser.Select((r, success, fail) =>
            {
                var best = r.Results.Where(alt => alt.Value % 2 == 1).OrderByDescending(alt => alt.Value).FirstOrDefault();
                if (best == null)
                    return fail();
                return success(best);
            });

            var result = target.Parse("11+25*30");
            result.Success.Should().BeTrue();
            result.Value.Should().Be(761); // 11+(25*30) = 761
        }

        // TODO: Need tests showing matches with common prefixes work correctly

        [Test]
        public void LeftRecurse_CountSymbols()
        {
            // E ::= empty
            //     | E a
            var target = Earley<int>(symbols =>
            {
                var empty = Produce(() => 0);

                var expr = symbols.New<int>("Expr")
                    .AddProduction(empty, v => v);
                expr.AddProduction(expr, Match('a'), (count, _) => count + 1);

                var eof = If(End(), Produce(() => true));

                return symbols.New("Start")
                    .AddProduction(expr, eof, (v, _) => v);
            });

            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(1);
            result.Results[0].Value.Should().Be(0);

            result = target.Parse("a");
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(1);
            result.Results[0].Value.Should().Be(1);

            result = target.Parse("aaaa");
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(1);
            result.Results[0].Value.Should().Be(4);

            result = target.Parse("aaaaaaaa");
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(1);
            result.Results[0].Value.Should().Be(8);
        }

        [Test]
        public void RightRecurse_CountSymbols()
        {
            // E ::= a
            //     | a E
            var target = Earley<int>(symbols =>
            {
                var a = Match('a').Named("a");

                var e = symbols.New<int>("E")
                    .AddProduction(a, _ => 1);
                e.AddProduction(a, e, (_, count) => count + 1);

                var eof = If(End(), Produce(() => true)).Named("End");

                return symbols.New("S")
                    .AddProduction(e, eof, (count, _) => count).Named("S");
            });

            // TODO: Need to look at the State listings to verify if we have Leo's optimization
            // or not.

            var result = target.Parse("");
            result.Success.Should().BeFalse();

            result = target.Parse("a");
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(1);
            result.Results[0].Value.Should().Be(1);

            result = target.Parse("aaaa");
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(1);
            result.Results[0].Value.Should().Be(4);

            result = target.Parse("aaaaaaaa");
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(1);
            result.Results[0].Value.Should().Be(8);
        }

        [Test]
        public void Nullable_Test()
        {
            var target = Earley<string>(symbols =>
            {
                // Nullable is a production rule which consumes no input
                var nullable = Produce(() => "Test");
                var eof = If(End(), Produce(() => true));
                return symbols.New("S")
                    .AddProduction(nullable, eof, (a, _) => a);
            });

            var results = target.Parse("");
            results.Success.Should().BeTrue();
            results.Results.Count.Should().Be(1);
            results.Results[0].Value.Should().Be("Test");
        }

        [Test]
        public void Nullable_Indirect_Sequence()
        {
            // Nullable rules consume no input. In this case, "nullable" nonterminal calls down
            // to two nullable rules, so "nullable" is nullable indirectly (both sub-symbols are
            // nullable)

            // A := Produce("A")
            // B := Produce("B")
            // N := A B
            // S := N N END
            var target = Earley<string>(symbols =>
            {
                var rulea = Produce(() => "A").Named("A");
                var ruleb = Produce(() => "B").Named("B");
                var nullable = symbols.New("N")
                    .AddProduction(rulea, ruleb, (a, b) => $"({a},{b})");

                var eof = If(End(), Produce(() => true)).Named("END");
                return symbols.New("S")
                    .AddProduction(nullable, nullable, eof, (first, second, end) => $"[{first}:{second}]");
            });

            var results = target.Parse("");
            results.Success.Should().BeTrue();
            results.Results.Count.Should().Be(1);
            results.Results[0].Value.Should().Be("[(A,B):(A,B)]");

            results = target.Parse("GARBAGE");
            results.Success.Should().BeFalse();
        }

        [Test]
        public void Nullable_Indirect_Alternatives()
        {
            // Nullable rules consume no input. In this case, "nullable" nonterminal calls down
            // to two nullable rules, so "nullable" is nullable indirectly (both sub-symbols are
            // nullable)

            // A := Produce("A")
            // B := Produce("B")
            // N := A | B
            // S := N N END
            var target = Earley<string>(symbols =>
            {
                var rulea = Produce(() => "A").Named("A");
                var ruleb = Produce(() => "B").Named("B");

                var nullable = symbols.New<string>("N")
                    .AddProduction(rulea, a => a)
                    .AddProduction(ruleb, b => b);

                var eof = If(End(), Produce(() => true)).Named("END");
                return symbols.New("S")
                    .AddProduction(nullable, nullable, eof, (first, second, end) => $"[{first}:{second}]");
            });

            // A:A A:B B:A B:B

            var results = target.Parse("");
            results.Success.Should().BeTrue();
            results.Results.Count.Should().Be(4);
            var values = results.Results.Where(r => r.Success).Select(r => r.Value).ToList();
            values.Should().Contain("[A:A]");
            values.Should().Contain("[A:B]");
            values.Should().Contain("[B:A]");
            values.Should().Contain("[B:B]");
        }

        // TODO: More testing of multiple sequential nullables, including in the context of
        // other non-nullable rules (before and after). Need to really stress-test the Aycock fix.

        [Test]
        public void RightRecurse_ContinueWith_Single()
        {
            // E ::= a
            //     | a E
            var target = Earley<string>(symbols =>
            {
                var a = Match('a').Named("a");

                var e = symbols.New("E")
                    .AddProduction(a, _ => "a");
                e.AddProduction(a, e, (_, rr) => "a" + rr);
                return e;
            });

            var parser = target.ContinueWith(left =>
                Rule(
                    left,
                    Any().ListCharToString(),
                    (l, rem) => $"({l})({rem})"
                )
            );

            var result = parser.Parse("aaaaa");
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(5);
            var values = result.Results.Where(r => r.Success).Select(r => r.Value).ToList();
            values.Should().Contain("(a)(aaaa)");
            values.Should().Contain("(aa)(aaa)");
            values.Should().Contain("(aaa)(aa)");
            values.Should().Contain("(aaaa)(a)");
            values.Should().Contain("(aaaaa)()");
        }

        [Test]
        public void RightRecurse_ContinueWith_Multi()
        {
            // E ::= a
            //     | a E
            var parser = Earley<string>(symbols =>
            {
                var a = Match('a').Named("a");

                var e = symbols.New("E")
                    .AddProduction(a, _ => "a");
                e.AddProduction(a, e, (_, rr) => "a" + rr);
                return e;
            });

            // Start :: = left E
            // E ::= a
            //     | a E
            var target = parser.ContinueWith(left => Earley<string>(symbols =>
            {
                var start = symbols.New("Start");
                var a = Match('a').Named("a");
                var e = symbols.New("E")
                    .AddProduction(a, _ => "a");
                e.AddProduction(a, e, (_, rr) => "a" + rr);

                start.AddProduction(left, e, (l, rr) => $"({l})({rr})");
                return start;
            }));

            var result = target.Parse("aaaaa");
            result.Success.Should().BeTrue();
            var values = result.Results.Where(r => r.Success).Select(r => r.Value).ToList();
            values.Count.Should().Be(10);
            values.Should().Contain("(a)(a)");
            values.Should().Contain("(a)(aa)");
            values.Should().Contain("(a)(aaa)");
            values.Should().Contain("(a)(aaaa)");
            values.Should().Contain("(aa)(a)");
            values.Should().Contain("(aa)(aa)");
            values.Should().Contain("(aa)(aa)");
            values.Should().Contain("(aaa)(a)");
            values.Should().Contain("(aaa)(aa)");
            values.Should().Contain("(aaaa)(a)");
        }

        [TestCase("a", 1)]
        [TestCase("aa", 2)]
        [TestCase("aaa", 3)]
        [TestCase("aaaa", 4)]
        [TestCase("aaaaa", 5)]
        [TestCase("aaaaaa", 6)]
        [TestCase("aaaaaaa", 7)]
        [TestCase("aaaaaaaa", 8)]
        [TestCase("aaaaaaaaa", 9)]
        public void ProductionRules_Tests(string pattern, int expected)
        {
            var target = Earley<int>(symbols =>
            {
                var a = Match('a').Named("a");

                var e = symbols.New<int>("E")
                    .AddProduction(a, _ => 1)
                    .AddProduction(a, a, (_, _) => 2)
                    .AddProduction(a, a, a, (_, _, _) => 3)
                    .AddProduction(a, a, a, a, (_, _, _, _) => 4)
                    .AddProduction(a, a, a, a, a, (_, _, _, _, _) => 5)
                    .AddProduction(a, a, a, a, a, a, (_, _, _, _, _, _) => 6)
                    .AddProduction(a, a, a, a, a, a, a, (_, _, _, _, _, _, _) => 7)
                    .AddProduction(a, a, a, a, a, a, a, a, (_, _, _, _, _, _, _, _) => 8)
                    .AddProduction(a, a, a, a, a, a, a, a, a, (_, _, _, _, _, _, _, _, _) => 9)
                    ;

                var eof = IsEnd().Named("END");
                return symbols.New("S")
                    .AddProduction(e, eof, (v, _) => v);
            });

            var result = target.Parse(pattern);
            result.Success.Should().Be(true);
            result.Results.Count.Should().Be(1);
            result.Results.Single().Value.Should().Be(expected);
        }

        [Test]
        public void Bnf_Test()
        {
            var target = Earley<int>(symbols =>
            {
                var plus = Match('+').Named("'+'");
                var star = Match('*').Named("'*'");

                var expr = symbols.New<int>("Expr")
                    .AddProduction(UnsignedInteger().Named("literal"));

                expr.AddProduction(expr, plus, expr, (l, _, r) => l + r);
                expr.AddProduction(expr, star, expr, (l, _, r) => l * r);

                var eof = If(End(), Produce(() => true)).Named("End");

                return symbols.New("S")
                    .AddProduction(expr, eof, (e, _) => e);
            });

            var result = target.ToBnf();
            result.Should().Contain("Expr := <literal>");
            result.Should().Contain("Expr := <Expr> <'+'> <Expr>");
            result.Should().Contain("Expr := <Expr> <'*'> <Expr>");
            result.Should().Contain("S := <Expr> <End>");
        }

        [Test]
        public void GetChildren_Test()
        {
            var target = Earley<int>(symbols =>
            {
                var plus = Match('+').Named("'+'");
                var star = Match('*').Named("'*'");

                var expr = symbols.New<int>("Expr")
                    .AddProduction(UnsignedInteger().Named("literal"), n => n);

                expr.AddProduction(expr, plus, expr, (l, _, r) => l + r);
                expr.AddProduction(expr, star, expr, (l, _, r) => l * r);

                var eof = If(End(), Produce(() => true)).Named("End");

                return symbols.New("S")
                    .AddProduction(expr, eof, (e, _) => e);
            });

            var result = target.GetChildren().ToList();
            var names = result.Select(r => r.Name).ToList();
            names.Should().Contain("literal");
            names.Should().Contain("'+'");
            names.Should().Contain("'*'");
            names.Should().Contain("End");
        }
    }
}

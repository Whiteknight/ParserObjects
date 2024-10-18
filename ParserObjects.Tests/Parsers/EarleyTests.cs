using System.Linq;
using ParserObjects;
using ParserObjects.Earley;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers.C;
using static ParserObjects.Parsers.Digits;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public class EarleyTests
{
    [Test]
    public void SingleTerminal_Test()
    {
        var target = Earley<int>(symbols => symbols
            .New("S")
            .Rule(UnsignedInteger(), n => (int)n)
        );

        var result = target.Parse("4");
        result.Success.Should().BeTrue();
        result.Results.Count.Should().Be(1);
        result.Results[0].Value.Should().Be(4);
    }

    [Test]
    public void SingleTerminal_Unnamed()
    {
        var target = Earley<int>(symbols => symbols
            .New()
            .Rule(UnsignedInteger(), n => (int)n)
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
                .Rule(UnsignedInteger().Named("literal"), n => (int)n);

            expr.Rule(expr, plus, expr, (l, _, r) => l + r);
            expr.Rule(expr, star, expr, (l, _, r) => l * r);

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
    public void BasicExpression_Statistics()
    {
        var target = Earley<int>(
            symbols =>
            {
                var plus = Match('+').Named("plus");
                var star = Match('*').Named("star");

                var expr = symbols.New("Expr")
                    .Rule(UnsignedInteger().Named("literal"), n => (int)n);

                expr.Rule(expr, plus, expr, (l, _, r) => l + r);
                expr.Rule(expr, star, expr, (l, _, r) => l * r);

                return expr;
            }
        );

        var result = target.Parse("4*5+6");
        result.Success.Should().BeTrue();
        var statistics = result.TryGetData<IParseStatistics>().Value;

        statistics.CreatedItems.Should().Be(54);
        statistics.NumberOfStates.Should().Be(6);

        statistics.ScannedSuccess.Should().Be(6);

        statistics.PredictedItems.Should().Be(6);
        statistics.PredictedByCompletedNullable.Should().Be(0);

        statistics.CompletedNullables.Should().Be(0);
        statistics.CompletedParentItem.Should().Be(18);

        statistics.ProductionRuleAttempts.Should().Be(7);
        statistics.ProductionRuleSuccesses.Should().Be(7);
        statistics.DerivationCacheHit.Should().Be(5);
        statistics.DerivationSingleSymbolShortCircuits.Should().Be(3);
        statistics.ItemsWithSingleDerivation.Should().Be(0);
        statistics.ItemsWithZeroDerivations.Should().Be(0);
    }

    [Test]
    public void BasicExpression_EOF()
    {
        var target = Earley<int>(symbols =>
        {
            var plus = Match('+').Named("'+'");
            var star = Match('*').Named("'*'");

            var expr = symbols.New<int>("Expr")
                .Rule(UnsignedInteger().Named("literal"), n => (int)n);

            expr.Rule(expr, plus, expr, (l, _, r) => l + r);
            expr.Rule(expr, star, expr, (l, _, r) => l * r);

            var eof = IsEnd();

            return symbols.New("S")
                .Rule(expr, eof, (e, _) => e);
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
    public void BasicExpression_Precedence()
    {
        var target = Earley<(string, int)>(symbols =>
        {
            var plus = Match('+').Named("'+'");
            var minus = Match('-').Named("'-'");
            var star = Match('*').Named("'*'");
            var divide = Match('/').Named("'/'");
            var number = Integer().Named("literal");

            var primary = symbols.New("Primary")
                .Rule(number, n => (n.ToString(), n));

            var term = symbols.New<(string, int)>("Term");
            term.Rule(primary, p => p);
            term.Rule(term, star, primary, (l, _, r) => ($"({l.Item1}*{r.Item1})", l.Item2 * r.Item2));
            term.Rule(term, divide, primary, (l, _, r) => ($"({l.Item1}/{r.Item1})", l.Item2 / r.Item2));

            var expr = symbols.New<(string, int)>("Expr");
            expr.Rule(term, t => t);
            expr.Rule(expr, plus, term, (l, _, r) => ($"({l.Item1}+{r.Item1})", l.Item2 + r.Item2));
            expr.Rule(expr, minus, term, (l, _, r) => ($"({l.Item1}-{r.Item1})", l.Item2 - r.Item2));

            var eof = IsEnd();

            return symbols.New("S")
                .Rule(expr, eof, (e, _) => e);
        });

        var result = target.Parse("4*5+6/3-2");
        result.Success.Should().BeTrue();
        result.Results.Count.Should().Be(1);
        result.Results[0].Value.Item2.Should().Be(20); // (4*5)+6
    }

    [Test]
    public void BasicExpression_MultiCharItems()
    {
        var target = Earley<int>(symbols =>
        {
            var plus = CharacterString("+++").Named("plus");
            var star = CharacterString("**").Named("star");

            var expr = symbols.New("Expr")
                .Rule(UnsignedInteger().Named("literal"), n => (int)n);

            expr.Rule(expr, plus, expr, (l, _, r) => l + r);
            expr.Rule(expr, star, expr, (l, _, r) => l * r);
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
                .Rule(UnsignedInteger().Named("literal"), n => (int)n);

            expr.Rule(expr, plus, expr, (l, _, r) => l + r);
            expr.Rule(expr, star, expr, (l, _, r) => l * r);
            return expr;
        });

        var target = parser.Select(args =>
        {
            var best = args.Result.Results.Where(alt => alt.Value % 2 == 1).OrderByDescending(alt => alt.Value).FirstOrDefault();
            if (best == null)
                return args.Failure();
            return args.Success(best);
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
                .Rule(empty, v => v);
            expr.Rule(expr, Match('a'), (count, _) => count + 1);

            var eof = IsEnd();

            return symbols.New("Start")
                .Rule(expr, eof, (v, _) => v);
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
    public void LeftRecurse_Statistics()
    {
        var target = Earley<int>(
            symbols =>
            {
                var empty = Produce(() => 0);

                var expr = symbols.New<int>("Expr")
                    .Rule(empty, v => v);
                expr.Rule(expr, Match('a'), (count, _) => count + 1);

                var eof = IsEnd();

                return symbols.New("Start")
                    .Rule(expr, eof, (v, _) => v);
            }
        );

        var result = target.Parse("");
        result.Success.Should().BeTrue();
        var statistics = result.TryGetData<IParseStatistics>().Value;

        statistics.ScannedSuccess.Should().Be(2);

        statistics.PredictedItems.Should().Be(2);
        statistics.PredictedByCompletedNullable.Should().Be(0);

        statistics.CompletedNullables.Should().Be(2);
        statistics.CompletedParentItem.Should().Be(2);

        statistics.ProductionRuleAttempts.Should().Be(2);
        statistics.ProductionRuleSuccesses.Should().Be(2);
        statistics.DerivationCacheHit.Should().Be(0);
        statistics.DerivationSingleSymbolShortCircuits.Should().Be(1);
        statistics.ItemsWithSingleDerivation.Should().Be(0);
        statistics.ItemsWithZeroDerivations.Should().Be(0);
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
                .Rule(a, _ => 1);
            e.Rule(a, e, (_, count) => count + 1);

            var eof = IsEnd().Named("End");

            return symbols.New("S")
                .Rule(e, eof, (count, _) => count).Named("S");
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
    public void RightRecurse_Statistics()
    {
        var target = Earley<int>(
            symbols =>
            {
                var a = Match('a').Named("a");

                var e = symbols.New<int>("E")
                    .Rule(a, _ => 1);
                e.Rule(a, e, (_, count) => count + 1);

                var eof = IsEnd().Named("End");

                return symbols.New("S")
                    .Rule(e, eof, (count, _) => count).Named("S");
            }
        );

        // TODO: Need to look at the State listings to verify if we have Leo's optimization
        // or not.

        var result = target.Parse("a");
        result.Success.Should().BeTrue();
        var statistics = result.TryGetData<IParseStatistics>().Value;

        statistics.ScannedSuccess.Should().Be(3);

        statistics.PredictedItems.Should().Be(4);
        statistics.PredictedByCompletedNullable.Should().Be(0);

        statistics.CompletedNullables.Should().Be(0);
        statistics.CompletedParentItem.Should().Be(1);

        statistics.ProductionRuleAttempts.Should().Be(2);
        statistics.ProductionRuleSuccesses.Should().Be(2);
        statistics.DerivationCacheHit.Should().Be(0);
        statistics.DerivationSingleSymbolShortCircuits.Should().Be(1);
        statistics.ItemsWithSingleDerivation.Should().Be(0);
        statistics.ItemsWithZeroDerivations.Should().Be(0);
    }

    [Test]
    public void Nullable_Test()
    {
        var target = Earley<string>(symbols =>
        {
            // Nullable is a production rule which consumes no input
            var nullable = Produce(() => "Test");
            var eof = IsEnd();
            return symbols.New("S")
                .Rule(nullable, eof, (a, _) => a);
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
                .Rule(rulea, ruleb, (a, b) => $"({a},{b})");

            var eof = IsEnd().Named("END");
            return symbols.New("S")
                .Rule(nullable, nullable, eof, (first, second, _) => $"[{first}:{second}]");
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
                .Rule(rulea, a => a)
                .Rule(ruleb, b => b);

            var eof = IsEnd().Named("END");
            return symbols.New("S")
                .Rule(nullable, nullable, eof, (first, second, _) => $"[{first}:{second}]");
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
                .Rule(a, _ => "a");
            e.Rule(a, e, (_, rr) => "a" + rr);
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
                .Rule(a, _ => "a");
            e.Rule(a, e, (_, rr) => "a" + rr);
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
                .Rule(a, _ => "a");
            e.Rule(a, e, (_, rr) => "a" + rr);

            start.Rule(left, e, (l, rr) => $"({l})({rr})");
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
                .Rule(a, _ => 1)
                .Rule(a, a, (_, _) => 2)
                .Rule(a, a, a, (_, _, _) => 3)
                .Rule(a, a, a, a, (_, _, _, _) => 4)
                .Rule(a, a, a, a, a, (_, _, _, _, _) => 5)
                .Rule(a, a, a, a, a, a, (_, _, _, _, _, _) => 6)
                .Rule(a, a, a, a, a, a, a, (_, _, _, _, _, _, _) => 7)
                .Rule(a, a, a, a, a, a, a, a, (_, _, _, _, _, _, _, _) => 8)
                .Rule(a, a, a, a, a, a, a, a, a, (_, _, _, _, _, _, _, _, _) => 9)
                ;

            var eof = IsEnd().Named("END");
            return symbols.New("S")
                .Rule(e, eof, (v, _) => v);
        });

        var result = target.Parse(pattern);
        result.Success.Should().Be(true);
        result.Results.Count.Should().Be(1);
        result.Results.Single().Value.Should().Be(expected);
    }

    [Test]
    public void ToBnf_Test()
    {
        var target = Earley<int>(symbols =>
        {
            var plus = Match('+').Named("'+'");
            var star = Match('*').Named("'*'");

            var expr = symbols.New<int>("Expr")
                .Rule(Integer().Named("literal"));

            expr.Rule(expr, plus, expr, (l, _, r) => l + r);
            expr.Rule(expr, star, expr, (l, _, r) => l * r);

            var eof = IsEnd().Named("End");

            return symbols.New("S")
                .Rule(expr, eof, (e, _) => e);
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
                .Rule(UnsignedInteger().Named("literal"), n => (int)n);

            expr.Rule(expr, plus, expr, (l, _, r) => l + r);
            expr.Rule(expr, star, expr, (l, _, r) => l * r);

            var eof = IsEnd().Named("End");

            return symbols.New("S")
                .Rule(expr, eof, (e, _) => e);
        });

        var result = target.GetChildren().ToList();
        var names = result.Select(r => r.Name).ToList();
        names.Should().Contain("literal");
        names.Should().Contain("'+'");
        names.Should().Contain("'*'");
        names.Should().Contain("End");
    }

    [Test]
    public void Production_Throws()
    {
        var target = Earley<int>(symbols =>
        {
            return symbols.New<int>("E")
                .Rule(Match('A'), (_) => throw new System.Exception("FAIL"));
        });

        var result = target.Parse("");
    }

    [Test]
    public void Examine_ResultRecreate()
    {
        var target = Examine(
            Earley<char>(symbols => symbols
                .New("S")
                .Rule(Digit(), n => n)
            ),
            c => c.Input.GetNext(),
            c => c.Input.GetNext()
        );

        var result = target.Parse("456");
        result.Success.Should().BeTrue();
        result.Results.Count.Should().Be(1);
        result.Results[0].Value.Should().Be('5');
        result.Results[0].Consumed.Should().Be(3);
    }

    [Test]
    public void BasicExpression_NestedMultiParser()
    {
        var inner = Earley<int>(symbols =>
        {
            var plus = Match('+').Named("plus");
            var expr = symbols.New("Expr")
                .Rule(UnsignedInteger().Named("literal"), n => (int)n);
            expr.Rule(expr, plus, expr, (l, _, r) => l + r);
            return expr;
        }).Named("inner");
        var target = Earley<int>(symbols =>
        {
            var star = Match('*').Named("star");

            var expr = symbols.New("Expr");
            expr.Rule(inner, i => i);
            expr.Rule(expr, star, expr, (l, _, r) => l * r);

            return expr;
        });

        var result = target.Parse("4*5+6");
        result.Success.Should().BeTrue();
        result.Results.Count.Should().Be(3);

        var values = result.Results.Where(r => r.Success).Select(r => r.Value).ToList();

        values.Should().Contain(4);  // 4 = 4
        values.Should().Contain(20); // 4*5 = 20
                                     // We cannot get the value (4*5)+6 = 26 because we recurse to inner for the "5 +"
        values.Should().Contain(44); // 4*(5+6) = 44
    }

    [Test]
    public void TwoSymbolsWithSameName()
    {
        var act = () => Earley<string>(symbols =>
        {
            var a = symbols.New("test");
            var b = symbols.New("test");
            return b;
        });

        act.Should().Throw<GrammarException>();
    }

    [Test]
    public void GetNewName_Search()
    {
        Earley<string>(symbols =>
        {
            // Get a symbol with an automatically-generated name, so we can see where the
            // counter is
            var a = symbols.New();

            var currentInt = int.Parse(a.Name.Substring(2));

            // now allocate a new symbol with what should be the next auto-generated value:
            var b = symbols.New("_S" + (currentInt + 1));

            // Now try to allocate a third symbol with an auto-generated name. The system will
            // try the name we created for b, and then search for a better one
            var c = symbols.New();

            // This whole thing shouldn't throw an exception, so we need to set up a simple
            // production here just to avoid that check.
            c.Rule(Produce(() => "X"), x => x);
            return c;
        });
    }
}

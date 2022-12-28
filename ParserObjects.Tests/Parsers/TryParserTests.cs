using ParserObjects.Internal.Utility;
using ParserObjects.Pratt;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public static class TryParserTests
{
    public class Output
    {
        [Test]
        public void Parse_Test()
        {
            var target = Try(Any());
            var result = target.Parse("abc");
            result.Success.Should().BeTrue();
            result.Value.Should().Be('a');
            result.Consumed.Should().Be(1);
            var ex = result.TryGetData<Exception>();
            ex.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Fail()
        {
            var target = Try(Fail());
            var result = target.Parse("abc");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
            var ex = result.TryGetData<Exception>();
            ex.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Throw()
        {
            var target = Try(Produce<char>(() => throw new Exception("test")));
            var result = target.Parse("abc");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
            var ex = result.TryGetData<Exception>();
            ex.Success.Should().BeTrue();
            ex.Value.Message.Should().Be("test");
        }

        [Test]
        public void Parse_Throw_ExamineBubble()
        {
            Exception exception = null;
            var inner = Produce<char>(() => throw new Exception("test"));
            var target = Try(inner, bubble: true, examine: ex => exception = ex);
            var act = () => target.Parse("abc");
            act.Should().Throw<Exception>();
            exception.Should().NotBeNull();
            exception.Message.Should().Be("test");
        }

        [Test]
        public void Parse_Pratt_ControlFlowException()
        {
            // Parser parses "a", then recurses self->try->self to parse "b"
            // Callback for "b" throws a control flow exception
            // Try parser should ignore the ControlFlowException, which will bubble up and
            // cause the whole Pratt to fail.
            var target = Pratt<string>(c => c
                .Add(Match('a'), p => p
                    .Bind((ctx, _) =>
                    {
                        var next = ctx.TryParse(Try(ctx));
                        return "a" + next.GetValueOrDefault("fail");
                    })
                )
                .Add(Match('b'), p => p
                    .Bind((ctx, _) =>
                    {
                        ctx.FailAll();
                        return "b";
                    })
                )
            );

            var result = target.Parse("ab");
            result.Success.Should().BeFalse();
            var ex = result.TryGetData<Exception>();
            ex.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Pratt_Exception()
        {
            // Parser parses "a", then recurses self->try->self to parse "b"
            // Callback for "b" throws a regular exception
            // Try parser should catch this exception, return failure
            // The Pratt will succeed, with "afail"
            var target = Pratt<string>(c => c
                .Add(Match('a'), p => p
                    .Bind((ctx, _) =>
                    {
                        var next = ctx.TryParse(Try(ctx));
                        return "a" + next.GetValueOrDefault("fail");
                    })
                )
                .Add(Match('b'), p => p
                    .Bind((_, _) =>
                    {
                        throw new Exception("test");
                    })
                )
            );

            var result = target.Parse("ab");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("afail");
        }

        [Test]
        public void Match_ControlFlowException()
        {
            // ControlFlowExceptions are used internally, and it's generally a bad idea to use them
            // in downstream code, but it's also hard to set up a test to prove that Try() does not
            // catch or interfere with them otherwise.
            var target = Try(Function<char>(_ => throw new ControlFlowException("test"), _ => throw new ControlFlowException("test")));
            var act = () => target.Match("ab");
            act.Should().Throw<ControlFlowException>();
        }

        [Test]
        public void Match_Throw_ExamineBubble()
        {
            Exception exception = null;
            var inner = Function<char>(_ => throw new Exception("test"), _ => throw new Exception("test"));
            var target = Try(inner, bubble: true, examine: ex => exception = ex);
            var act = () => target.Match("abc");
            act.Should().Throw<Exception>();
            exception.Should().NotBeNull();
            exception.Message.Should().Be("test");
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = Try(Any());
            var result = target.ToBnf();
            result.Should().Contain("(TARGET) := TRY .");
        }
    }

    public class NoOutput
    {
        [Test]
        public void Parse_Test()
        {
            var target = Try(Empty());
            var result = target.Parse("abc");
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(0);
            var ex = result.TryGetData<Exception>();
            ex.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Fail()
        {
            var target = Try(End());
            var result = target.Parse("abc");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
            var ex = result.TryGetData<Exception>();
            ex.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Throw()
        {
            var inner = (IParser<char>)Produce<char>(() => throw new Exception("test"));
            var target = Try(inner);
            var result = target.Parse("abc");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
            var ex = result.TryGetData<Exception>();
            ex.Success.Should().BeTrue();
            ex.Value.Message.Should().Be("test");
        }

        [Test]
        public void Parse_Throw_ExamineBubble()
        {
            Exception exception = null;
            var inner = (IParser<char>)Produce<char>(() => throw new Exception("test"));
            var target = Try(inner, bubble: true, examine: ex => exception = ex);
            var act = () => target.Parse("abc");
            act.Should().Throw<Exception>();
            exception.Should().NotBeNull();
            exception.Message.Should().Be("test");
        }

        [Test]
        public void Parse_ControlFlowException()
        {
            // ControlFlowExceptions are used internally, and it's generally a bad idea to use them
            // in downstream code, but it's also hard to set up a test to prove that Try() does not
            // catch or interfere with them otherwise.
            var target = Try((IParser<char>)Produce<char>(() => throw new ControlFlowException("fail")));
            var act = () => target.Parse("ab");
            act.Should().Throw<ControlFlowException>();
        }

        [Test]
        public void Match_Test()
        {
            var target = Try(Empty());
            var result = target.Match("abc");
            result.Should().BeTrue();
        }

        [Test]
        public void Match_Fail()
        {
            var target = Try(End());
            var result = target.Match("abc");
            result.Should().BeFalse();
        }

        [Test]
        public void Match_Throw()
        {
            var inner = (IParser<char>)Function<char>(_ => throw new Exception("test"), _ => throw new Exception("test"));
            var target = Try(inner);
            var result = target.Match("abc");
            result.Should().BeFalse();
        }

        [Test]
        public void Match_Throw_ExamineBubble()
        {
            Exception exception = null;
            var inner = (IParser<char>)Function<char>(_ => throw new Exception("test"), _ => throw new Exception("test"));
            var target = Try(inner, bubble: true, examine: ex => exception = ex);
            var act = () => target.Match("abc");
            act.Should().Throw<Exception>();
            exception.Should().NotBeNull();
            exception.Message.Should().Be("test");
        }

        [Test]
        public void Match_ControlFlowException()
        {
            // ControlFlowExceptions are used internally, and it's generally a bad idea to use them
            // in downstream code, but it's also hard to set up a test to prove that Try() does not
            // catch or interfere with them otherwise.
            var target = Try((IParser<char>)Function<char>(_ => throw new ControlFlowException("test"), _ => throw new ControlFlowException("test")));
            var act = () => target.Match("ab");
            act.Should().Throw<ControlFlowException>();
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = Try(End());
            var result = target.ToBnf();
            result.Should().Contain("(TARGET) := TRY END");
        }
    }

    public class Multi
    {
        [Test]
        public void Parse_Test()
        {
            var target = Try(ProduceMulti(() => new[] { 'a', 'b', 'c' }));
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Results[0].Value.Should().Be('a');
            var ex = result.TryGetData<Exception>();
            ex.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Throw()
        {
            var target = Try(ProduceMulti<char>(() => throw new Exception("test")));
            var result = target.Parse("");
            result.Success.Should().BeFalse();
            var ex = result.TryGetData<Exception>();
            ex.Success.Should().BeTrue();
            ex.Value.Message.Should().Be("test");
        }

        [Test]
        public void Parse_Throw_ExamineBubble()
        {
            Exception exception = null;
            var inner = ProduceMulti<char>(() => throw new Exception("test"));
            var target = Try(inner, bubble: true, examine: ex => exception = ex);
            var act = () => target.Parse("abc");
            act.Should().Throw<Exception>();
            exception.Should().NotBeNull();
            exception.Message.Should().Be("test");
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = Try(ProduceMulti(() => new[] { 'a', 'b', 'c' }));
            var result = target.ToBnf();
            result.Should().Contain("(TARGET) := TRY PRODUCE");
        }
    }
}

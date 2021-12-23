using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class TryParserTests
    {
        [Test]
        public void Parse_Output_Test()
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
        public void Parse_Untyped_Test()
        {
            var target = Try(Empty());
            var result = target.Parse("abc");
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(0);
            var ex = result.TryGetData<Exception>();
            ex.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Output_Fail()
        {
            var target = Try(Fail());
            var result = target.Parse("abc");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
            var ex = result.TryGetData<Exception>();
            ex.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Untyped_Fail()
        {
            var target = Try(End());
            var result = target.Parse("abc");
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
            var ex = result.TryGetData<Exception>();
            ex.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Output_Throw()
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
        public void Parse_Untyped_Throw()
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
        public void Parse_Output_Pratt_ControlFlowException()
        {
            // Parser parses "a", then recurses self->try->self to parse "b"
            // Callback for "b" throws a control flow exception
            // Try parser should ignore the ControlFlowException, which will bubble up and
            // cause the whole Pratt to fail.
            var target = Pratt<string>(c => c
                .Add(Match('a'), p => p
                    .ProduceRight((ctx, _) =>
                    {
                        var next = ctx.TryParse(Try(ctx));
                        return "a" + next.GetValueOrDefault("fail");
                    })
                )
                .Add(Match('b'), p => p
                    .ProduceRight((ctx, _) =>
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
        public void Parse_Output_Pratt_Exception()
        {
            // Parser parses "a", then recurses self->try->self to parse "b"
            // Callback for "b" throws a regular exception
            // Try parser should catch this exception, return failure
            // The Pratt will succeed, with "afail"
            var target = Pratt<string>(c => c
                .Add(Match('a'), p => p
                    .ProduceRight((ctx, _) =>
                    {
                        var next = ctx.TryParse(Try(ctx));
                        return "a" + next.GetValueOrDefault("fail");
                    })
                )
                .Add(Match('b'), p => p
                    .ProduceRight((_, _) =>
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
        public void Parse_Multi_Output_Test()
        {
            var target = Try(ProduceMulti(() => new[] { 'a', 'b', 'c' }));
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Results[0].Value.Should().Be('a');
            var ex = result.TryGetData<Exception>();
            ex.Success.Should().BeFalse();
        }

        [Test]
        public void Parse_Multi_Output_Throw()
        {
            var target = Try(ProduceMulti<char>(() => throw new Exception("test")));
            var result = target.Parse("");
            result.Success.Should().BeFalse();
            var ex = result.TryGetData<Exception>();
            ex.Success.Should().BeTrue();
            ex.Value.Message.Should().Be("test");
        }

        [Test]
        public void ToBnf_SingleOutput()
        {
            var target = Try(Any());
            var result = target.ToBnf();
            result.Should().Contain("(TARGET) := TRY .");
        }

        [Test]
        public void ToBnf_SingleNoOutput()
        {
            var target = Try(End());
            var result = target.ToBnf();
            result.Should().Contain("(TARGET) := TRY END");
        }

        [Test]
        public void ToBnf_MultiOutput()
        {
            var target = Try(ProduceMulti(() => new[] { 'a', 'b', 'c' }));
            var result = target.ToBnf();
            result.Should().Contain("(TARGET) := TRY PRODUCE");
        }
    }
}

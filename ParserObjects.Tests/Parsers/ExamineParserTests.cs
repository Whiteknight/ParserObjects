using System.Linq;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Parsers
{
    public class ExamineParserTests
    {
        [Test]
        public void Output_Method_Test()
        {
            char before = '\0';
            string after = "";
            var parser = Examine(Any(), s => before = s.Input.Peek(), s => after = $"{s.Result.Value}{s.Input.Peek()}");
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            before.Should().Be('a');
            after.Should().Be("ab");
        }

        [Test]
        public void Output_Method_Consume()
        {
            char before = '\0';
            string after = "";
            var parser = Examine(Any(), s => before = s.Input.GetNext(), s => after = $"{s.Result.Value}{s.Input.Peek()}");
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            result.Consumed.Should().Be(2);
            before.Should().Be('a');
            result.Value.Should().Be('b');
            after.Should().Be("bc");
        }

        [Test]
        public void Multi_Output_Method_Consume()
        {
            char before = '\0';
            char after = '\0';
            var parser = Examine(ProduceMulti(() => new[] { 'b' }),
                s => before = s.Input.GetNext(),
                s => after = s.Input.GetNext()
            );
            var input = FromString("ac");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            before.Should().Be('a');
            result.Results[0].Value.Should().Be('b');
            after.Should().Be('c');
        }

        [Test]
        public void Multi_Output_Method_Consume_Fail()
        {
            char before = '\0';
            char after = '\0';
            var parser = Examine(FailMulti<char>(),
                s => before = s.Input.GetNext(),
                s => after = s.Input.GetNext()
            );
            var input = FromString("ac");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            before.Should().Be('a');
            after.Should().Be('c');
        }

        [Test]
        public void Output_Method_Consume_Fail()
        {
            char before = '\0';
            char after = '\0';
            var parser = Examine(Fail(), s => before = s.Input.GetNext(), s => after = s.Input.Peek());
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            result.Consumed.Should().Be(0);
            before.Should().Be('a');
            after.Should().Be('b');
        }

        [Test]
        public void Output_Method_Fail()
        {
            char before = '\0';
            char after = '\0';
            var parser = Examine(Fail(), s => before = s.Input.Peek(), s => after = s.Input.Peek());
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeFalse();
            before.Should().Be('a');
            after.Should().Be('a');
        }

        [Test]
        public void Output_Extension_Test()
        {
            char before = '\0';
            char after = '\0';
            var parser = Any().Examine(s => before = s.Input.Peek(), s => after = s.Input.Peek());
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            before.Should().Be('a');
            after.Should().Be('b');
        }

        [Test]
        public void Output_GetChildren_Test()
        {
            char before = '\0';
            string after = "";
            var anyParser = Any();
            var parser = Examine(anyParser, s => before = s.Input.Peek(), s => after = $"{s.Result.Value}{s.Input.Peek()}");
            var children = parser.GetChildren();
            children.Count().Should().Be(1);
            children.Single().Should().BeSameAs(anyParser);
        }

        [Test]
        public void NoOutput_Method_Test()
        {
            char before = '\0';
            string after = "";
            var parser = Examine((IParser<char>)Any(), s => before = s.Input.Peek(), s => after = $"{s.Result.Value}{s.Input.Peek()}");
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            before.Should().Be('a');
            after.Should().Be("ab");
        }

        [Test]
        public void NoOutput_Extension_Test()
        {
            char before = '\0';
            char after = '\0';
            var parser = ((IParser<char>)Any()).Examine(s => before = s.Input.Peek(), s => after = s.Input.Peek());
            var input = FromString("abc");
            var result = parser.Parse(input);
            result.Success.Should().BeTrue();
            before.Should().Be('a');
            after.Should().Be('b');
        }

        [Test]
        public void NoOutput_GetChildren_Test()
        {
            char before = '\0';
            string after = "";
            var anyParser = (IParser<char>)Any();
            var parser = Examine(anyParser, s => before = s.Input.Peek(), s => after = $"{s.Result.Value}{s.Input.Peek()}");
            var children = parser.GetChildren();
            children.Count().Should().Be(1);
            children.Single().Should().BeSameAs(anyParser);
        }

        [Test]
        public void ToBnf_Single_NoOutput()
        {
            var parser = Examine(End()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := END");
        }

        [Test]
        public void ToBnf_Single_Output()
        {
            var parser = Examine(Any()).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := .");
        }

        [Test]
        public void ToBnf_Multi_Output()
        {
            var parser = Examine(ProduceMulti(() => new char[0])).Named("parser");
            var result = parser.ToBnf();
            result.Should().Contain("parser := PRODUCE");
        }
    }
}

using System.Collections.Generic;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public static class WithDataContextTests
{
    public class SingleNoArgs
    {
        [Test]
        public void Parse_Test()
        {
            var target = Rule(
                SetData("test", "value1"),
                Rule(
                    SetData("test", "value2"),
                    GetData<string>("test"),
                    (a, b) => b
                ).WithDataContext(),
                GetData<string>("test"),
                (a, b, c) => $"{a}-{b}-{c}"
            );
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("value1-value2-value1");
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = Any().WithDataContext().Named("SUT");
            var result = target.ToBnf();
            result.Should().Contain("SUT := .");
        }
    }

    public class SingleOneKeyValue
    {
        [Test]
        public void Parse_SingleValue()
        {
            var target = Rule(
                SetData("test", "value1"),
                GetData<string>("test").WithDataContext("test", "value2"),
                GetData<string>("test"),
                (a, b, c) => $"{a}-{b}-{c}"
            );
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("value1-value2-value1");
        }
    }

    public class SingleDictionary
    {
        [Test]
        public void Parse_DictValue()
        {
            var target = Rule(
                SetData("test", "value1"),
                GetData<string>("test").WithDataContext(new Dictionary<string, object>
                {
                    { "test", "value2" }
                }),
                GetData<string>("test"),
                (a, b, c) => $"{a}-{b}-{c}"
            );
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("value1-value2-value1");
        }
    }

    public class MultiNoArgs
    {
        [Test]
        public void Parse_Test()
        {
            var target = ProduceMulti(() => "abc").WithDataContext();
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            // TODO: Need to find a better way to test this.
        }
    }

    public class MultiDictionary
    {
        [Test]
        public void Parse_Test()
        {
            var target = ProduceMulti(s => s.Data.Get<string[]>("test").Value)
                .WithDataContext(new Dictionary<string, object>
                {
                    { "test", new[] { "a", "b", "c" } }
                });
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Results[0].Value.Should().Be("a");
            result.Results[1].Value.Should().Be("b");
            result.Results[2].Value.Should().Be("c");
        }
    }

    public class MultiOneKeyValue
    {
        [Test]
        public void Parse_Test()
        {
            var target = ProduceMulti(s => s.Data.Get<string[]>("test").Value)
                .WithDataContext("test", new[] { "a", "b", "c" });
            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Results[0].Value.Should().Be("a");
            result.Results[1].Value.Should().Be("b");
            result.Results[2].Value.Should().Be("c");
        }
    }
}

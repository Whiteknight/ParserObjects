using System.Collections.Generic;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers;

public static class DataContextTests
{
    public class Single
    {
        [Test]
        public void Parse_Test()
        {
            var target = DataContext(
                Rule(
                    SetData("test", "value"),
                    GetData<string>("test"),
                    (set, get) => set + get
                )
            );

            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("valuevalue");
        }

        [Test]
        public void Parse_ValuesDictionary()
        {
            var target = DataContext(
                Rule(
                    GetData<string>("a"),
                    GetData<string>("b"),
                    (a, b) => a + b
                ),
                new Dictionary<string, object>
                {
                    { "a", "1" },
                    { "b", "2" }
                }
            );

            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("12");
        }

        [Test]
        public void Parse_KeyValue()
        {
            var target = DataContext(
                GetData<string>("a"),
                "a", "1"
            );

            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("1");
        }

        [Test]
        public void Match_Test()
        {
            var target = DataContext(
                Rule(
                    SetData("test", "value"),
                    GetData<string>("test"),
                    (set, get) => set + get
                )
            );

            var result = target.Match("");
            result.Should().BeTrue();
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = DataContext(Any()).Named("SUT");
            var result = target.ToBnf();
            result.Should().Contain("SUT := .");
        }
    }

    public class Multi
    {
        [Test]
        public void Parse_KeyValue()
        {
            var target = DataContext(
                ProduceMulti(state =>
                {
                    var x = state.Data.Get<char>("A").Value;
                    return new[] { x };
                }), "A", 'X'
            );

            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Results[0].Value.Should().Be('X');
        }

        [Test]
        public void Parse_Dictionary()
        {
            var target = DataContext(
                ProduceMulti(state =>
                {
                    return new[] { state.Data.Get<char>("A").Value, state.Data.Get<char>("B").Value };
                }),
                new Dictionary<string, object>
                {
                     { "A", 'X' },
                     { "B", 'Y' }
                }
            );

            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(2);
            result.Results[0].Value.Should().Be('X');
            result.Results[1].Value.Should().Be('Y');
        }

        [Test]
        public void Parse_Test()
        {
            var target = DataContext(
                ProduceMulti(state =>
                {
                    state.Data.Set("A", 'X');
                    state.Data.Set("B", 'Y');
                    return new[] { state.Data.Get<char>("A").Value, state.Data.Get<char>("B").Value };
                })
            );

            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Results.Count.Should().Be(2);
            result.Results[0].Value.Should().Be('X');
            result.Results[1].Value.Should().Be('Y');
        }

        [Test]
        public void ToBnf_Test()
        {
            var target = DataContext(
                ProduceMulti(state =>
                {
                    var x = state.Data.Get<char>("A").Value;
                    return new[] { x };
                }), "A", 'X'
            ).Named("SUT");
            var result = target.ToBnf();
            result.Should().Contain("SUT := PRODUCE");
        }
    }
}

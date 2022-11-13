using System.Collections.Generic;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Parsers
{
    public class DataContextTests
    {
        [Test]
        public void StateData_SingleOutput_Test1()
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
        public void StateData_SingleOutput_Test2()
        {
            var target = DataContext(
                Rule(
                    GetData<string>("a"),
                    GetData<string>("b"),
                    (a, b) => a + b
                ),
                new Dictionary<string, string>
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
        public void StateData_SingleOutput_Test3()
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
        public void StateData_MultiOutput_NameValue()
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
        public void StateData_MultiOutput_Dictionary()
        {
            var target = DataContext(
                ProduceMulti(state =>
                {
                    return new[] { state.Data.Get<char>("A").Value, state.Data.Get<char>("B").Value };
                }),
                new Dictionary<string, char>
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
        public void StateData_MultiOutput()
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
    }
}

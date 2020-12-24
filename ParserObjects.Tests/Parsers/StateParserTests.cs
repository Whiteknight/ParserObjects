using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class StateParserTests
    {
        [Test]
        public void StateData_Test1()
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
        public void StateData_Test2()
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
        public void StateData_Test3()
        {
            var target = DataContext(
                GetData<string>("a"),
                "a", "1"
            );

            var result = target.Parse("");
            result.Success.Should().BeTrue();
            result.Value.Should().Be("1");
        }
    }
}

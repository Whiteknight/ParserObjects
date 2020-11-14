//using System.Collections.Generic;
//using FluentAssertions;
//using NUnit.Framework;
//using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Parsers
{
    public class StateParserMethodsTests
    {
        //[Test]
        //public void State_Test()
        //{
        //    var parser = Sequential(s =>
        //    {
        //        var first = s.Parse(Any());
        //        if (first == 'a')
        //            s.Parse(UpdateData<List<string>>(d => d.Add("starts with a")));
        //        var second = s.Parse(Any());
        //        if (second != 'b')
        //            s.Parse(UpdateData<List<string>>(d => d.Add("no b")));
        //        return "ok";
        //    });

        //    var state = new List<string>();
        //    var result = parser.Parse("abcde", state);
        //    result.Success.Should().BeTrue();
        //    result.Value.Should().Be("ok");

        //    state.Count.Should().Be(1);
        //    state[0].Should().Be("starts with a");
        //}
    }
}

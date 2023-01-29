using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal;
using ParserObjects.Internal.Bnf;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Visitors;

public class ToBnfTests
{
    [Test]
    public void ToBnf_AlreadySeenUnnamedParser_RecurseOk()
    {
        var offender = Any();
        var parser = (offender, offender).First().Named("parser");
        var result = parser.ToBnf();
        result.Should().Contain("parser := (. | .)");
    }

    [Test]
    public void ToBnf_AlreadySeenUnnamedParser_RecurseFail()
    {
        var offender = Deferred(() => Any());
        var parser = (offender, offender).First().Named("parser");
        var result = parser.ToBnf();
        var offenderString = offender.ToString();
        result.Should().Contain($"parser := (. | <ALREADY SEEN {offenderString}>)");
    }

    private class TestParser : IParser<char, string>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name => null;

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public bool Match(IParseState<char> state)
        {
            throw new NotImplementedException();
        }

        public IResult<string> Parse(IParseState<char> state)
        {
            throw new NotImplementedException();
        }

        public INamed SetName(string name)
        {
            throw new NotImplementedException();
        }

        IResult IParser<char>.Parse(IParseState<char> state)
        {
            throw new NotImplementedException();
        }

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
        {
            visitor.Get<ITestPartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    private interface ITestPartialVisitor<TState> : IPartialVisitor<TState>
    {
        void Accept(IParser parser, TState state);
    }

    private class TestParserBnfVisitor : ITestPartialVisitor<BnfStringifyState>
    {
        public void Accept(IParser parser, BnfStringifyState state)
        {
            if (parser is TestParser tp)
            {
                state.Append("TEST");
            }
        }
    }

    [Test]
    public void CustomParserTest()
    {
        var stringifier = new BnfStringifier();
        stringifier.Add<TestParserBnfVisitor>();

        var parser = List(new TestParser()).Named("TARGET");

        var result = stringifier.Stringify(parser);
        result.Should().Contain("TARGET := TEST*");
    }
}

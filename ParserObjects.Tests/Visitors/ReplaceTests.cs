using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Visitors;

public class ReplaceTests
{
    [Test]
    public void Replace_Fail_NoMatches()
    {
        var needle = Fail<char>().Replaceable().Named("needle");
        var haystack = (Any(), Any(), Any(), needle).First();
        var result = haystack.Replace(p => false, Fail<char>());
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Replace_Fail_SameParser()
    {
        var needle = Fail<char>();
        var haystack = (Any(), Any(), Any(), needle.Replaceable().Named("needle")).First();
        var result = haystack.Replace(p => p.Name == "needle", needle);
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Replace_Fail_RootNull()
    {
        var needle = Fail<char>().Replaceable().Named("needle");
        var haystack = (Any(), Any(), Any(), needle).First();
        var result = ((IParser)null).Replace(_ => false, needle);
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Replace_Fail_PredicateNull()
    {
        var needle = Fail<char>().Replaceable().Named("needle");
        var haystack = (Any(), Any(), Any(), needle).First();
        var result = haystack.Replace((Func<IParser, bool>)null, needle);
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Replace_Fail_ReplacementNull()
    {
        var needle = Fail<char>().Replaceable().Named("needle");
        var haystack = (Any(), Any(), Any(), needle).First();
        var result = haystack.Replace(r => false, null);
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Replace_Transform_Fail_RootNull()
    {
        var needle = Fail<char>().Replaceable().Named("needle");
        var haystack = (Any(), Any(), Any(), needle).First();
        var result = ((IParser<char, char>)null).Replace<char, char>(_ => false, _ => needle);
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Replace_Transform_Fail_PredicateNull()
    {
        var needle = Fail<char>().Replaceable().Named("needle");
        var haystack = (Any(), Any(), Any(), needle).First();
        var result = haystack.Replace<char, char>((Func<IParser, bool>)null, _ => needle);
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Replace_Transform_Fail_ReplacementNull()
    {
        var needle = Fail<char>().Replaceable().Named("needle");
        var haystack = (Any(), Any(), Any(), needle).First();
        var result = haystack.Replace<char, char>(_ => false, null);
        result.Success.Should().BeFalse();
    }

    [Test]
    public void Replace_Single_ByName()
    {
        var fail = Fail<char>();
        var needle = Fail<char>().Replaceable().Named("needle");
        var success = Any();
        var haystack = (fail, fail, fail, needle).First();

        var parseResult = haystack.Parse("X");
        parseResult.Success.Should().BeFalse();

        var result = haystack.Replace("needle", success);
        result.Success.Should().BeTrue();

        parseResult = haystack.Parse("X");
        parseResult.Success.Should().BeTrue();
        parseResult.Value.Should().Be('X');
    }
}

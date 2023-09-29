namespace ParserObjects.Tests.Examples.PN;

public class PolishCalculator
{
    public int Calculate(string s)
    {
        var parser = PolishGrammar.GetParser();
        var result = parser.Parse(s);
        if (!result.Success)
            throw new Exception("Parse failed");
        return result.Value;
    }
}

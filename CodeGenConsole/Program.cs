namespace CodeGenConsole;

public static class Program
{
    public static void Main(string[] args)
    {
        // Generate tests for the Rule() parser
        // CodeGenConsole/bin/Debug/net6.0/CodeGenConsole.exe > ParserObjects.Tests/Parsers/RuleTests.cs
        Console.WriteLine(RuleTestGenerator.Generate());
    }
}

using System;

namespace ParserObjects.Tests.Examples.PN
{
    public class PolishCalculator
    {
        public int Calculate(string s)
        {
            var parser = PolishGrammar.GetParser();
            var (success, value) = parser.Parse(s);
            if (!success)
                throw new Exception("Parse failed");
            return value;
        }
    }
}


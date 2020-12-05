namespace ParserObjects.Tests.Examples.Classes
{
    public class ClassParser
    {
        public Definition Parse(string s)
        {
            var parser = ClassGrammar.CreateParser();
            var result = parser.Parse(s);
            if (!result.Success)
                return null;
            var def = result.Value;
            if (!def.Validate())
                return null;
            return def;
        }
    }
}

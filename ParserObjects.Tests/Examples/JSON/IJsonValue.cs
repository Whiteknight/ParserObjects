using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Tests.Examples.JSON;

public interface IJsonValue
{
}

public class JsonArray : IJsonValue
{
    public JsonArray(IEnumerable<IJsonValue> values)
    {
        Values = values.ToList();
    }

    public IReadOnlyList<IJsonValue> Values { get; }
}

public class JsonObject : IJsonValue
{
    public JsonObject(IEnumerable<(string, IJsonValue)> values)
    {
        Properties = values.ToDictionary(t => t.Item1, t => t.Item2);
    }

    public Dictionary<string, IJsonValue> Properties { get; }
}

public class JsonString : IJsonValue
{
    public JsonString(string s)
    {
        Value = s;
    }

    public string Value { get; }
}

public class JsonNumber : IJsonValue
{
    public JsonNumber(double value)
    {
        Value = value;
    }

    public JsonNumber(string s)
    {
        Value = double.Parse(s, System.Globalization.NumberStyles.Float);
    }

    public double Value { get; }
}

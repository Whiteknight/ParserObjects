using System.Linq;

namespace ParserObjects.Tests.Examples.JSON;

public class JsonTests
{
    [Test]
    public void Number_Test()
    {
        var target = new JsonDeserializer();
        var result = target.Deserialize("123.45") as JsonNumber;
        result.Should().NotBeNull();
        result.Value.Should().Be(123.45);
    }

    [Test]
    public void String_Test()
    {
        var target = new JsonDeserializer();
        var result = target.Deserialize("'test'") as JsonString;
        result.Should().NotBeNull();
        result.Value.Should().Be("test");
    }

    [Test]
    public void Array_OfNumber_Test()
    {
        var target = new JsonDeserializer();
        var result = target.Deserialize("[1, 2, 3, 4]") as JsonArray;
        result.Should().NotBeNull();
        result.Values.Count.Should().Be(4);
        result.Values.Cast<JsonNumber>().Select(n => n.Value).Should().Contain(new[] { 1.0, 2.0, 3.0, 4.0 });
    }

    [Test]
    public void Object_Test()
    {
        var target = new JsonDeserializer();
        var result = target.Deserialize("{ 'a' : 1, 'b' : \"test\", 'c' : [2, 3] }") as JsonObject;
        result.Should().NotBeNull();
        result.Properties.Count.Should().Be(3);

        var a = result.Properties["a"] as JsonNumber;
        a.Should().NotBeNull();
        a.Value.Should().Be(1);

        var b = result.Properties["b"] as JsonString;
        b.Should().NotBeNull();
        b.Value.Should().Be("test");

        var c = result.Properties["c"] as JsonArray;
        c.Should().NotBeNull();
        c.Values.Count.Should().Be(2);
        c.Values.Cast<JsonNumber>().Select(n => n.Value).Should().Contain(new[] { 2.0, 3.0 });
    }
}

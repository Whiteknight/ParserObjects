using System.Collections.Generic;

namespace ParserObjects.Tests.Examples.XML;

public class XmlNode
{
    public XmlNode(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
    public IReadOnlyList<XmlNode> Children { get; set; }
}

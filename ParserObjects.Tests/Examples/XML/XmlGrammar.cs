using System.Collections.Generic;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Examples.XML;

public static class XmlGrammar
{
    // Implements a simple XML parser showing off recursive state data features.
    // Notice that this XML parser does not support text, comments, whitespace or tags with
    // non-letter characters in their names.
    public static IParser<char, XmlNode> CreateParser()
    {
        IParser<char, XmlNode> nodeInternal = null;

        // Deferred node parser so we can recurse. Call RecurseData here so each new node gets
        // a new data frame to play with.
        var node = Deferred(() => nodeInternal);

        var nodeName = Word();

        // openTag sets the generated XmlNode instance to the current data frame with
        // .SetResultState
        var openTag = (Match('<'), nodeName, Match('>'))
            .Rule((open, name, close) => new XmlNode(name))
            .Named("openTag")
            .SetResultData("tag");

        // closeTagName gets the current XmlNode instance from the current data frame, and
        // creates a Match() parser to match it.
        var closeTagName = Create(state =>
        {
            var result = state.Data.Get<XmlNode>("tag");
            if (result.Success)
                return Match(result.Value.Name);
            return Fail<IReadOnlyList<char>>("No tag found in current data frame");
        }).Named("closeTagName");

        // closeTag references closeTagName
        var closeTag = (Match("</"), closeTagName, Match('>'))
            .Rule((open, name, close) => (object)null)
            .Named("closeTag");

        // nodeInternal is the implementation of node above, it is an open tag and matching
        // close tag, with zero or more children
        nodeInternal = Rule(
                openTag,
                node.List(),
                closeTag,
                (open, children, close) =>
                {
                    open.Children = children;
                    return open;
                }
            )
            .WithDataContext()
            .Named("node");

        return node;
    }
}

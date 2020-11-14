# XML Parser Example

The toy XML parser here is used to demonstrate the use of contextual/state data during the parse. In general it's a good idea for parsers to be "pure". That is, the output of the parser should depend only on the current input `ISequence<T>` and not rely on any other external, mutable state. However, in some cases, it may be useful or even necessary to provide contextual clues to the parser to help it more effectively parse the input.

In the XML parser pay particular attention to a few parts:

```csharp
var node = Deferred(() => RecurseData(nodeInternal));
``` 

This line does two things: It uses `Deferred()` to setup a reference so the parser can recurse, and it uses `RecurseData()` to create a new data frame for every attempt to parse a node. This data frame is the way that the `"tag"` data set on a child tag does not overwrite the `"tag"` data set from the parent tag.

```csharp
var openTag = (Match('<'), nodeName, Match('>'))
    .Produce((open, name, close) => new XmlNode(name))
    .Named("openTag")
    .SetResultState("tag");
```

The final method call `.SetResultState()` saves the `XmlNode` result of the parser to the current data frame using the name `"tag"`.

```csharp
var closeTagName = Create(t =>
{
    var (has, node) = t.Data.Get<XmlNode>("tag");
    if (has)
        return Match(node.Name);
    return Fail<IReadOnlyList<char>>("No tag found in current data frame");
}).Named("closeTagName");
```

Here is where we read the state data. The `Create()` parser allows creating a parser during parse time. In this case it gets the string name of the `XmlNode` from the `openTag` in the current data frame, and creates a `MatchSequenceParser` instance to match that sequence of characters. If the close tag does not match the open tag, a failure result will be returned and bubbled up to the caller.

There are other ways to accomplish this same goal without the use of impure parsers and state. For example, the `closeTagName` rule could match any `nodeName` value, and later in the `nodeInternal` rule when we are trying to assemble things we could check if the open and close tags match and throw an exception if not. Throwing an exception in the middle of the parse is probably not a great idea either, because you can't continue from that point. Modern editors and IDEs want the parser to try to get all the way to the end, and give a list of all problems in the input document, not just the first one.

The final approach would be to assemble an abstract syntax tree first, instead of trying to assemble an `XmlNode` output directly. Then when you generate your AST you would have a separate step where you converted to an `XmlNode`, during which you could aggregate all errors you find along the way and report them all to the user at the end.
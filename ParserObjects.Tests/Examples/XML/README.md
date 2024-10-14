# XML Parser Example

The toy XML parser here is used to demonstrate the use of contextual/state data during the parse. In general it's a good idea for parsers to be "pure". That is, the output of the parser should depend only on the current input `ISequence<T>` and not rely on any other external, mutable state. However, in some cases, it may be useful or even necessary to provide contextual clues to the parser to help it more effectively parse the input.

In the XML parser pay particular attention to a few parts:

```csharp
var node = Deferred(() => nodeInternal);
``` 

This line uses `Deferred()` to setup a reference so the parser can recurse. Later we provide a definition for `nodeInternal`:

```csharp
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
```

Here you can also see the `.WithDataContext()` method, which sets up a data frame. Whenever we try to parse a node we set up a contextual data frame, and that is the storage scratchpad where we can store the `"tag"` name of the `openTag`. We set the tag name here, with the `.SetResultData("tag")` parser:

```csharp
var openTag = (Match('<'), nodeName, Match('>'))
    .Rule((open, name, close) => new XmlNode(name))
    .Named("openTag")
    .SetResultData("tag");
```

Then when we want to find a matching closing tag, we use the `Create()` parser to get the `"tag"` name from the current data frame and create a `Match()` parser for that name:

```csharp
var closeTagName = Create(t =>
{
    var (has, node) = t.Data.Get<XmlNode>("tag");
    if (has)
        return Match(node.Name);
    return Fail<IReadOnlyList<char>>("No tag found in current data frame");
}).Named("closeTagName");
```

There are other ways to accomplish this same goal without the use of impure parsers and state. For example, the `closeTagName` rule could match any `nodeName` value, and later in the `nodeInternal` rule's production callback we could verify that the open and close tags match. Throwing an exception in the middle of the parse is probably not a great idea either, because you can't continue from that point. Modern editors and IDEs want the parser to try to get all the way to the end, and give a list of all problems in the input document, not just the first one.

Another approach would be to assemble an abstract syntax tree first, instead of trying to assemble an `XmlNode` output directly. Then when you generate your AST you would have a separate step where you converted to an `XmlNode`, during which you could aggregate all errors you find along the way and report them all to the user at the end.

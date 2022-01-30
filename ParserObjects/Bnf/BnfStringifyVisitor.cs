using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ParserObjects.Bnf;

public sealed class BnfStringifyVisitor
{
    private readonly IReadOnlyList<IPartialVisitor<BnfStringifyVisitor>> _partials;

    public StringBuilder Builder { get; }
    public Stack<StringBuilder> History { get; }
    public HashSet<IParser> Seen { get; }
    public StringBuilder Current { get; set; }

    public BnfStringifyVisitor(StringBuilder sb, IReadOnlyList<IPartialVisitor<BnfStringifyVisitor>> partials)
    {
        Builder = sb;
        _partials = partials;
        History = new Stack<StringBuilder>();
        Seen = new HashSet<IParser>();
        Current = new StringBuilder();
    }

    public void Visit(IParser parser, bool writeInPlace = true)
    {
        if (parser == null)
            return;

        // If we have already seen this parser, just put in a tag to represent it
        if (Seen.Contains(parser))
        {
            if (!string.IsNullOrEmpty(parser.Name))
            {
                Current.Append('<').Append(parser.Name).Append('>');
                return;
            }

            if (!parser.GetChildren().Any())
            {
                // if it's a simple parser with no recursion, we can just visit it again.
                // No harm because it won't cause a loop
                TryVisit(parser);
                return;
            }

            Current.Append("<ALREADY SEEN UNNAMED PARSER>");
            return;
        }

        Seen.Add(parser);

        // If the parser doesn't have a name, recursively visit it in-place
        if (writeInPlace && string.IsNullOrEmpty(parser.Name))
        {
            TryVisit(parser);
            return;
        }

        // If the parser does have a name, write a tag for it
        if (writeInPlace)
        {
            Current.Append('<');
            Current.Append(parser.Name);
            Current.Append('>');
        }

        if (parser is IHiddenInternalParser)
            return;

        // Start a new builder, so we can start stringifying this new parser on it's own line.
        History.Push(Current);
        Current = new StringBuilder();

        // Visit the parser recursively to fill in the builder
        TryVisit(parser);

        // Append the current builder to the overall builder
        var rule = Current.ToString();
        if (!string.IsNullOrEmpty(rule))
        {
            Builder.Append(parser.Name);
            Builder.Append(" := ");
            Builder.Append(Current);
            Builder.AppendLine(";");
        }

        Current = History.Pop();
    }

    private void TryVisit(IParser parser)
    {
        foreach (var visitor in _partials)
        {
            var visited = visitor.TryAccept(parser, this);
            if (visited)
                return;
        }

        Debug.WriteLine($"No override match found for {parser.GetType().Name}");
        Current.Append($"<UNVISITABLE PARSER Id={parser.Id} Type={parser.GetType().FullName}>");
    }

    public BnfStringifyVisitor Append(char c)
    {
        Current.Append(c);
        return this;
    }

    public BnfStringifyVisitor Append(int i)
    {
        Current.Append(i);
        return this;
    }

    public BnfStringifyVisitor Append(int? i)
    {
        Current.Append(i);
        return this;
    }

    public BnfStringifyVisitor Append(string s)
    {
        Current.Append(s);
        return this;
    }

    public BnfStringifyVisitor Append(params object[] args)
    {
        foreach (var arg in args)
        {
            if (arg is string s)
                Current.Append(s);
            else if (arg is IParser p)
                Visit(p);
            else
                Current.Append(arg);
        }

        return this;
    }
}

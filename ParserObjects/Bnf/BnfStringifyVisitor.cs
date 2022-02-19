using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ParserObjects.Bnf;

public sealed class BnfStringifyVisitor
{
    private readonly IReadOnlyList<IPartialVisitor<BnfStringifyVisitor>> _partials;
    private readonly StringBuilder _builder;
    private readonly Stack<StringBuilder> _history;
    private readonly HashSet<IParser> _seen;

    private StringBuilder Current => _history.Peek();

    public BnfStringifyVisitor(StringBuilder sb, IReadOnlyList<IPartialVisitor<BnfStringifyVisitor>> partials)
    {
        _builder = sb;
        _partials = partials;
        _history = new Stack<StringBuilder>();
        _history.Push(new StringBuilder());
        _seen = new HashSet<IParser>();
    }

    public void Visit(IParser parser, bool writeInPlace = true)
    {
        if (parser == null)
            return;

        // We have already seen this parser. Figure out how to display it without getting into a loop
        if (_seen.Contains(parser))
        {
            // If it has a name, just write a tag here
            if (!string.IsNullOrEmpty(parser.Name))
            {
                Current.Append('<').Append(parser.Name).Append('>');
                return;
            }

            // If it doesn't have children, we can safely recurse without getting into a loop
            if (!parser.GetChildren().Any())
            {
                TryVisit(parser);
                return;
            }

            // Recursing would cause an infinite loop. Instead write a warning tag with type/id
            // You should really give your parsers names.
            Current.Append($"<ALREADY SEEN {parser}>");
            return;
        }

        _seen.Add(parser);

        // If the parser doesn't have a name, recursively visit it in-place
        if (writeInPlace && string.IsNullOrEmpty(parser.Name))
        {
            TryVisit(parser);
            return;
        }

        if (writeInPlace)
            Current.Append(parser.ToString());

        if (parser is IHiddenInternalParser)
            return;

        // Start a new builder, so we can start stringifying this new parser on it's own line.
        var current = new StringBuilder();
        _history.Push(current);
        TryVisit(parser);
        _history.Pop();

        // Append the current builder to the overall builder
        var rule = current.ToString();
        if (!string.IsNullOrEmpty(rule))
        {
            _builder.Append(parser.Name);
            _builder.Append(" := ");
            _builder.Append(rule);
            _builder.AppendLine(";");
        }
    }

    private void TryVisit(IParser parser)
    {
        foreach (var visitor in _partials)
        {
            var visited = visitor.TryAccept(parser, this);
            if (visited)
                return;
        }

        Debug.WriteLine($"No override match found for {parser.GetType().FullName}");
        Current.Append($"<UNVISITABLE PARSER {parser}>");
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
            {
                Current.Append(s);
                continue;
            }

            if (arg is IParser p)
            {
                Visit(p);
                continue;
            }

            Current.Append(arg);
        }

        return this;
    }
}

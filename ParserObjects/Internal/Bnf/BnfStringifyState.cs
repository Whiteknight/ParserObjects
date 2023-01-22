using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserObjects.Internal.Bnf;

public sealed class BnfStringifyState
{
    private readonly BnfStringifier _visitor;
    private readonly StringBuilder _builder;
    private readonly Stack<StringBuilder> _history;
    private readonly HashSet<IParser> _seen;

    private StringBuilder Current => _history.Peek();

    public BnfStringifyState(BnfStringifier visitor, StringBuilder sb)
    {
        _visitor = visitor;
        _builder = sb;
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
            VisitAlreadySeenParser(parser);
            return;
        }

        _seen.Add(parser);

        // If the parser doesn't have a name, recursively visit it in-place
        if (writeInPlace && string.IsNullOrEmpty(parser.Name))
        {
            parser.Visit(_visitor, this);
            return;
        }

        if (writeInPlace)
            Current.Append(parser.ToString());

        // Start a new builder, so we can start stringifying this new parser on it's own line.
        var current = new StringBuilder();
        _history.Push(current);
        parser.Visit(_visitor, this);
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

    private void VisitAlreadySeenParser(IParser parser)
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
            parser.Visit(_visitor, this);
            return;
        }

        // Recursing would cause an infinite loop. Instead write a warning tag with type/id
        // You should really give your parsers names.
        Current.Append("<ALREADY SEEN ").Append(parser).Append('>');
    }

    public BnfStringifyState Append(char c)
    {
        Current.Append(c);
        return this;
    }

    public BnfStringifyState Append(int i)
    {
        Current.Append(i);
        return this;
    }

    public BnfStringifyState Append(int? i)
    {
        Current.Append(i);
        return this;
    }

    public BnfStringifyState Append(string s)
    {
        Current.Append(s);
        return this;
    }

    public BnfStringifyState Append(params object[] args)
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

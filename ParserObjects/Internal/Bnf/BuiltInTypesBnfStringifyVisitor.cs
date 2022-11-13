using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects;
using ParserObjects.Internal.Parsers;
using ParserObjects.Internal.Pratt;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Bnf;

public sealed class BuiltInTypesBnfStringifyVisitor : IPartialVisitor<BnfStringifyVisitor>
{
    public bool TryAccept(IParser parser, BnfStringifyVisitor state)
    {
        Assert.ArgumentNotNull(parser, nameof(parser));
        Assert.ArgumentNotNull(state, nameof(state));

        var result = ((dynamic)this).Accept((dynamic)parser, state);
        if (result == null)
            return false;

        var asBool = Convert.ToBoolean(result);
        return asBool;
    }

    // fallback method if a better match can't be found. This method should never be called if
    // we properly define a .Accept method variant for every type of parser
    private bool Accept(IParser p, BnfStringifyVisitor state)
    {
        return false;
    }

    // Some regex syntax:
    // (?= ) is "positive lookahead" syntax. We use it to show something doesn't consume input
    // (?! ) is "negative lookahead" syntax. We use it for non-follows situations

    private bool Accept<TInput>(AndParser<TInput> p, BnfStringifyVisitor state)
    {
        var children = p.GetChildren().ToArray();
        state.Append(children[0], " && ", children[1]);
        return true;
    }

    private bool Accept<TInput>(AnyParser<TInput> p, BnfStringifyVisitor state)
    {
        state.Append('.');
        return true;
    }

    private bool Accept<TInput, TMiddle, TOutput>(Chain<TInput, TMiddle, TOutput>.Parser p, BnfStringifyVisitor state)
    {
        var child = p.GetChildren().Single();
        state.Append(child, "->Chain");
        return true;
    }

    private bool Accept<TInput, TOutput>(Chain<TInput, TOutput>.Parser p, BnfStringifyVisitor state)
    {
        var child = p.GetChildren().Single();
        state.Append(child, "->Chain");
        return true;
    }

    private bool Accept<TInput, TMulti, TOutput>(ContinueWith<TInput, TMulti, TOutput>.MultiParser p, BnfStringifyVisitor state)
    {
        var children = p.GetChildren().ToList();
        state.Append(children[0], " CONTINUEWITH ", children[1]);
        return true;
    }

    private bool Accept<TInput, TMulti, TOutput>(ContinueWith<TInput, TMulti, TOutput>.SingleParser p, BnfStringifyVisitor state)
    {
        var children = p.GetChildren().ToList();
        state.Append(children[0], " CONTINUEWITH ", children[1]);
        return true;
    }

    private bool Accept<TInput, TOutput>(Create<TInput, TOutput>.Parser p, BnfStringifyVisitor state)
    {
        state.Append("CREATE");
        return true;
    }

    private bool Accept<TInput, TOutput>(Create<TInput, TOutput>.MultiParser p, BnfStringifyVisitor state)
    {
        state.Append("CREATE");
        return true;
    }

    private bool Accept<TInput, TOutput>(Deferred<TInput, TOutput>.Parser p, BnfStringifyVisitor state)
    {
        state.Append(p.GetChildren().First());
        return true;
    }

    private bool Accept<TInput, TOutput>(Deferred<TInput, TOutput>.MultiParser p, BnfStringifyVisitor state)
    {
        state.Append(p.GetChildren().First());
        return true;
    }

    private bool Accept<TInput, TOutput>(EachParser<TInput, TOutput> p, BnfStringifyVisitor state)
    {
        var children = p.GetChildren().ToList();
        if (children.Count == 1)
        {
            state.Append(children[0]);
            return true;
        }

        state.Append("EACH(", children[0]);

        for (int i = 1; i <= children.Count - 1; i++)
            state.Append(" | ", children[i]);

        state.Append(')');
        return true;
    }

    private bool Accept<TInput, TOutput>(Earley<TInput, TOutput>.Parser p, BnfStringifyVisitor state)
    {
        state.Append(p.GetBnf(state));
        return true;
    }

    private bool Accept<TInput>(EmptyParser<TInput> p, BnfStringifyVisitor state)
    {
        state.Append("()");
        return true;
    }

    private bool Accept<TInput>(EndParser<TInput> p, BnfStringifyVisitor state)
    {
        state.Append("END");
        return true;
    }

    private bool Accept<TInput>(Examine<TInput>.Parser p, BnfStringifyVisitor state)
    {
        state.Append(p.GetChildren().First());
        return true;
    }

    private bool Accept<TInput, TOutput>(Examine<TInput, TOutput>.Parser p, BnfStringifyVisitor state)
    {
        state.Append(p.GetChildren().First());
        return true;
    }

    private bool Accept<TInput, TOutput>(Examine<TInput, TOutput>.MultiParser p, BnfStringifyVisitor state)
    {
        state.Append(p.GetChildren().First());
        return true;
    }

    private bool Accept<TInput, TOutput>(FailParser<TInput, TOutput> p, BnfStringifyVisitor state)
    {
        state.Append("FAIL");
        return true;
    }

    private bool Accept<TInput, TOutput>(FirstParser<TInput, TOutput> p, BnfStringifyVisitor state)
    {
        var children = p.GetChildren().ToList();
        if (children.Count == 1)
        {
            state.Append(children[0]);
            return true;
        }

        state.Append("(", children[0]);

        for (int i = 1; i <= children.Count - 1; i++)
            state.Append(" | ", children[i]);

        state.Append(')');
        return true;
    }

    // Includes all variants of Function<T>.Parser, Function<TIn, TOut>.Parser, .MultiParser, etc
    private bool AcceptFunctionVariant(string? description, IReadOnlyList<IParser> children, BnfStringifyVisitor state)
    {
        if (string.IsNullOrEmpty(description))
        {
            if (children.Count == 0)
            {
                state.Append("User Function");
                return true;
            }

            if (children.Count == 1)
            {
                state.Append(children[0]);
                return true;
            }

            state.Append("User Function of");
            foreach (var child in children)
            {
                state.Append(" ");
                state.Append(child);
            }

            return true;
        }

        var parts = description!.Split(new[] { "{child}" }, children.Count + 1, StringSplitOptions.None);
        state.Append(parts[0]);

        for (int i = 1; i < parts.Length; i++)
            state.Append(children[i - 1], parts[i]);

        return true;
    }

    private bool Accept<TInput>(Function<TInput>.Parser p, BnfStringifyVisitor state)
    {
        return AcceptFunctionVariant(p.Description, p.GetChildren().ToList(), state);
    }

    private bool Accept<TInput, TOutput>(Function<TInput, TOutput>.Parser p, BnfStringifyVisitor state)
    {
        return AcceptFunctionVariant(p.Description, p.GetChildren().ToList(), state);
    }

    private bool Accept<TInput, TOutput>(Function<TInput, TOutput>.MultiParser p, BnfStringifyVisitor state)
    {
        return AcceptFunctionVariant(p.Description, p.GetChildren().ToList(), state);
    }

    private bool Accept<TInput, TOutput>(IfParser<TInput, TOutput> p, BnfStringifyVisitor state)
    {
        var children = p.GetChildren().ToArray();
        state.Append("IF ", children[0], " THEN ", children[1], " ELSE ", children[2]);
        return true;
    }

    private bool Accept<TInput, TOutput>(LeftApply<TInput, TOutput>.Parser p, BnfStringifyVisitor state)
    {
        var children = p.GetChildren().ToArray();
        var initial = children[0];
        var middle = children[1];
        state.Append(middle, " | ", initial);
        return true;
    }

    private bool Accept<TInput, TOutput>(LimitedListParser<TInput, TOutput> p, BnfStringifyVisitor state)
    {
        state.Append(p.GetChildren().First());

        // If we have a maximum, handle a range with a maximum. We always have a minimum
        if (p.Maximum.HasValue)
        {
            if (p.Maximum == p.Minimum)
            {
                state.Append('{').Append(p.Minimum).Append('}');
                return true;
            }

            state.Append('{').Append(p.Minimum).Append(", ").Append(p.Maximum).Append('}');
            return true;
        }

        // No maximum, so handle special cases with minimum values first.
        if (p.Minimum == 0)
        {
            state.Append('*');
            return true;
        }

        if (p.Minimum == 1)
        {
            state.Append('+');
            return true;
        }

        state.Append('{').Append(p.Minimum).Append(",}");
        return true;
    }

    private bool Accept<TInput>(MatchPredicateParser<TInput> p, BnfStringifyVisitor state)
    {
        state.Append("MATCH");
        return true;
    }

    private bool Accept<TInput>(MatchPatternParser<TInput> p, BnfStringifyVisitor state)
    {
        var pattern = string.Join(" ", p.Pattern.Select(i => $"'{i}'"));
        state.Append(pattern);
        return true;
    }

    private bool Accept<TInput>(NegativeLookaheadParser<TInput> p, BnfStringifyVisitor state)
    {
        state.Append("(?! ", p.GetChildren().First(), " )");
        return true;
    }

    private bool Accept<TInput>(NotParser<TInput> p, BnfStringifyVisitor state)
    {
        var child = p.GetChildren().First();
        state.Append("!", child);
        return true;
    }

    private bool Accept<TInput>(OrParser<TInput> p, BnfStringifyVisitor state)
    {
        var children = p.GetChildren().ToArray();
        state.Append(children[0], " || ", children[1]);
        return true;
    }

    private bool Accept<TInput>(PeekParser<TInput> p, BnfStringifyVisitor state)
    {
        state.Append("(?=.)");
        return true;
    }

    private bool Accept<TInput, TOutput>(PrattParser<TInput, TOutput> p, BnfStringifyVisitor state)
    {
        var children = p.GetChildren().ToArray();
        if (children.Length == 0)
        {
            state.Append("PRATT()");
            return true;
        }

        state.Append("PRATT(", children[0]);
        for (int i = 1; i < children.Length; i++)
            state.Append(", ", children[i]);

        state.Append(')');
        return true;
    }

    /* Unmerged change from project 'ParserObjects (net5.0)'
    Before:
        private bool Accept<TInput, TOutput>(Pratt.ParseContext<TInput, TOutput> p, BnfStringifyVisitor state)
    After:
        private bool Accept<TInput, TOutput>(Internal.Pratt.ParseContext<TInput, TOutput> p, BnfStringifyVisitor state)
    */

    /* Unmerged change from project 'ParserObjects (net6.0)'
    Before:
        private bool Accept<TInput, TOutput>(Pratt.ParseContext<TInput, TOutput> p, BnfStringifyVisitor state)
    After:
        private bool Accept<TInput, TOutput>(Internal.Pratt.ParseContext<TInput, TOutput> p, BnfStringifyVisitor state)
    */

    private bool Accept<TInput, TOutput>(ParseContext<TInput, TOutput> p, BnfStringifyVisitor state)
    {
        state.Append("PRATT RECURSE");
        return true;
    }

    private bool Accept<TInput>(PositiveLookaheadParser<TInput> p, BnfStringifyVisitor state)
    {
        state.Append("(?= ", p.GetChildren().First(), " )");
        return true;
    }

    private bool Accept(RegexParser p, BnfStringifyVisitor state)
    {
        state.Append("/", p.Pattern, "/");
        return true;
    }

    private bool Accept<TInput>(Replaceable<TInput>.SingleParser p, BnfStringifyVisitor state)
    {
        state.Append(p.GetChildren().First());
        return true;
    }

    private bool Accept<TInput, TOutput>(Replaceable<TInput, TOutput>.SingleParser p, BnfStringifyVisitor state)
    {
        state.Append(p.GetChildren().First());
        return true;
    }

    private bool Accept<TInput, TOutput>(Replaceable<TInput, TOutput>.MultiParser p, BnfStringifyVisitor state)
    {
        state.Append(p.GetChildren().First());
        return true;
    }

    private bool Accept<TInput, TMiddle, TOutput>(RightApply<TInput, TMiddle, TOutput>.Parser p, BnfStringifyVisitor state)
    {
        var children = p.GetChildren().ToArray();
        state.Append(children[0], " (", children[1], string.IsNullOrEmpty(p.Name) ? " SELF" : $" <{p.Name}>)*");
        return true;
    }

    private bool Accept<TInput, TOutput>(RuleParser<TInput, TOutput> p, BnfStringifyVisitor state)
    {
        var children = p.GetChildren().ToArray();
        state.Append("(", children[0]);
        foreach (var child in children.Skip(1))
            state.Append(" ", child);

        state.Append(')');
        return true;
    }

    private bool Accept<TInput, TOutput>(Select<TInput, TOutput>.Parser p, BnfStringifyVisitor state)
    {
        state.Append("SELECT ", p.GetChildren().Single());
        return true;
    }

    private bool Accept<TInput, TOutput>(Sequential.Parser<TInput, TOutput> p, BnfStringifyVisitor state)
    {
        state.Append("User Function");
        return true;
    }

    private bool Accept<TInput, TOutput>(SynchronizeParser<TInput, TOutput> p, BnfStringifyVisitor state)
    {
        state.Append(p.GetChildren().First());
        return true;
    }

    private bool Accept<TInput, TMiddle, TOutput>(Transform<TInput, TMiddle, TOutput>.Parser p, BnfStringifyVisitor state)
    {
        state.Append(p.GetChildren().First());
        return true;
    }

    private bool Accept<TInput, TMiddle, TOutput>(Transform<TInput, TMiddle, TOutput>.MultiParser p, BnfStringifyVisitor state)
    {
        state.Append(p.GetChildren().First());
        return true;
    }

    private bool Accept<TInput, TOutput>(TrieParser<TInput, TOutput> p, BnfStringifyVisitor state)
    {
        var allPatterns = p.Trie.GetAllPatterns().ToList();
        if (allPatterns.Count == 0)
            return true;

        static void PrintPattern(IEnumerable<TInput> pattern, BnfStringifyVisitor s)
        {
            var spaceSeparatedQuotedItems = string.Join(" ", pattern.Select(item => $"'{item}'"));
            s.Append("(", spaceSeparatedQuotedItems, ")");
        }

        PrintPattern(allPatterns[0], state);

        foreach (var pattern in allPatterns.Skip(1))
        {
            state.Append(" | ");
            PrintPattern(pattern, state);
        }

        return true;
    }
}

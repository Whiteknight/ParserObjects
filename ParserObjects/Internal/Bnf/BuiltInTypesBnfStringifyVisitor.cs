using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects;
using ParserObjects.Internal.Parsers;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Bnf;

#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable RCS1163 // Unused parameter.

public sealed class BuiltInTypesBnfStringifyVisitor : IPartialVisitor<BnfStringifyVisitor>
{
    // WARNING: This class uses dynamic dispatch to call different Accept() method variants
    // depending on the runtime type of the "IParser parser" parameter. Because the dispatch
    // happens at runtime, these Accept() method variants appear to be unused at compile-time
    // and the compiler will raise warnings about it. DO NOT DELETE "unused" CODE IN THIS FILE!

    public bool TryAccept(IParser parser, BnfStringifyVisitor state)
    {
        Assert.ArgumentNotNull(parser, nameof(parser));
        Assert.ArgumentNotNull(state, nameof(state));

        var result = ((dynamic)this).Accept((dynamic)parser, state);
        if (result == null)
            return false;

        return Convert.ToBoolean(result);
    }

    // fallback method if a better match can't be found. This method should never be called if
    // we properly define a .Accept method variant for every type of parser
    private bool Accept(IParser? p, BnfStringifyVisitor? state) => false;

    // Some regex syntax:
    // (?= ) is "positive lookahead" syntax. We use it to show something doesn't consume input
    // (?! ) is "negative lookahead" syntax. We use it for non-follows situations

    private bool Accept<TInput>(AndParser<TInput> p, BnfStringifyVisitor state)
    {
        var children = p.GetChildren().ToArray();
        state.Append(children[0], " && ", children[1]);
        return true;
    }

    private bool Accept<TInput>(AnyParser<TInput> _, BnfStringifyVisitor state)
    {
        state.Append('.');
        return true;
    }

    private bool Accept<TInput>(Cache<TInput>.Parser p, BnfStringifyVisitor state)
    {
        state.Append("CACHED(", p.GetChildren().First(), ")");
        return true;
    }

    private bool Accept<TInput, TOutput>(Cache<TInput>.Parser<TOutput> p, BnfStringifyVisitor state)
    {
        state.Append("CACHED(", p.GetChildren().First(), ")");
        return true;
    }

    private bool Accept<TInput, TOutput>(Cache<TInput>.MultiParser<TOutput> p, BnfStringifyVisitor state)
    {
        state.Append("CACHED(", p.GetChildren().First(), ")");
        return true;
    }

    private bool Accept<TInput>(CaptureParser<TInput> p, BnfStringifyVisitor state)
    {
        var children = p.GetChildren().ToArray();
        state.Append("(", children[0]);
        foreach (var child in children.Skip(1))
            state.Append(" ", child);

        state.Append(')');
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

    private bool Accept<TInput, TOutput>(Context<TInput>.MultiParser<TOutput> p, BnfStringifyVisitor state)
    {
        state.Append(p.Inner);
        return true;
    }

    private bool Accept<TInput, TOutput>(Context<TInput>.Parser<TOutput> p, BnfStringifyVisitor state)
    {
        state.Append(p.Inner);
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

    private bool Accept<TInput, TOutput>(Create<TInput, TOutput>.Parser _, BnfStringifyVisitor state)
    {
        state.Append("CREATE");
        return true;
    }

    private bool Accept<TInput, TOutput>(Create<TInput, TOutput>.MultiParser _, BnfStringifyVisitor state)
    {
        state.Append("CREATE");
        return true;
    }

    private bool Accept<TInput>(DataFrame<TInput>.Parser p, BnfStringifyVisitor state)
    {
        state.Append(p.GetChildren().First());
        return true;
    }

    private bool Accept<TInput, TOutput>(DataFrame<TInput>.Parser<TOutput> p, BnfStringifyVisitor state)
    {
        state.Append(p.GetChildren().First());
        return true;
    }

    private bool Accept<TInput, TOutput>(DataFrame<TInput>.MultiParser<TOutput> p, BnfStringifyVisitor state)
    {
        state.Append(p.GetChildren().First());
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

    private bool Accept<TInput>(EmptyParser<TInput> _, BnfStringifyVisitor state)
    {
        state.Append("()");
        return true;
    }

    private bool Accept<TInput>(EndParser<TInput> _, BnfStringifyVisitor state)
    {
        state.Append("END");
        return true;
    }

    private bool Accept<TInput>(ExamineParser<TInput> p, BnfStringifyVisitor state)
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

    private bool Accept<TInput, TData>(Function<TInput>.Parser<TData> p, BnfStringifyVisitor state)
    {
        return AcceptFunctionVariant(p.Description, p.GetChildren().ToList(), state);
    }

    private bool Accept<TInput, TOutput, TData>(Function<TInput, TOutput>.Parser<TData> p, BnfStringifyVisitor state)
    {
        return AcceptFunctionVariant(p.Description, p.GetChildren().ToList(), state);
    }

    private bool Accept<TInput, TOutput, TData>(Function<TInput, TOutput>.MultiParser<TData> p, BnfStringifyVisitor state)
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

    private bool Accept<TInput>(MatchItemParser<TInput> p, BnfStringifyVisitor state)
    {
        state.Append("'", (object?)p.Item ?? ' ', "'");
        return true;
    }

    private bool Accept<TInput>(MatchPredicateParser<TInput> _, BnfStringifyVisitor state)
    {
        state.Append("MATCH()");
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

    private bool Accept<TInput, TItem, TOutput>(NonGreedyList<TInput, TItem, TOutput>.Parser p, BnfStringifyVisitor state)
    {
        var children = p.GetChildren().ToArray();
        var item = children[0];
        var separator = children[1];
        var continuation = children[2];

        state.Append(item);

        if (separator != null && separator is not EmptyParser<TInput>)
            state.Append(" (", separator, " ", item, ")");

        static void WriteQuantifier(BnfStringifyVisitor v, int minimum, int? maximum)
        {
            // If we have a maximum, handle a range with a maximum. We always have a minimum
            if (maximum.HasValue)
            {
                if (maximum == 1 && minimum == 0)
                {
                    v.Append("?");
                    return;
                }

                if (maximum == minimum)
                {
                    v.Append('{').Append(minimum).Append('}');
                    return;
                }

                v.Append('{').Append(minimum).Append(", ").Append(maximum).Append('}');
                return;
            }

            // No maximum, so handle special cases with minimum values first.
            if (minimum == 0)
            {
                v.Append('*');
                return;
            }

            if (minimum == 1)
            {
                v.Append('+');
                return;
            }

            v.Append('{').Append(minimum).Append(",}");
        }

        WriteQuantifier(state, p.Minimum, p.Maximum);
        state.Append(" ", continuation);
        return true;
    }

    private bool Accept<TInput>(NotParser<TInput> p, BnfStringifyVisitor state)
    {
        var child = p.GetChildren().First();
        state.Append("!", child);
        return true;
    }

    private bool Accept<TInput, TOutput>(Optional<TInput, TOutput>.DefaultValueParser p, BnfStringifyVisitor state)
    {
        state.Append(p.GetChildren().First(), "?");
        return true;
    }

    private bool Accept<TInput, TOutput>(Optional<TInput, TOutput>.NoDefaultParser p, BnfStringifyVisitor state)
    {
        state.Append(p.GetChildren().First(), "?");
        return true;
    }

    private bool Accept<TInput>(OrParser<TInput> p, BnfStringifyVisitor state)
    {
        var children = p.GetChildren().ToArray();
        state.Append(children[0], " || ", children[1]);
        return true;
    }

    private bool Accept<TInput>(PeekParser<TInput> _, BnfStringifyVisitor state)
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

    private bool Accept<TInput, TOutput>(ParserObjects.Internal.Pratt.ParseContext<TInput, TOutput> _, BnfStringifyVisitor state)
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

    private void AcceptRepetitionParser<TInput>(BnfStringifyVisitor state, IParser item, IParser separator, int minimum, int? maximum)
    {
        state.Append(item);

        if (separator != null && separator is not EmptyParser<TInput>)
            state.Append(" (", separator, " ", item, ")");

        // If we have a maximum, handle a range with a maximum. We always have a minimum
        if (maximum.HasValue)
        {
            if (maximum == 1 && minimum == 0)
            {
                state.Append("?");
                return;
            }

            if (maximum == minimum)
            {
                state.Append('{').Append(minimum).Append('}');
                return;
            }

            state.Append('{').Append(minimum).Append(", ").Append(maximum).Append('}');
            return;
        }

        // No maximum, so handle special cases with minimum values first.
        if (minimum == 0)
        {
            state.Append('*');
            return;
        }

        if (minimum == 1)
        {
            state.Append('+');
            return;
        }

        state.Append('{').Append(minimum).Append(",}");
    }

    private bool Accept<TInput>(Repetition<TInput>.Parser p, BnfStringifyVisitor state)
    {
        var children = p.GetChildren().ToArray();
        var item = children[0];
        var separator = children[1];

        AcceptRepetitionParser<TInput>(state, item, separator, p.Minimum, p.Maximum);
        return true;
    }

    private bool Accept<TInput, TOutput>(Repetition<TInput>.Parser<TOutput> p, BnfStringifyVisitor state)
    {
        var children = p.GetChildren().ToArray();
        var item = children[0];
        var separator = children[1];

        AcceptRepetitionParser<TInput>(state, item, separator, p.Minimum, p.Maximum);
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

    private bool Accept<TInput, TMiddle, TOutput>(RightApplyParser<TInput, TMiddle, TOutput> p, BnfStringifyVisitor state)
    {
        var children = p.GetChildren().ToArray();
        state.Append(children[0], " (", children[1], string.IsNullOrEmpty(p.Name) ? " SELF" : $" <{p.Name}>)*");
        return true;
    }

    private bool Accept<TInput, TOutput, TData>(RuleParser<TInput, TOutput, TData> p, BnfStringifyVisitor state)
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

    private bool Accept<TInput, TOutput>(Sequential.Parser<TInput, TOutput> _, BnfStringifyVisitor state)
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

    private bool Accept<TInput>(TryParser<TInput>.Parser p, BnfStringifyVisitor state)
    {
        state.Append("TRY ", p.GetChildren().First());
        return true;
    }

    private bool Accept<TInput, TOutput>(TryParser<TInput>.Parser<TOutput> p, BnfStringifyVisitor state)
    {
        state.Append("TRY ", p.GetChildren().First());
        return true;
    }

    private bool Accept<TInput, TOutput>(TryParser<TInput>.MultiParser<TOutput> p, BnfStringifyVisitor state)
    {
        state.Append("TRY ", p.GetChildren().First());
        return true;
    }
}

#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore RCS1163 // Unused parameter.

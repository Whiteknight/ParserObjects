using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ParserObjects;
using ParserObjects.Internal.Parsers;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Bnf;

public sealed class BuiltInTypesBnfStringifyVisitor : IBuiltInPartialVisitor<BnfStringifyState>
{
    // Uses a syntax inspired by W3C EBNF (https://www.w3.org/TR/REC-xml/#sec-notation) and Regex
    // (for extensions beyond what EBNF normally handles). This NOT intended for round-trip operations
    // or formal analysis purposes.
    // Some regex syntax:
    // (?= ) is "positive lookahead" syntax. We use it to show something doesn't consume input
    // (?! ) is "negative lookahead" syntax. We use it for non-follows situations

    public void Accept<TInput>(AndParser<TInput> p, BnfStringifyState state)
    {
        var children = p.GetChildren().ToArray();
        state.Append(children[0], " && ", children[1]);
    }

    public void Accept<TInput>(AnyParser<TInput> _, BnfStringifyState state)
    {
        state.Append('.');
    }

    public void Accept<TInput>(Cache<TInput>.Parser p, BnfStringifyState state)
    {
        state.Append("CACHED(", p.GetChildren().First(), ")");
    }

    public void Accept<TInput, TOutput>(Cache<TInput>.Parser<TOutput> p, BnfStringifyState state)
    {
        state.Append("CACHED(", p.GetChildren().First(), ")");
    }

    public void Accept<TInput, TOutput>(Cache<TInput>.MultiParser<TOutput> p, BnfStringifyState state)
    {
        state.Append("CACHED(", p.GetChildren().First(), ")");
    }

    public void Accept<TInput>(CaptureParser<TInput> p, BnfStringifyState state)
    {
        var children = p.GetChildren().ToArray();
        state.Append("(", children[0]);
        foreach (var child in children.Skip(1))
            state.Append(" ", child);

        state.Append(')');
    }

    private static void AcceptChain(IReadOnlyList<IParser> children, BnfStringifyState state)
    {
        if (children.Count == 1)
        {
            state.Append(children[0], "->CHAIN");
            return;
        }

        state.Append(children[0], "->CHAIN(", children[1]);
        for (int i = 2; i < children.Count; i++)
            state.Append(" ", children[i]);
        state.Append(")");
    }

    public void Accept<TInput, TOutput, TMiddle, TData>(Chain<TInput, TOutput>.Parser<TMiddle, TData> p, BnfStringifyState state)
    {
        AcceptChain(p.GetChildren().ToList(), state);
    }

    public void Accept<TInput, TOutput, TData>(Chain<TInput, TOutput>.Parser<TData> p, BnfStringifyState state)
    {
        AcceptChain(p.GetChildren().ToList(), state);
    }

    public void Accept<TInput, TOutput>(Context<TInput>.MultiParser<TOutput> p, BnfStringifyState state)
    {
        state.Append(p.GetChildren().Single());
    }

    public void Accept<TInput, TOutput>(Context<TInput>.Parser<TOutput> p, BnfStringifyState state)
    {
        state.Append(p.GetChildren().Single());
    }

    public void Accept<TInput, TMulti, TOutput>(ContinueWith<TInput, TMulti, TOutput>.MultiParser p, BnfStringifyState state)
    {
        var children = p.GetChildren().ToList();
        state.Append(children[0], " CONTINUEWITH ", children[1]);
    }

    public void Accept<TInput, TMulti, TOutput>(ContinueWith<TInput, TMulti, TOutput>.SingleParser p, BnfStringifyState state)
    {
        var children = p.GetChildren().ToList();
        state.Append(children[0], " CONTINUEWITH ", children[1]);
    }

    public void Accept<TInput, TOutput>(Create<TInput, TOutput>.Parser _, BnfStringifyState state)
    {
        state.Append("CREATE");
    }

    public void Accept<TInput, TOutput>(Create<TInput, TOutput>.MultiParser _, BnfStringifyState state)
    {
        state.Append("CREATE");
    }

    public void Accept<TInput, TOutput>(DataFrame<TInput>.Parser<TOutput> p, BnfStringifyState state)
    {
        state.Append(p.GetChildren().First());
    }

    public void Accept<TInput, TOutput>(DataFrame<TInput>.MultiParser<TOutput> p, BnfStringifyState state)
    {
        state.Append(p.GetChildren().First());
    }

    public void Accept<TInput, TOutput>(Deferred<TInput, TOutput>.Parser p, BnfStringifyState state)
    {
        state.Append(p.GetChildren().First());
    }

    public void Accept<TInput, TOutput>(Deferred<TInput, TOutput>.MultiParser p, BnfStringifyState state)
    {
        state.Append(p.GetChildren().First());
    }

    public void Accept<TInput, TOutput>(EachParser<TInput, TOutput> p, BnfStringifyState state)
    {
        var children = p.GetChildren().ToList();
        if (children.Count == 1)
        {
            state.Append(children[0]);
            return;
        }

        state.Append("EACH(", children[0]);

        for (int i = 1; i <= children.Count - 1; i++)
            state.Append(" | ", children[i]);

        state.Append(')');
    }

    public void Accept<TInput, TOutput>(Earley<TInput, TOutput>.Parser p, BnfStringifyState state)
    {
        state.Append(p.GetBnf(state));
    }

    public void Accept<TInput>(EmptyParser<TInput> _, BnfStringifyState state)
    {
        state.Append("()");
    }

    public void Accept<TInput>(EndParser<TInput> _, BnfStringifyState state)
    {
        state.Append("END");
    }

    public void Accept<TInput>(ExamineParser<TInput> p, BnfStringifyState state)
    {
        state.Append(p.GetChildren().First());
    }

    public void Accept<TInput, TOutput>(Examine<TInput, TOutput>.Parser p, BnfStringifyState state)
    {
        state.Append(p.GetChildren().First());
    }

    public void Accept<TInput, TOutput>(Examine<TInput, TOutput>.MultiParser p, BnfStringifyState state)
    {
        state.Append(p.GetChildren().First());
    }

    public void Accept<TInput, TOutput>(FailParser<TInput, TOutput> p, BnfStringifyState state)
    {
        state.Append("FAIL");
    }

    private void AcceptFirstVariant(IReadOnlyList<IParser> children, BnfStringifyState state)
    {
        Debug.Assert(children.Count >= 2, "We should not have a First with 0 or 1 children");

        state.Append("(", children[0]);

        for (int i = 1; i <= children.Count - 1; i++)
            state.Append(" | ", children[i]);

        state.Append(')');
    }

    public void Accept<TInput>(FirstParser<TInput>.WithoutOutput p, BnfStringifyState state)
        => AcceptFirstVariant(p.GetChildren().ToList(), state);

    public void Accept<TInput, TOutput>(FirstParser<TInput>.WithOutput<TOutput> p, BnfStringifyState state)
        => AcceptFirstVariant(p.GetChildren().ToList(), state);

    // Includes all variants of Function<T>.Parser, Function<TIn, TOut>.Parser, .MultiParser, etc
    private static void AcceptFunctionVariant(string? description, IReadOnlyList<IParser> children, BnfStringifyState state)
    {
        if (string.IsNullOrEmpty(description))
        {
            // There is not currently a way to have children but no description. The Function()
            // method doesn't take children, and all internal uses of Function always fill in
            // description. In the future if we have other combinations we can expand this method.
            Debug.Assert(children.Count == 0, "There is not currently any supported way to have children but no description");
            state.Append("User Function");
            return;
        }

        var parts = description!.Split(new[] { "{child}" }, children.Count + 1, StringSplitOptions.None);
        state.Append(parts[0]);

        for (int i = 1; i < parts.Length; i++)
            state.Append(children[i - 1], parts[i]);
    }

    public void Accept<TInput, TData>(Function<TInput>.Parser<TData> p, BnfStringifyState state)
    {
        AcceptFunctionVariant(p.Description, p.GetChildren().ToList(), state);
    }

    public void Accept<TInput, TOutput, TData>(Function<TInput, TOutput>.Parser<TData> p, BnfStringifyState state)
    {
        AcceptFunctionVariant(p.Description, p.GetChildren().ToList(), state);
    }

    public void Accept<TInput, TOutput, TData>(Function<TInput, TOutput>.MultiParser<TData> p, BnfStringifyState state)
    {
        AcceptFunctionVariant(p.Description, p.GetChildren().ToList(), state);
    }

    public void Accept<TInput, TOutput>(IfParser<TInput, TOutput> p, BnfStringifyState state)
    {
        var children = p.GetChildren().ToArray();
        state.Append("IF ", children[0], " THEN ", children[1], " ELSE ", children[2]);
    }

    public void Accept<TInput, TOutput>(LeftApplyParser<TInput, TOutput> p, BnfStringifyState state)
    {
        var children = p.GetChildren().ToArray();
        var initial = children[0];
        var middle = children[1];
        state.Append(middle, " | ", initial);
    }

    public void Accept<TInput>(MatchItemParser<TInput> p, BnfStringifyState state)
    {
        state.Append("'", (object?)p.Item ?? ' ', "'");
    }

    public void Accept<TInput>(MatchPredicateParser<TInput> _, BnfStringifyState state)
    {
        state.Append("MATCH()");
    }

    public void Accept<TInput>(MatchPatternParser<TInput> p, BnfStringifyState state)
    {
        var pattern = string.Join(" ", p.Pattern.Select(i => $"'{i}'"));
        state.Append(pattern);
    }

    public void Accept<TInput>(NegativeLookaheadParser<TInput> p, BnfStringifyState state)
    {
        state.Append("(?! ", p.GetChildren().First(), " )");
    }

    public void Accept<TInput, TItem, TOutput>(NonGreedyListParser<TInput, TItem, TOutput> p, BnfStringifyState state)
    {
        var children = p.GetChildren().ToArray();
        var item = children[0];
        var separator = children[1];
        var continuation = children[2];

        state.Append(item);

        if (separator is not EmptyParser<TInput>)
            state.Append(" (", separator, " ", item, ")");

        static void WriteQuantifier(BnfStringifyState v, int minimum, int? maximum)
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
                    v.Append('{').Append(minimum).Append("}?");
                    return;
                }

                v.Append('{').Append(minimum).Append(", ").Append(maximum).Append("}?");
                return;
            }

            // No maximum, so handle special cases with minimum values first.
            if (minimum == 0)
            {
                v.Append("*?");
                return;
            }

            if (minimum == 1)
            {
                v.Append("+?");
                return;
            }

            v.Append('{').Append(minimum).Append(",}?");
        }

        WriteQuantifier(state, p.Minimum, p.Maximum);
        state.Append(" ", continuation);
    }

    public void Accept<TInput, TOutput>(Optional<TInput, TOutput>.DefaultValueParser p, BnfStringifyState state)
    {
        state.Append(p.GetChildren().First(), "?");
    }

    public void Accept<TInput, TOutput>(Optional<TInput, TOutput>.NoDefaultParser p, BnfStringifyState state)
    {
        state.Append(p.GetChildren().First(), "?");
    }

    public void Accept<TInput>(PeekParser<TInput> _, BnfStringifyState state)
    {
        state.Append("(?=.)");
    }

    public void Accept<TInput>(PositiveLookaheadParser<TInput> p, BnfStringifyState state)
    {
        state.Append("(?= ", p.GetChildren().First(), " )");
    }

    public void Accept<TInput, TOutput>(PrattParser<TInput, TOutput> p, BnfStringifyState state)
    {
        var children = p.GetChildren().ToArray();
        if (children.Length == 0)
        {
            state.Append("PRATT()");
            return;
        }

        state.Append("PRATT(", children[0]);
        for (int i = 1; i < children.Length; i++)
            state.Append(", ", children[i]);

        state.Append(')');
    }

    public void Accept(RegexParser p, BnfStringifyState state)
    {
        state.Append("/", p.Pattern, "/");
    }

    private static void AcceptRepetitionParser<TInput>(BnfStringifyState state, IParser item, IParser separator, int minimum, int? maximum)
    {
        state.Append(item);

        if (separator is not EmptyParser<TInput>)
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

    public void Accept<TInput>(Repetition<TInput>.Parser p, BnfStringifyState state)
    {
        var children = p.GetChildren().ToArray();
        var item = children[0];
        var separator = children[1];

        AcceptRepetitionParser<TInput>(state, item, separator, p.Minimum, p.Maximum);
    }

    public void Accept<TInput, TOutput>(Repetition<TInput>.Parser<TOutput> p, BnfStringifyState state)
    {
        var children = p.GetChildren().ToArray();
        var item = children[0];
        var separator = children[1];

        AcceptRepetitionParser<TInput>(state, item, separator, p.Minimum, p.Maximum);
    }

    public void Accept<TInput>(Replaceable<TInput>.SingleParser p, BnfStringifyState state)
    {
        state.Append(p.GetChildren().First());
    }

    public void Accept<TInput, TOutput>(Replaceable<TInput, TOutput>.SingleParser p, BnfStringifyState state)
    {
        state.Append(p.GetChildren().First());
    }

    public void Accept<TInput, TOutput>(Replaceable<TInput, TOutput>.MultiParser p, BnfStringifyState state)
    {
        state.Append(p.GetChildren().First());
    }

    public void Accept<TInput, TMiddle, TOutput>(RightApplyParser<TInput, TMiddle, TOutput> p, BnfStringifyState state)
    {
        var children = p.GetChildren().ToArray();
        state.Append(children[0], " (", children[1], string.IsNullOrEmpty(p.Name) ? " SELF" : $" <{p.Name}>)*");
    }

    public void Accept<TInput, TOutput, TData>(RuleParser<TInput, TOutput, TData> p, BnfStringifyState state)
    {
        var children = p.GetChildren().ToArray();
        state.Append("(", children[0]);
        foreach (var child in children.Skip(1))
            state.Append(" ", child);

        state.Append(')');
    }

    public void Accept<TInput, TOutput>(SelectParser<TInput, TOutput> p, BnfStringifyState state)
    {
        state.Append("SELECT ", p.GetChildren().Single());
    }

    public void Accept<TInput, TOutput>(Sequential.Parser<TInput, TOutput> _, BnfStringifyState state)
    {
        state.Append("User Function");
    }

    public void Accept<TInput, TOutput>(SynchronizeParser<TInput, TOutput> p, BnfStringifyState state)
    {
        state.Append(p.GetChildren().First());
    }

    public void Accept<TInput, TMiddle, TOutput>(Transform<TInput, TMiddle, TOutput>.Parser p, BnfStringifyState state)
    {
        state.Append(p.GetChildren().First());
    }

    public void Accept<TInput, TMiddle, TOutput>(Transform<TInput, TMiddle, TOutput>.MultiParser p, BnfStringifyState state)
    {
        state.Append(p.GetChildren().First());
    }

    public void Accept<TInput, TOutput>(TrieParser<TInput, TOutput> p, BnfStringifyState state)
    {
        var allPatterns = p.Trie.GetAllPatterns().ToList();
        if (allPatterns.Count == 0)
        {
            state.Append("()");
            return;
        }

        static void PrintPattern(IEnumerable<TInput> pattern, BnfStringifyState s)
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
    }

    public void Accept<TInput>(TryParser<TInput>.Parser p, BnfStringifyState state)
    {
        state.Append("TRY ", p.GetChildren().First());
    }

    public void Accept<TInput, TOutput>(TryParser<TInput>.Parser<TOutput> p, BnfStringifyState state)
    {
        state.Append("TRY ", p.GetChildren().First());
    }

    public void Accept<TInput, TOutput>(TryParser<TInput>.MultiParser<TOutput> p, BnfStringifyState state)
    {
        state.Append("TRY ", p.GetChildren().First());
    }
}

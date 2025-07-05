using System;
using System.Collections.Generic;
using System.Linq;
using static ParserObjects.Internal.Assert;

namespace ParserObjects.Internal.Visitors;

/// <summary>
/// Parser-visitor type to traverse the parser tree and find matching parser nodes.
/// </summary>
public static class FindParserVisitor
{
    private readonly struct State
    {
        public Func<IParser, bool> Predicate { get; }
        public bool JustOne { get; }
        public List<IParser> Found { get; }
        public HashSet<IParser> Seen { get; }

        public bool CanStop => JustOne && Found.Count > 0;

        public State(Func<IParser, bool> predicate, bool justOne)
        {
            Predicate = NotNull(predicate);
            JustOne = justOne;
            Found = [];
            Seen = [];
        }
    }

    private static Option<IParser> FindSingle(IParser root, Func<IParser, bool> predicate)
    {
        NotNull(root);
        var state = new State(predicate, true);
        Visit(root, state);
        return state.Found.Count > 0
            ? new Option<IParser>(true, state.Found[0])
            : default;
    }

    /// <summary>
    /// Search for a parser with the given Name. Returns only the first result in case of
    /// duplicates.
    /// </summary>
    /// <param name="root"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Option<IParser> Named(IParser root, string name)
    {
        NotNullOrEmpty(name);
        return FindSingle(root, p => p.Name == name);
    }

    /// <summary>
    /// Search for a parser with the given Id.
    /// </summary>
    /// <param name="root"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Option<IParser> ById(IParser root, int id)
        => FindSingle(root, p => p.Id == id);

    /// <summary>
    /// Search for all parsers of the given type. Returns all results.
    /// </summary>
    /// <typeparam name="TParser"></typeparam>
    /// <param name="root"></param>
    /// <returns></returns>
    public static IReadOnlyList<TParser> OfType<TParser>(IParser root)
        where TParser : IParser
    {
        NotNull(root);
        var state = new State(p => p is TParser, false);
        Visit(root, state);
        return state.Found.Cast<TParser>().ToList();
    }

    /// <summary>
    /// Search for ReplaceableParsers matching a predicate and attempt to replace their contents with the
    /// replacement parser if it is found. The replacement parser must be non-null and of the correct
    /// type. Replaces all matching instances.
    /// </summary>
    /// <param name="root"></param>
    /// <param name="predicate"></param>
    /// <param name="replacement"></param>
    /// <returns></returns>
    public static MultiReplaceResult Replace(
        IParser root,
        Func<IReplaceableParserUntyped, bool> predicate,
        IParser replacement
    )
    {
        if (root == null || predicate == null || replacement == null)
            return MultiReplaceResult.Failure();
        var state = new State(p => p is IReplaceableParserUntyped replaceable && predicate(replaceable), true);
        Visit(root, state);
        var results = new List<SingleReplaceResult>();
        foreach (var found in state.Found.Cast<IReplaceableParserUntyped>())
        {
            var result = found.SetParser(replacement);
            results.Add(result);
        }

        return new MultiReplaceResult(results);
    }

    /// <summary>
    /// Search for ReplaceableParsers with the given name and attempt to replace their contents with the
    /// replacement parser. The replacement parser must be non-null and of the correct type. Replaces
    /// all matching instances.
    /// </summary>
    /// <param name="root"></param>
    /// <param name="name"></param>
    /// <param name="replacement"></param>
    /// <returns></returns>
    public static MultiReplaceResult Replace(IParser root, string name, IParser replacement)
        => Replace(root, p => p.Name == name, replacement);

    /// <summary>
    /// Search for ReplaceableParsers matching a predicate and attempt to transform the contents using
    /// the given transformation. The contents of the ReplaceableParser will be replaced with the
    /// transformed result if it is new and valid.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="root"></param>
    /// <param name="predicate"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static MultiReplaceResult Replace<TInput, TOutput>(
        IParser root,
        Func<IReplaceableParserUntyped, bool> predicate,
        GetParserFromParser<TInput, TOutput> transform
    )
    {
        if (root == null || predicate == null || transform == null)
            return MultiReplaceResult.Failure();
        var state = new State(p => p is IReplaceableParserUntyped replaceable && predicate(replaceable), true);
        Visit(root, state);
        var results = new List<SingleReplaceResult>();
        foreach (var found in state.Found.Cast<IReplaceableParserUntyped>())
        {
            if (found.ReplaceableChild is not IParser<TInput, TOutput> parser)
                continue;
            var replacement = transform(parser);
            if (replacement == null || ReferenceEquals(replacement, parser))
                continue;
            var result = found.SetParser(replacement);
            results.Add(result);
        }

        return new MultiReplaceResult(results);
    }

    /// <summary>
    /// Search for ReplaceableParsers with the given name and attempt to transform the contents using
    /// the given transformation. The contents of the ReplaceableParser will be replaced with the
    /// transformed result if it is new and valid.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="root"></param>
    /// <param name="name"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static MultiReplaceResult Replace<TInput, TOutput>(
        IParser root,
        string name,
        GetParserFromParser<TInput, TOutput> transform
    ) => Replace(root, p => p.Name == name, transform);

    /// <summary>
    /// Search for ReplaceableParsers with the given name and attempt to transform the contents
    /// using the given transformation. The contents of the ReplaceableParser will be replaced
    /// with the transformed result if it is new and valid.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="root"></param>
    /// <param name="predicate"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static MultiReplaceResult ReplaceMulti<TInput, TOutput>(
        IParser root,
        Func<IReplaceableParserUntyped, bool> predicate,
        GetMultiParserFromMultiParser<TInput, TOutput> transform
    )
    {
        if (root == null || predicate == null || transform == null)
            return MultiReplaceResult.Failure();
        var state = new State(p => p is IReplaceableParserUntyped replaceable && predicate(replaceable), true);
        Visit(root, state);
        var results = new List<SingleReplaceResult>();
        foreach (var found in state.Found.Cast<IReplaceableParserUntyped>())
        {
            if (found.ReplaceableChild is not IMultiParser<TInput, TOutput> parser)
                continue;
            var replacement = transform(parser);
            if (replacement == null || ReferenceEquals(replacement, parser))
                continue;
            var result = found.SetParser(replacement);
            results.Add(result);
        }

        return new MultiReplaceResult(results);
    }

    /// <summary>
    /// Search for ReplaceableParsers with the given name and attempt to transform the contents
    /// using the given transformation. The contents of the ReplaceableParser will be replaced
    /// with the transformed result if it is new and valid.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="root"></param>
    /// <param name="name"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static MultiReplaceResult ReplaceMulti<TInput, TOutput>(
        IParser root,
        string name,
        GetMultiParserFromMultiParser<TInput, TOutput> transform
    ) => ReplaceMulti(root, p => p.Name == name, transform);

    private static void Visit(IParser parser, State state)
    {
        if (parser != null)
            VisitInternal(parser, state);
    }

    private static void VisitInternal(IParser parser, State state)
    {
        if (state.CanStop || state.Seen.Contains(parser))
            return;
        state.Seen.Add(parser);

        if (state.Predicate(parser))
        {
            state.Found.Add(parser);
            if (state.JustOne)
                return;
        }

        foreach (var child in parser.GetChildren())
        {
            VisitInternal(child, state);
            if (state.CanStop)
                break;
        }
    }
}

﻿using System;
using ParserObjects.Utility;
using ParserObjects.Visitors;

namespace ParserObjects
{
    public static class ParserFindReplaceExtensions
    {
        /// <summary>
        /// Recurse the tree searching for a parser with the given name. Returns a result with the
        /// parser if found, a failure flag otherwise.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IOption<IParser> FindNamed(this IParser root, string name) => FindParserVisitor.Named(name, root);

        /// <summary>
        /// Given a parser tree, replace all children of ReplaceableParsers matching the given
        /// predicate with the provided replacement parser. Returns information about which
        /// instances were replaced.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="predicate"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static MultiReplaceResult Replace(this IParser root, Func<IParser, bool> predicate, IParser replacement)
            => FindParserVisitor.Replace(root, predicate, replacement);

        /// <summary>
        /// Given a parser tree, find a ReplaceableParser with a child which is reference equal to the given
        /// find parser, and replaces it with the given replacement parser.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="find"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static MultiReplaceResult Replace(this IParser root, IParser find, IParser replacement)
            => Replace(root, p => ReferenceEquals((p as IReplaceableParserUntyped)?.ReplaceableChild, find), replacement);

        /// <summary>
        /// Given a parser tree, find a ReplaceableParsers matching a predicate and attempt to transform
        /// the contents using the given transformation. The contents of the ReplaceableParser will be
        /// replaced with the transformed result if it is new and valid.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="root"></param>
        /// <param name="predicate"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static MultiReplaceResult Replace<TInput, TOutput>(this IParser root, Func<IParser, bool> predicate, Func<IParser<TInput, TOutput>, IParser<TInput, TOutput>> transform)
            => FindParserVisitor.Replace(root, predicate, transform);

        /// <summary>
        /// Given a parser tree, find a ReplaceableParser with the given name and replace the child
        /// parser with the given replacement parser.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="name"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static MultiReplaceResult Replace(this IParser root, string name, IParser replacement)
            => FindParserVisitor.Replace(root, name, replacement);

        /// <summary>
        /// Given a parser graph, find a ReplaceableParser matching a predicate and attempt to
        /// transform the contents using the given transformation. The contents of the
        /// ReplaceableParser will be replaced with the transformed result if it is new and valid.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="root"></param>
        /// <param name="name"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static MultiReplaceResult Replace<TInput, TOutput>(this IParser root, string name, Func<IParser<TInput, TOutput>, IParser<TInput, TOutput>> transform)
            => FindParserVisitor.Replace(root, name, transform);
    }
}

﻿using System.Collections.Generic;
using ParserObjects.Pratt;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers.C;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Examples.Classes;

public static class ClassGrammar
{
    public static IParser<char, Definition> CreateParser()
    {
        var ws = Whitespace();
        var ows = OptionalWhitespace();

        var accessModifier = ows
            .Then(
                Trie<string>(trie => trie
                    .Add("public")
                    .Add("internal")
                    .Add("private")
                )
            )
            .Named("Access Modifier");

        var structureType = ows
            .Then(
                Trie<string>(trie => trie
                    .Add("class")
                    .Add("interface")
                    .Add("struct")
                )
            )
            .Named("Structure Type");

        var name = ows
            .Then(
                If(
                    (accessModifier, structureType).First(),
                    Fail<string>(),
                    Identifier()
                )
            )
            .Named("Name");

        var openBracket = ows.Then(Match('{')).Named("Open Bracket");
        var closeBracket = ows.Then(Match('}')).Named("Close Bracket");

        var definition = Pratt<Definition>(setup => setup
            .Add(name, p => p.Bind(0, (ctx, n) => new Definition(n.Value)))
            .Add(accessModifier, p => p
                .Bind(3, (ctx, am) =>
                {
                    var def = ctx.Parse();
                    if (string.IsNullOrEmpty(def.StructureType))
                        ctx.FailLevel($"Definition {def.Name} must declare as {am} {def.StructureType}, not the other way around");
                    if (!string.IsNullOrEmpty(def.AccessModifier))
                        ctx.FailLevel($"Definition {def.Name} already has an access modifier");
                    def.AccessModifier = am.Value;
                    return def;
                })
            )
            .Add(structureType, p => p
                .Bind(5, (ctx, st) =>
                {
                    var def = ctx.Parse();
                    if (!string.IsNullOrEmpty(def.StructureType))
                        ctx.FailLevel($"Definition {def.Name} already has a type");
                    if (!string.IsNullOrEmpty(def.AccessModifier))
                        ctx.FailLevel($"Definition {def.Name} must declare as {def.AccessModifier} {st.Value}, not the other way around");
                    def.StructureType = st.Value;
                    return def;
                })
            )
            .Add(openBracket, p => p
                .BindLeft(1, (ctx, defToken, _) =>
                {
                    var def = defToken.Value;

                    if (def.Children != null)
                        ctx.FailRule($"{def.Name} already has a body");

                    var children = new List<Definition>();
                    while (true)
                    {
                        var childResult = ctx.TryParse(0);
                        if (!childResult.Success)
                            break;
                        if (def.StructureType == "interface")
                            ctx.FailAll("Interfaces may not contain child classes/interfaces");
                        children.Add(childResult.Value);
                    }

                    def.Children = children;
                    ctx.Expect(closeBracket);
                    return def;
                })
            )
        );
        return definition;
    }
}

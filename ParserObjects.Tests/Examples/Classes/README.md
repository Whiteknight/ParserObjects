# C#-like Class Definition Parser

This parser parses a very small subset of a C#-like language, where classes, structs and interfaces can be declared. Members (constructors, methods, fields, properties) are not parsed. This parser demonstrates the use of the Pratt parser for handling non-expression grammars and using recursion and syntax-directed translation to build an abstract syntax tree.

## Whitespace Handling

There are several ways to handle insignificant whitespace in a parser. Some other examples use a two-phase parser with a lexical analysis phase which discards whitespace internally. Some examples show a two-phase parser where the lexer returns whitespace tokens, but those tokens are filtered out before being inserted into the parser. This example uses the following idiom to ignore whitespace:

```csharp
var openBracket = ows.Then(Match('{'));
```

The `ows` parser is the `OptionalWhitespace` parser, and `ws` is the non-optional `Whitespace` parser. The `.Then()` extension method is a short-hand for the `If` parser. That line above is equivalent to this one:

```csharp
var openBracket = If(ows, Match('{'));
```

The `ows` parser, because it is optional, always succeeds. The `If` parser parses this value, discards it, and then returns the result of the `Match('{')` parser directly. Another way to accomplish this same thing, in a way that is a little bit more readable but also more verbose is to use the `Rule` parser:

```csharp
var openBracket = Rule(
    ows,
    Match('{'),
    (_, t) => t
)
```

Notice in the production callback `(_, t) => t` that the value of the first parser (`ows`) is discarded and only the value of the second parser is kept.

## Identifiers

This parser uses the C-style `Identifier` parser to parse class and interface names. The problem with this parser is that words like "public", "protected" and "interface" all parse as identifiers. If we don't want our parser to treat these keywords like regular identifiers, we have to filter them out. This line of code parses identifiers but not keywords:

```csharp
var name = ows.Then(
    If((accessModifier, structureType).First().None(), Fail<string>(), Identifier())
)
```

The first part of this, we have already seen, is the whitespace removal mechanism. Concentrating on the inner parser definition, we see this:

```csharp
If(
    (accessModifier, structureType).First(), 
    Fail<string>(), 
    Identifier()
)
```

This is an inversion of a normal `If` parser, where the parser returns failure if the predicate succeeds, and returns a value if the predicate fails. If we are able to parse one of our keywords at this position, it is *not an identifier* so the Name parser fails. If we are not able to parse a keyword at this position, then we can attempt to parse an Identifier. In either case, the attempt to parse the keyword consumes no input.

If we were using a two-phase parser, the lexer could do something like this to separate keywords from non-keyword identifiers:

```csharp
return new Token(value, IsKeyword(value) ? TokenType.Keyword : TokenType.Identifier);
```

Then in our parser we could try to match just an identifier token:

```csharp
var name = ows.Then(Match(t => t.Type == TokenType.Identifier));
```

In this scannerless design, we can rely on the fact that the `Trie` parser is sufficiently efficient that we can attempt to parse it before we attempt our Identifiers.

## Pratt Parser

The Pratt parser is the heart of this example. The parser wants to parse a `Definition`, which comes from the `name` (identifier) rule. A definition may have prefixes ("public", "class") and may have suffixes ("{", "}"). The Pratt parser as defined would allow these things to be parsed in a variety of orders, so we have to insert of checks inside the syntax-directed translation callbacks. So we see this line in one of the callbacks:

```csharp
if (string.IsNullOrEmpty(def.StructureType))
    ctx.Fail($"Definition {def.Name} must declare as {am} {def.StructureType}, not the other way around");
```

Because of the recursive nature of the parser, the `accessModifier` "public" will be seen first, it will recurse to the `structureType` value "class", and then will find the name. At that point the recursions unwind, filling in the `structureType` value and then filling in the `accessModifier` value. So, to verify that things are happening in the correct order, we check while filling in the `structureType` that we don't already have an `accessModifier` (remember it gets filled in later, despite appearing first in the input sequence), and while filling in the `accessModifier` value we check that we do have a `structureType`. In this way we can detect errors that the structure of the parser itself cannot detect for us.

One other limitation we have here is that an `interface` cannot contain child definitions. There is no good way to implement this check inside the parser as it's currently structured. Instead we add a validation phase with the `Definition.Validate()` method, which checks that an interface cannot have child definitions and fails the parse otherwise. When we call `ctx.Fail()` in that method when a child is found for an interface, the `openBracket` rule fails, but the parser returns success with the `Definition` object, with `definition.Children == null`. The validation step checks this condition and propagates errors to the test method.

## Required Components

One unit test in particular tells the story:

```csharp
var target = new ClassParser();
var result = target.Parse("class MyClass { }");
result.Should().NotBeNull();
result.AccessModifier.Should().BeNull();
```

While it is acceptable C# syntax for a class definition to not have an access modifier, there's no good way within the Pratt parser to require any particular prefix or suffix to exist. A validation step would have to be added after the parse to make sure all fields were properly initialized. The same limitation is the same for the "class" keyword and the `{ }` class body. The parser will report success despite missing any or all of these.
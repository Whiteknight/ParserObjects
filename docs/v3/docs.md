<a name='assembly'></a>
# ParserObjects

## Contents

- [AlwaysFullRingBuffer\`1](#T-ParserObjects-Utility-AlwaysFullRingBuffer`1 'ParserObjects.Utility.AlwaysFullRingBuffer`1')
- [AndParser\`1](#T-ParserObjects-Parsers-AndParser`1 'ParserObjects.Parsers.AndParser`1')
- [AnyParser\`1](#T-ParserObjects-Parsers-AnyParser`1 'ParserObjects.Parsers.AnyParser`1')
- [BnfStringifyVisitor](#T-ParserObjects-Visitors-BnfStringifyVisitor 'ParserObjects.Visitors.BnfStringifyVisitor')
- [CPlusPlusStyleParserMethods](#T-ParserObjects-CPlusPlusStyleParserMethods 'ParserObjects.CPlusPlusStyleParserMethods')
  - [Comment()](#M-ParserObjects-CPlusPlusStyleParserMethods-Comment 'ParserObjects.CPlusPlusStyleParserMethods.Comment')
- [CStyleParserMethods](#T-ParserObjects-CStyleParserMethods 'ParserObjects.CStyleParserMethods')
  - [Comment()](#M-ParserObjects-CStyleParserMethods-Comment 'ParserObjects.CStyleParserMethods.Comment')
  - [Double()](#M-ParserObjects-CStyleParserMethods-Double 'ParserObjects.CStyleParserMethods.Double')
  - [DoubleString()](#M-ParserObjects-CStyleParserMethods-DoubleString 'ParserObjects.CStyleParserMethods.DoubleString')
  - [HexadecimalInteger()](#M-ParserObjects-CStyleParserMethods-HexadecimalInteger 'ParserObjects.CStyleParserMethods.HexadecimalInteger')
  - [HexadecimalString()](#M-ParserObjects-CStyleParserMethods-HexadecimalString 'ParserObjects.CStyleParserMethods.HexadecimalString')
  - [Identifier()](#M-ParserObjects-CStyleParserMethods-Identifier 'ParserObjects.CStyleParserMethods.Identifier')
  - [Integer()](#M-ParserObjects-CStyleParserMethods-Integer 'ParserObjects.CStyleParserMethods.Integer')
  - [IntegerString()](#M-ParserObjects-CStyleParserMethods-IntegerString 'ParserObjects.CStyleParserMethods.IntegerString')
  - [UnsignedInteger()](#M-ParserObjects-CStyleParserMethods-UnsignedInteger 'ParserObjects.CStyleParserMethods.UnsignedInteger')
  - [UnsignedIntegerString()](#M-ParserObjects-CStyleParserMethods-UnsignedIntegerString 'ParserObjects.CStyleParserMethods.UnsignedIntegerString')
- [DeferredParser\`2](#T-ParserObjects-Parsers-DeferredParser`2 'ParserObjects.Parsers.DeferredParser`2')
- [EmptyParser\`1](#T-ParserObjects-Parsers-EmptyParser`1 'ParserObjects.Parsers.EmptyParser`1')
- [EndParser\`1](#T-ParserObjects-Parsers-EndParser`1 'ParserObjects.Parsers.EndParser`1')
- [EnumerableExtensions](#T-ParserObjects-Sequences-EnumerableExtensions 'ParserObjects.Sequences.EnumerableExtensions')
  - [ToSequence\`\`1(enumerable,endValue)](#M-ParserObjects-Sequences-EnumerableExtensions-ToSequence``1-System-Collections-Generic-IEnumerable{``0},``0- 'ParserObjects.Sequences.EnumerableExtensions.ToSequence``1(System.Collections.Generic.IEnumerable{``0},``0)')
  - [ToSequence\`\`1(enumerable,getEndValue)](#M-ParserObjects-Sequences-EnumerableExtensions-ToSequence``1-System-Collections-Generic-IEnumerable{``0},System-Func{``0}- 'ParserObjects.Sequences.EnumerableExtensions.ToSequence``1(System.Collections.Generic.IEnumerable{``0},System.Func{``0})')
- [EnumerableSequence\`1](#T-ParserObjects-Sequences-EnumerableSequence`1 'ParserObjects.Sequences.EnumerableSequence`1')
- [FailParser\`2](#T-ParserObjects-Parsers-FailParser`2 'ParserObjects.Parsers.FailParser`2')
- [FilterSequence\`1](#T-ParserObjects-Sequences-FilterSequence`1 'ParserObjects.Sequences.FilterSequence`1')
- [FindParserVisitor](#T-ParserObjects-Visitors-FindParserVisitor 'ParserObjects.Visitors.FindParserVisitor')
  - [Named(name,root)](#M-ParserObjects-Visitors-FindParserVisitor-Named-System-String,ParserObjects-IParser- 'ParserObjects.Visitors.FindParserVisitor.Named(System.String,ParserObjects.IParser)')
  - [OfType\`\`1(root)](#M-ParserObjects-Visitors-FindParserVisitor-OfType``1-ParserObjects-IParser- 'ParserObjects.Visitors.FindParserVisitor.OfType``1(ParserObjects.IParser)')
  - [Replace(root,predicate,replacement)](#M-ParserObjects-Visitors-FindParserVisitor-Replace-ParserObjects-IParser,System-Func{ParserObjects-IReplaceableParserUntyped,System-Boolean},ParserObjects-IParser- 'ParserObjects.Visitors.FindParserVisitor.Replace(ParserObjects.IParser,System.Func{ParserObjects.IReplaceableParserUntyped,System.Boolean},ParserObjects.IParser)')
  - [Replace(root,name,replacement)](#M-ParserObjects-Visitors-FindParserVisitor-Replace-ParserObjects-IParser,System-String,ParserObjects-IParser- 'ParserObjects.Visitors.FindParserVisitor.Replace(ParserObjects.IParser,System.String,ParserObjects.IParser)')
  - [Replace\`\`2(root,predicate,transform)](#M-ParserObjects-Visitors-FindParserVisitor-Replace``2-ParserObjects-IParser,System-Func{ParserObjects-IReplaceableParserUntyped,System-Boolean},System-Func{ParserObjects-IParser{``0,``1},ParserObjects-IParser{``0,``1}}- 'ParserObjects.Visitors.FindParserVisitor.Replace``2(ParserObjects.IParser,System.Func{ParserObjects.IReplaceableParserUntyped,System.Boolean},System.Func{ParserObjects.IParser{``0,``1},ParserObjects.IParser{``0,``1}})')
  - [Replace\`\`2(root,name,transform)](#M-ParserObjects-Visitors-FindParserVisitor-Replace``2-ParserObjects-IParser,System-String,System-Func{ParserObjects-IParser{``0,``1},ParserObjects-IParser{``0,``1}}- 'ParserObjects.Visitors.FindParserVisitor.Replace``2(ParserObjects.IParser,System.String,System.Func{ParserObjects.IParser{``0,``1},ParserObjects.IParser{``0,``1}})')
- [FirstParser\`2](#T-ParserObjects-Parsers-FirstParser`2 'ParserObjects.Parsers.FirstParser`2')
- [FuncParser\`2](#T-ParserObjects-Parsers-FuncParser`2 'ParserObjects.Parsers.FuncParser`2')
- [IInsertableTrie\`2](#T-ParserObjects-IInsertableTrie`2 'ParserObjects.IInsertableTrie`2')
  - [Add(keys,result)](#M-ParserObjects-IInsertableTrie`2-Add-System-Collections-Generic-IEnumerable{`0},`1- 'ParserObjects.IInsertableTrie`2.Add(System.Collections.Generic.IEnumerable{`0},`1)')
- [IParser](#T-ParserObjects-IParser 'ParserObjects.IParser')
  - [Name](#P-ParserObjects-IParser-Name 'ParserObjects.IParser.Name')
  - [GetChildren()](#M-ParserObjects-IParser-GetChildren 'ParserObjects.IParser.GetChildren')
  - [ReplaceChild(find,replace)](#M-ParserObjects-IParser-ReplaceChild-ParserObjects-IParser,ParserObjects-IParser- 'ParserObjects.IParser.ReplaceChild(ParserObjects.IParser,ParserObjects.IParser)')
- [IParser\`1](#T-ParserObjects-IParser`1 'ParserObjects.IParser`1')
  - [Parse(state)](#M-ParserObjects-IParser`1-Parse-ParserObjects-ParseState{`0}- 'ParserObjects.IParser`1.Parse(ParserObjects.ParseState{`0})')
- [IParser\`2](#T-ParserObjects-IParser`2 'ParserObjects.IParser`2')
  - [Parse(state)](#M-ParserObjects-IParser`2-Parse-ParserObjects-ParseState{`0}- 'ParserObjects.IParser`2.Parse(ParserObjects.ParseState{`0})')
- [IReadOnlyTrie\`2](#T-ParserObjects-IReadOnlyTrie`2 'ParserObjects.IReadOnlyTrie`2')
  - [Get(keys)](#M-ParserObjects-IReadOnlyTrie`2-Get-System-Collections-Generic-IEnumerable{`0}- 'ParserObjects.IReadOnlyTrie`2.Get(System.Collections.Generic.IEnumerable{`0})')
  - [Get(keys)](#M-ParserObjects-IReadOnlyTrie`2-Get-ParserObjects-ISequence{`0}- 'ParserObjects.IReadOnlyTrie`2.Get(ParserObjects.ISequence{`0})')
  - [GetAllPatterns()](#M-ParserObjects-IReadOnlyTrie`2-GetAllPatterns 'ParserObjects.IReadOnlyTrie`2.GetAllPatterns')
- [IReplaceableParserUntyped](#T-ParserObjects-IReplaceableParserUntyped 'ParserObjects.IReplaceableParserUntyped')
  - [ReplaceableChild](#P-ParserObjects-IReplaceableParserUntyped-ReplaceableChild 'ParserObjects.IReplaceableParserUntyped.ReplaceableChild')
  - [SetParser(parser)](#M-ParserObjects-IReplaceableParserUntyped-SetParser-ParserObjects-IParser- 'ParserObjects.IReplaceableParserUntyped.SetParser(ParserObjects.IParser)')
- [IResult](#T-ParserObjects-IResult 'ParserObjects.IResult')
  - [Location](#P-ParserObjects-IResult-Location 'ParserObjects.IResult.Location')
  - [Success](#P-ParserObjects-IResult-Success 'ParserObjects.IResult.Success')
- [IResult\`1](#T-ParserObjects-IResult`1 'ParserObjects.IResult`1')
  - [Value](#P-ParserObjects-IResult`1-Value 'ParserObjects.IResult`1.Value')
  - [Transform\`\`1(transform)](#M-ParserObjects-IResult`1-Transform``1-System-Func{`0,``0}- 'ParserObjects.IResult`1.Transform``1(System.Func{`0,``0})')
- [ISequence](#T-ParserObjects-ISequence 'ParserObjects.ISequence')
  - [CurrentLocation](#P-ParserObjects-ISequence-CurrentLocation 'ParserObjects.ISequence.CurrentLocation')
  - [IsAtEnd](#P-ParserObjects-ISequence-IsAtEnd 'ParserObjects.ISequence.IsAtEnd')
- [ISequence\`1](#T-ParserObjects-ISequence`1 'ParserObjects.ISequence`1')
  - [GetNext()](#M-ParserObjects-ISequence`1-GetNext 'ParserObjects.ISequence`1.GetNext')
  - [Peek()](#M-ParserObjects-ISequence`1-Peek 'ParserObjects.ISequence`1.Peek')
  - [PutBack(value)](#M-ParserObjects-ISequence`1-PutBack-`0- 'ParserObjects.ISequence`1.PutBack(`0)')
- [IfParser\`2](#T-ParserObjects-Parsers-IfParser`2 'ParserObjects.Parsers.IfParser`2')
- [InsertOnlyTrie\`2](#T-ParserObjects-Utility-InsertOnlyTrie`2 'ParserObjects.Utility.InsertOnlyTrie`2')
- [JavaScriptStyleParserMethods](#T-ParserObjects-JavaScriptStyleParserMethods 'ParserObjects.JavaScriptStyleParserMethods')
  - [Number()](#M-ParserObjects-JavaScriptStyleParserMethods-Number 'ParserObjects.JavaScriptStyleParserMethods.Number')
  - [NumberString()](#M-ParserObjects-JavaScriptStyleParserMethods-NumberString 'ParserObjects.JavaScriptStyleParserMethods.NumberString')
- [LeftValueParser\`2](#T-ParserObjects-Parsers-LeftValueParser`2 'ParserObjects.Parsers.LeftValueParser`2')
- [Location](#T-ParserObjects-Location 'ParserObjects.Location')
  - [Column](#P-ParserObjects-Location-Column 'ParserObjects.Location.Column')
  - [FileName](#P-ParserObjects-Location-FileName 'ParserObjects.Location.FileName')
  - [Line](#P-ParserObjects-Location-Line 'ParserObjects.Location.Line')
- [MapSequence\`2](#T-ParserObjects-Sequences-MapSequence`2 'ParserObjects.Sequences.MapSequence`2')
- [MatchPatternParser\`1](#T-ParserObjects-Parsers-MatchPatternParser`1 'ParserObjects.Parsers.MatchPatternParser`1')
- [MatchPredicateParser\`1](#T-ParserObjects-Parsers-MatchPredicateParser`1 'ParserObjects.Parsers.MatchPredicateParser`1')
- [NegativeLookaheadParser\`1](#T-ParserObjects-Parsers-NegativeLookaheadParser`1 'ParserObjects.Parsers.NegativeLookaheadParser`1')
- [NotParser\`1](#T-ParserObjects-Parsers-NotParser`1 'ParserObjects.Parsers.NotParser`1')
- [OrParser\`1](#T-ParserObjects-Parsers-OrParser`1 'ParserObjects.Parsers.OrParser`1')
- [ParseResultSequence\`2](#T-ParserObjects-Sequences-ParseResultSequence`2 'ParserObjects.Sequences.ParseResultSequence`2')
- [ParserCombinatorExtensions](#T-ParserObjects-ParserCombinatorExtensions 'ParserObjects.ParserCombinatorExtensions')
  - [Examine\`\`2(parser,before,after)](#M-ParserObjects-ParserCombinatorExtensions-Examine``2-ParserObjects-IParser{``0,``1},System-Action{ParserObjects-Parsers-Examine{``0,``1}-Context},System-Action{ParserObjects-Parsers-Examine{``0,``1}-Context}- 'ParserObjects.ParserCombinatorExtensions.Examine``2(ParserObjects.IParser{``0,``1},System.Action{ParserObjects.Parsers.Examine{``0,``1}.Context},System.Action{ParserObjects.Parsers.Examine{``0,``1}.Context})')
  - [FollowedBy\`\`2(p,lookahead)](#M-ParserObjects-ParserCombinatorExtensions-FollowedBy``2-ParserObjects-IParser{``0,``1},ParserObjects-IParser{``0}- 'ParserObjects.ParserCombinatorExtensions.FollowedBy``2(ParserObjects.IParser{``0,``1},ParserObjects.IParser{``0})')
  - [ListCharToString(p,atLeastOne)](#M-ParserObjects-ParserCombinatorExtensions-ListCharToString-ParserObjects-IParser{System-Char,System-Char},System-Boolean- 'ParserObjects.ParserCombinatorExtensions.ListCharToString(ParserObjects.IParser{System.Char,System.Char},System.Boolean)')
  - [ListCharToString(p,minimum,maximum)](#M-ParserObjects-ParserCombinatorExtensions-ListCharToString-ParserObjects-IParser{System-Char,System-Char},System-Int32,System-Nullable{System-Int32}- 'ParserObjects.ParserCombinatorExtensions.ListCharToString(ParserObjects.IParser{System.Char,System.Char},System.Int32,System.Nullable{System.Int32})')
  - [ListSeparatedBy\`\`3(p,separator,atLeastOne)](#M-ParserObjects-ParserCombinatorExtensions-ListSeparatedBy``3-ParserObjects-IParser{``0,``2},ParserObjects-IParser{``0,``1},System-Boolean- 'ParserObjects.ParserCombinatorExtensions.ListSeparatedBy``3(ParserObjects.IParser{``0,``2},ParserObjects.IParser{``0,``1},System.Boolean)')
  - [ListSeparatedBy\`\`3(p,separator,minimum,maximum)](#M-ParserObjects-ParserCombinatorExtensions-ListSeparatedBy``3-ParserObjects-IParser{``0,``2},ParserObjects-IParser{``0,``1},System-Int32,System-Nullable{System-Int32}- 'ParserObjects.ParserCombinatorExtensions.ListSeparatedBy``3(ParserObjects.IParser{``0,``2},ParserObjects.IParser{``0,``1},System.Int32,System.Nullable{System.Int32})')
  - [ListStringsToString(p,atLeastOne)](#M-ParserObjects-ParserCombinatorExtensions-ListStringsToString-ParserObjects-IParser{System-Char,System-String},System-Boolean- 'ParserObjects.ParserCombinatorExtensions.ListStringsToString(ParserObjects.IParser{System.Char,System.String},System.Boolean)')
  - [List\`\`2(p,atLeastOne)](#M-ParserObjects-ParserCombinatorExtensions-List``2-ParserObjects-IParser{``0,``1},System-Boolean- 'ParserObjects.ParserCombinatorExtensions.List``2(ParserObjects.IParser{``0,``1},System.Boolean)')
  - [List\`\`2(p,minimum,maximum)](#M-ParserObjects-ParserCombinatorExtensions-List``2-ParserObjects-IParser{``0,``1},System-Int32,System-Nullable{System-Int32}- 'ParserObjects.ParserCombinatorExtensions.List``2(ParserObjects.IParser{``0,``1},System.Int32,System.Nullable{System.Int32})')
  - [Map\`\`3(parser,transform)](#M-ParserObjects-ParserCombinatorExtensions-Map``3-ParserObjects-IParser{``0,``1},System-Func{``1,``2}- 'ParserObjects.ParserCombinatorExtensions.Map``3(ParserObjects.IParser{``0,``1},System.Func{``1,``2})')
  - [NotFollowedBy\`\`2(p,lookahead)](#M-ParserObjects-ParserCombinatorExtensions-NotFollowedBy``2-ParserObjects-IParser{``0,``1},ParserObjects-IParser{``0}- 'ParserObjects.ParserCombinatorExtensions.NotFollowedBy``2(ParserObjects.IParser{``0,``1},ParserObjects.IParser{``0})')
  - [Optional\`\`2(p,getDefault)](#M-ParserObjects-ParserCombinatorExtensions-Optional``2-ParserObjects-IParser{``0,``1},System-Func{``1}- 'ParserObjects.ParserCombinatorExtensions.Optional``2(ParserObjects.IParser{``0,``1},System.Func{``1})')
  - [Optional\`\`2(p,getDefault)](#M-ParserObjects-ParserCombinatorExtensions-Optional``2-ParserObjects-IParser{``0,``1},System-Func{ParserObjects-ISequence{``0},``1}- 'ParserObjects.ParserCombinatorExtensions.Optional``2(ParserObjects.IParser{``0,``1},System.Func{ParserObjects.ISequence{``0},``1})')
  - [Replaceable\`\`2(p)](#M-ParserObjects-ParserCombinatorExtensions-Replaceable``2-ParserObjects-IParser{``0,``1}- 'ParserObjects.ParserCombinatorExtensions.Replaceable``2(ParserObjects.IParser{``0,``1})')
  - [Replaceable\`\`2(p,name)](#M-ParserObjects-ParserCombinatorExtensions-Replaceable``2-ParserObjects-IParser{``0,``1},System-String- 'ParserObjects.ParserCombinatorExtensions.Replaceable``2(ParserObjects.IParser{``0,``1},System.String)')
  - [Transform\`\`3(parser,transform)](#M-ParserObjects-ParserCombinatorExtensions-Transform``3-ParserObjects-IParser{``0,``1},System-Func{``1,``2}- 'ParserObjects.ParserCombinatorExtensions.Transform``3(ParserObjects.IParser{``0,``1},System.Func{``1,``2})')
- [ParserExtensions](#T-ParserObjects-ParserExtensions 'ParserObjects.ParserExtensions')
  - [Named\`\`1(parser,name)](#M-ParserObjects-ParserExtensions-Named``1-``0,System-String- 'ParserObjects.ParserExtensions.Named``1(``0,System.String)')
  - [ToBnf(parser)](#M-ParserObjects-ParserExtensions-ToBnf-ParserObjects-IParser- 'ParserObjects.ParserExtensions.ToBnf(ParserObjects.IParser)')
  - [ToSequence\`\`2(parser,input)](#M-ParserObjects-ParserExtensions-ToSequence``2-ParserObjects-IParser{``0,``1},ParserObjects-ISequence{``0}- 'ParserObjects.ParserExtensions.ToSequence``2(ParserObjects.IParser{``0,``1},ParserObjects.ISequence{``0})')
- [ParserFindReplaceExtensions](#T-ParserObjects-ParserFindReplaceExtensions 'ParserObjects.ParserFindReplaceExtensions')
  - [FindNamed(root,name)](#M-ParserObjects-ParserFindReplaceExtensions-FindNamed-ParserObjects-IParser,System-String- 'ParserObjects.ParserFindReplaceExtensions.FindNamed(ParserObjects.IParser,System.String)')
  - [Replace(root,predicate,replacement)](#M-ParserObjects-ParserFindReplaceExtensions-Replace-ParserObjects-IParser,System-Func{ParserObjects-IParser,System-Boolean},ParserObjects-IParser- 'ParserObjects.ParserFindReplaceExtensions.Replace(ParserObjects.IParser,System.Func{ParserObjects.IParser,System.Boolean},ParserObjects.IParser)')
  - [Replace(root,find,replacement)](#M-ParserObjects-ParserFindReplaceExtensions-Replace-ParserObjects-IParser,ParserObjects-IParser,ParserObjects-IParser- 'ParserObjects.ParserFindReplaceExtensions.Replace(ParserObjects.IParser,ParserObjects.IParser,ParserObjects.IParser)')
  - [Replace(root,name,replacement)](#M-ParserObjects-ParserFindReplaceExtensions-Replace-ParserObjects-IParser,System-String,ParserObjects-IParser- 'ParserObjects.ParserFindReplaceExtensions.Replace(ParserObjects.IParser,System.String,ParserObjects.IParser)')
  - [Replace\`\`2(root,predicate,transform)](#M-ParserObjects-ParserFindReplaceExtensions-Replace``2-ParserObjects-IParser,System-Func{ParserObjects-IParser,System-Boolean},System-Func{ParserObjects-IParser{``0,``1},ParserObjects-IParser{``0,``1}}- 'ParserObjects.ParserFindReplaceExtensions.Replace``2(ParserObjects.IParser,System.Func{ParserObjects.IParser,System.Boolean},System.Func{ParserObjects.IParser{``0,``1},ParserObjects.IParser{``0,``1}})')
  - [Replace\`\`2(root,name,transform)](#M-ParserObjects-ParserFindReplaceExtensions-Replace``2-ParserObjects-IParser,System-String,System-Func{ParserObjects-IParser{``0,``1},ParserObjects-IParser{``0,``1}}- 'ParserObjects.ParserFindReplaceExtensions.Replace``2(ParserObjects.IParser,System.String,System.Func{ParserObjects.IParser{``0,``1},ParserObjects.IParser{``0,``1}})')
- [ParserLogicalExtensions](#T-ParserObjects-ParserLogicalExtensions 'ParserObjects.ParserLogicalExtensions')
  - [And\`\`1(p1,parsers)](#M-ParserObjects-ParserLogicalExtensions-And``1-ParserObjects-IParser{``0},ParserObjects-IParser{``0}[]- 'ParserObjects.ParserLogicalExtensions.And``1(ParserObjects.IParser{``0},ParserObjects.IParser{``0}[])')
  - [If\`\`2(parser,predicate)](#M-ParserObjects-ParserLogicalExtensions-If``2-ParserObjects-IParser{``0,``1},ParserObjects-IParser{``0}- 'ParserObjects.ParserLogicalExtensions.If``2(ParserObjects.IParser{``0,``1},ParserObjects.IParser{``0})')
  - [Not\`\`1(p1)](#M-ParserObjects-ParserLogicalExtensions-Not``1-ParserObjects-IParser{``0}- 'ParserObjects.ParserLogicalExtensions.Not``1(ParserObjects.IParser{``0})')
  - [Or\`\`1(p1,parsers)](#M-ParserObjects-ParserLogicalExtensions-Or``1-ParserObjects-IParser{``0},ParserObjects-IParser{``0}[]- 'ParserObjects.ParserLogicalExtensions.Or``1(ParserObjects.IParser{``0},ParserObjects.IParser{``0}[])')
  - [Then\`\`2(predicate,parser)](#M-ParserObjects-ParserLogicalExtensions-Then``2-ParserObjects-IParser{``0},ParserObjects-IParser{``0,``1}- 'ParserObjects.ParserLogicalExtensions.Then``2(ParserObjects.IParser{``0},ParserObjects.IParser{``0,``1})')
- [ParserMatchParseExtensions](#T-ParserObjects-ParserMatchParseExtensions 'ParserObjects.ParserMatchParseExtensions')
  - [CanMatch(parser,input)](#M-ParserObjects-ParserMatchParseExtensions-CanMatch-ParserObjects-IParser{System-Char},System-String- 'ParserObjects.ParserMatchParseExtensions.CanMatch(ParserObjects.IParser{System.Char},System.String)')
  - [CanMatch\`\`1(parser,input)](#M-ParserObjects-ParserMatchParseExtensions-CanMatch``1-ParserObjects-IParser{``0},ParserObjects-ISequence{``0}- 'ParserObjects.ParserMatchParseExtensions.CanMatch``1(ParserObjects.IParser{``0},ParserObjects.ISequence{``0})')
  - [Parse\`\`1(parser,s)](#M-ParserObjects-ParserMatchParseExtensions-Parse``1-ParserObjects-IParser{System-Char,``0},System-String- 'ParserObjects.ParserMatchParseExtensions.Parse``1(ParserObjects.IParser{System.Char,``0},System.String)')
- [ParserMethods](#T-ParserObjects-ParserMethods 'ParserObjects.ParserMethods')
  - [CamelCase()](#M-ParserObjects-ParserMethods-CamelCase 'ParserObjects.ParserMethods.CamelCase')
  - [CharacterString(pattern)](#M-ParserObjects-ParserMethods-CharacterString-System-String- 'ParserObjects.ParserMethods.CharacterString(System.String)')
  - [DelimitedStringWithEscapedDelimiters(openStr,closeStr,escapeStr)](#M-ParserObjects-ParserMethods-DelimitedStringWithEscapedDelimiters-System-Char,System-Char,System-Char- 'ParserObjects.ParserMethods.DelimitedStringWithEscapedDelimiters(System.Char,System.Char,System.Char)')
  - [Digit()](#M-ParserObjects-ParserMethods-Digit 'ParserObjects.ParserMethods.Digit')
  - [DigitString()](#M-ParserObjects-ParserMethods-DigitString 'ParserObjects.ParserMethods.DigitString')
  - [DoubleQuotedString()](#M-ParserObjects-ParserMethods-DoubleQuotedString 'ParserObjects.ParserMethods.DoubleQuotedString')
  - [HexadecimalDigit()](#M-ParserObjects-ParserMethods-HexadecimalDigit 'ParserObjects.ParserMethods.HexadecimalDigit')
  - [HexadecimalString()](#M-ParserObjects-ParserMethods-HexadecimalString 'ParserObjects.ParserMethods.HexadecimalString')
  - [Line()](#M-ParserObjects-ParserMethods-Line 'ParserObjects.ParserMethods.Line')
  - [NonZeroDigit()](#M-ParserObjects-ParserMethods-NonZeroDigit 'ParserObjects.ParserMethods.NonZeroDigit')
  - [OptionalWhitespace()](#M-ParserObjects-ParserMethods-OptionalWhitespace 'ParserObjects.ParserMethods.OptionalWhitespace')
  - [PrefixedLine(prefix)](#M-ParserObjects-ParserMethods-PrefixedLine-System-String- 'ParserObjects.ParserMethods.PrefixedLine(System.String)')
  - [Regex(pattern)](#M-ParserObjects-ParserMethods-Regex-System-String- 'ParserObjects.ParserMethods.Regex(System.String)')
  - [SingleQuotedString()](#M-ParserObjects-ParserMethods-SingleQuotedString 'ParserObjects.ParserMethods.SingleQuotedString')
  - [StrippedDelimitedStringWithEscapedDelimiters(openStr,closeStr,escapeStr)](#M-ParserObjects-ParserMethods-StrippedDelimitedStringWithEscapedDelimiters-System-Char,System-Char,System-Char- 'ParserObjects.ParserMethods.StrippedDelimitedStringWithEscapedDelimiters(System.Char,System.Char,System.Char)')
  - [StrippedDoubleQuotedString()](#M-ParserObjects-ParserMethods-StrippedDoubleQuotedString 'ParserObjects.ParserMethods.StrippedDoubleQuotedString')
  - [StrippedSingleQuotedString()](#M-ParserObjects-ParserMethods-StrippedSingleQuotedString 'ParserObjects.ParserMethods.StrippedSingleQuotedString')
  - [Whitespace()](#M-ParserObjects-ParserMethods-Whitespace 'ParserObjects.ParserMethods.Whitespace')
  - [WhitespaceCharacter()](#M-ParserObjects-ParserMethods-WhitespaceCharacter 'ParserObjects.ParserMethods.WhitespaceCharacter')
- [ParserMethods\`1](#T-ParserObjects-ParserMethods`1 'ParserObjects.ParserMethods`1')
  - [And(parsers)](#M-ParserObjects-ParserMethods`1-And-ParserObjects-IParser{`0}[]- 'ParserObjects.ParserMethods`1.And(ParserObjects.IParser{`0}[])')
  - [Any()](#M-ParserObjects-ParserMethods`1-Any 'ParserObjects.ParserMethods`1.Any')
  - [Combine(parsers)](#M-ParserObjects-ParserMethods`1-Combine-ParserObjects-IParser{`0}[]- 'ParserObjects.ParserMethods`1.Combine(ParserObjects.IParser{`0}[])')
  - [Deferred\`\`1(getParser)](#M-ParserObjects-ParserMethods`1-Deferred``1-System-Func{ParserObjects-IParser{`0,``0}}- 'ParserObjects.ParserMethods`1.Deferred``1(System.Func{ParserObjects.IParser{`0,``0}})')
  - [Empty()](#M-ParserObjects-ParserMethods`1-Empty 'ParserObjects.ParserMethods`1.Empty')
  - [End()](#M-ParserObjects-ParserMethods`1-End 'ParserObjects.ParserMethods`1.End')
  - [Examine\`\`1(parser,before,after)](#M-ParserObjects-ParserMethods`1-Examine``1-ParserObjects-IParser{`0,``0},System-Action{ParserObjects-Parsers-Examine{`0,``0}-Context},System-Action{ParserObjects-Parsers-Examine{`0,``0}-Context}- 'ParserObjects.ParserMethods`1.Examine``1(ParserObjects.IParser{`0,``0},System.Action{ParserObjects.Parsers.Examine{`0,``0}.Context},System.Action{ParserObjects.Parsers.Examine{`0,``0}.Context})')
  - [Fail\`\`1()](#M-ParserObjects-ParserMethods`1-Fail``1-System-String- 'ParserObjects.ParserMethods`1.Fail``1(System.String)')
  - [First\`\`1(parsers)](#M-ParserObjects-ParserMethods`1-First``1-ParserObjects-IParser{`0,``0}[]- 'ParserObjects.ParserMethods`1.First``1(ParserObjects.IParser{`0,``0}[])')
  - [Function\`\`1(func)](#M-ParserObjects-ParserMethods`1-Function``1-ParserObjects-ParserFunction{`0,``0}- 'ParserObjects.ParserMethods`1.Function``1(ParserObjects.ParserFunction{`0,``0})')
  - [If\`\`1(predicate,onSuccess)](#M-ParserObjects-ParserMethods`1-If``1-ParserObjects-IParser{`0},ParserObjects-IParser{`0,``0}- 'ParserObjects.ParserMethods`1.If``1(ParserObjects.IParser{`0},ParserObjects.IParser{`0,``0})')
  - [LeftApply\`\`1(left,getRight,arity)](#M-ParserObjects-ParserMethods`1-LeftApply``1-ParserObjects-IParser{`0,``0},System-Func{ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0,``0}},ParserObjects-Parsers-ApplyArity- 'ParserObjects.ParserMethods`1.LeftApply``1(ParserObjects.IParser{`0,``0},System.Func{ParserObjects.IParser{`0,``0},ParserObjects.IParser{`0,``0}},ParserObjects.Parsers.ApplyArity)')
  - [List\`\`1(p,atLeastOne)](#M-ParserObjects-ParserMethods`1-List``1-ParserObjects-IParser{`0,``0},System-Boolean- 'ParserObjects.ParserMethods`1.List``1(ParserObjects.IParser{`0,``0},System.Boolean)')
  - [List\`\`1(p,minimum,maximum)](#M-ParserObjects-ParserMethods`1-List``1-ParserObjects-IParser{`0,``0},System-Int32,System-Nullable{System-Int32}- 'ParserObjects.ParserMethods`1.List``1(ParserObjects.IParser{`0,``0},System.Int32,System.Nullable{System.Int32})')
  - [Match(predicate)](#M-ParserObjects-ParserMethods`1-Match-System-Func{`0,System-Boolean}- 'ParserObjects.ParserMethods`1.Match(System.Func{`0,System.Boolean})')
  - [Match(pattern)](#M-ParserObjects-ParserMethods`1-Match-`0- 'ParserObjects.ParserMethods`1.Match(`0)')
  - [Match(pattern)](#M-ParserObjects-ParserMethods`1-Match-System-Collections-Generic-IEnumerable{`0}- 'ParserObjects.ParserMethods`1.Match(System.Collections.Generic.IEnumerable{`0})')
  - [MatchAny(patterns)](#M-ParserObjects-ParserMethods`1-MatchAny-System-Collections-Generic-IEnumerable{System-String}- 'ParserObjects.ParserMethods`1.MatchAny(System.Collections.Generic.IEnumerable{System.String})')
  - [NegativeLookahead(p)](#M-ParserObjects-ParserMethods`1-NegativeLookahead-ParserObjects-IParser{`0}- 'ParserObjects.ParserMethods`1.NegativeLookahead(ParserObjects.IParser{`0})')
  - [Not(p1)](#M-ParserObjects-ParserMethods`1-Not-ParserObjects-IParser{`0}- 'ParserObjects.ParserMethods`1.Not(ParserObjects.IParser{`0})')
  - [Optional\`\`1(p,getDefault)](#M-ParserObjects-ParserMethods`1-Optional``1-ParserObjects-IParser{`0,``0},System-Func{``0}- 'ParserObjects.ParserMethods`1.Optional``1(ParserObjects.IParser{`0,``0},System.Func{``0})')
  - [Optional\`\`1(p,getDefault)](#M-ParserObjects-ParserMethods`1-Optional``1-ParserObjects-IParser{`0,``0},System-Func{ParserObjects-ISequence{`0},``0}- 'ParserObjects.ParserMethods`1.Optional``1(ParserObjects.IParser{`0,``0},System.Func{ParserObjects.ISequence{`0},``0})')
  - [Or(parsers)](#M-ParserObjects-ParserMethods`1-Or-ParserObjects-IParser{`0}[]- 'ParserObjects.ParserMethods`1.Or(ParserObjects.IParser{`0}[])')
  - [PositiveLookahead(p)](#M-ParserObjects-ParserMethods`1-PositiveLookahead-ParserObjects-IParser{`0}- 'ParserObjects.ParserMethods`1.PositiveLookahead(ParserObjects.IParser{`0})')
  - [Produce\`\`1(produce)](#M-ParserObjects-ParserMethods`1-Produce``1-System-Func{``0}- 'ParserObjects.ParserMethods`1.Produce``1(System.Func{``0})')
  - [Produce\`\`1(produce)](#M-ParserObjects-ParserMethods`1-Produce``1-System-Func{ParserObjects-ISequence{`0},``0}- 'ParserObjects.ParserMethods`1.Produce``1(System.Func{ParserObjects.ISequence{`0},``0})')
  - [Replaceable\`\`1(defaultParser)](#M-ParserObjects-ParserMethods`1-Replaceable``1-ParserObjects-IParser{`0,``0}- 'ParserObjects.ParserMethods`1.Replaceable``1(ParserObjects.IParser{`0,``0})')
  - [RightApply\`\`2(item,middle,produce,getMissingRight)](#M-ParserObjects-ParserMethods`1-RightApply``2-ParserObjects-IParser{`0,``1},ParserObjects-IParser{`0,``0},System-Func{``1,``0,``1,``1},System-Func{ParserObjects-ISequence{`0},``1}- 'ParserObjects.ParserMethods`1.RightApply``2(ParserObjects.IParser{`0,``1},ParserObjects.IParser{`0,``0},System.Func{``1,``0,``1,``1},System.Func{ParserObjects.ISequence{`0},``1})')
  - [Rule\`\`10(p1,p2,p3,p4,p5,p6,p7,p8,p9,produce)](#M-ParserObjects-ParserMethods`1-Rule``10-ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0,``1},ParserObjects-IParser{`0,``2},ParserObjects-IParser{`0,``3},ParserObjects-IParser{`0,``4},ParserObjects-IParser{`0,``5},ParserObjects-IParser{`0,``6},ParserObjects-IParser{`0,``7},ParserObjects-IParser{`0,``8},System-Func{``0,``1,``2,``3,``4,``5,``6,``7,``8,``9}- 'ParserObjects.ParserMethods`1.Rule``10(ParserObjects.IParser{`0,``0},ParserObjects.IParser{`0,``1},ParserObjects.IParser{`0,``2},ParserObjects.IParser{`0,``3},ParserObjects.IParser{`0,``4},ParserObjects.IParser{`0,``5},ParserObjects.IParser{`0,``6},ParserObjects.IParser{`0,``7},ParserObjects.IParser{`0,``8},System.Func{``0,``1,``2,``3,``4,``5,``6,``7,``8,``9})')
  - [Rule\`\`3(p1,p2,produce)](#M-ParserObjects-ParserMethods`1-Rule``3-ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0,``1},System-Func{``0,``1,``2}- 'ParserObjects.ParserMethods`1.Rule``3(ParserObjects.IParser{`0,``0},ParserObjects.IParser{`0,``1},System.Func{``0,``1,``2})')
  - [Rule\`\`4(p1,p2,p3,produce)](#M-ParserObjects-ParserMethods`1-Rule``4-ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0,``1},ParserObjects-IParser{`0,``2},System-Func{``0,``1,``2,``3}- 'ParserObjects.ParserMethods`1.Rule``4(ParserObjects.IParser{`0,``0},ParserObjects.IParser{`0,``1},ParserObjects.IParser{`0,``2},System.Func{``0,``1,``2,``3})')
  - [Rule\`\`5(p1,p2,p3,p4,produce)](#M-ParserObjects-ParserMethods`1-Rule``5-ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0,``1},ParserObjects-IParser{`0,``2},ParserObjects-IParser{`0,``3},System-Func{``0,``1,``2,``3,``4}- 'ParserObjects.ParserMethods`1.Rule``5(ParserObjects.IParser{`0,``0},ParserObjects.IParser{`0,``1},ParserObjects.IParser{`0,``2},ParserObjects.IParser{`0,``3},System.Func{``0,``1,``2,``3,``4})')
  - [Rule\`\`6(p1,p2,p3,p4,p5,produce)](#M-ParserObjects-ParserMethods`1-Rule``6-ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0,``1},ParserObjects-IParser{`0,``2},ParserObjects-IParser{`0,``3},ParserObjects-IParser{`0,``4},System-Func{``0,``1,``2,``3,``4,``5}- 'ParserObjects.ParserMethods`1.Rule``6(ParserObjects.IParser{`0,``0},ParserObjects.IParser{`0,``1},ParserObjects.IParser{`0,``2},ParserObjects.IParser{`0,``3},ParserObjects.IParser{`0,``4},System.Func{``0,``1,``2,``3,``4,``5})')
  - [Rule\`\`7(p1,p2,p3,p4,p5,p6,produce)](#M-ParserObjects-ParserMethods`1-Rule``7-ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0,``1},ParserObjects-IParser{`0,``2},ParserObjects-IParser{`0,``3},ParserObjects-IParser{`0,``4},ParserObjects-IParser{`0,``5},System-Func{``0,``1,``2,``3,``4,``5,``6}- 'ParserObjects.ParserMethods`1.Rule``7(ParserObjects.IParser{`0,``0},ParserObjects.IParser{`0,``1},ParserObjects.IParser{`0,``2},ParserObjects.IParser{`0,``3},ParserObjects.IParser{`0,``4},ParserObjects.IParser{`0,``5},System.Func{``0,``1,``2,``3,``4,``5,``6})')
  - [Rule\`\`8(p1,p2,p3,p4,p5,p6,p7,produce)](#M-ParserObjects-ParserMethods`1-Rule``8-ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0,``1},ParserObjects-IParser{`0,``2},ParserObjects-IParser{`0,``3},ParserObjects-IParser{`0,``4},ParserObjects-IParser{`0,``5},ParserObjects-IParser{`0,``6},System-Func{``0,``1,``2,``3,``4,``5,``6,``7}- 'ParserObjects.ParserMethods`1.Rule``8(ParserObjects.IParser{`0,``0},ParserObjects.IParser{`0,``1},ParserObjects.IParser{`0,``2},ParserObjects.IParser{`0,``3},ParserObjects.IParser{`0,``4},ParserObjects.IParser{`0,``5},ParserObjects.IParser{`0,``6},System.Func{``0,``1,``2,``3,``4,``5,``6,``7})')
  - [Rule\`\`9(p1,p2,p3,p4,p5,p6,p7,p8,produce)](#M-ParserObjects-ParserMethods`1-Rule``9-ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0,``1},ParserObjects-IParser{`0,``2},ParserObjects-IParser{`0,``3},ParserObjects-IParser{`0,``4},ParserObjects-IParser{`0,``5},ParserObjects-IParser{`0,``6},ParserObjects-IParser{`0,``7},System-Func{``0,``1,``2,``3,``4,``5,``6,``7,``8}- 'ParserObjects.ParserMethods`1.Rule``9(ParserObjects.IParser{`0,``0},ParserObjects.IParser{`0,``1},ParserObjects.IParser{`0,``2},ParserObjects.IParser{`0,``3},ParserObjects.IParser{`0,``4},ParserObjects.IParser{`0,``5},ParserObjects.IParser{`0,``6},ParserObjects.IParser{`0,``7},System.Func{``0,``1,``2,``3,``4,``5,``6,``7,``8})')
  - [SeparatedList\`\`1(p,separator,atLeastOne)](#M-ParserObjects-ParserMethods`1-SeparatedList``1-ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0},System-Boolean- 'ParserObjects.ParserMethods`1.SeparatedList``1(ParserObjects.IParser{`0,``0},ParserObjects.IParser{`0},System.Boolean)')
  - [SeparatedList\`\`1(p,separator,minimum,maximum)](#M-ParserObjects-ParserMethods`1-SeparatedList``1-ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0},System-Int32,System-Nullable{System-Int32}- 'ParserObjects.ParserMethods`1.SeparatedList``1(ParserObjects.IParser{`0,``0},ParserObjects.IParser{`0},System.Int32,System.Nullable{System.Int32})')
  - [Transform\`\`2(parser,transform)](#M-ParserObjects-ParserMethods`1-Transform``2-ParserObjects-IParser{`0,``0},System-Func{``0,``1}- 'ParserObjects.ParserMethods`1.Transform``2(ParserObjects.IParser{`0,``0},System.Func{``0,``1})')
  - [Trie\`\`1(readOnlyTrie)](#M-ParserObjects-ParserMethods`1-Trie``1-ParserObjects-IReadOnlyTrie{`0,``0}- 'ParserObjects.ParserMethods`1.Trie``1(ParserObjects.IReadOnlyTrie{`0,``0})')
  - [Trie\`\`1(setupTrie)](#M-ParserObjects-ParserMethods`1-Trie``1-System-Action{ParserObjects-IInsertableTrie{`0,``0}}- 'ParserObjects.ParserMethods`1.Trie``1(System.Action{ParserObjects.IInsertableTrie{`0,``0}})')
- [PositiveLookaheadParser\`1](#T-ParserObjects-Parsers-PositiveLookaheadParser`1 'ParserObjects.Parsers.PositiveLookaheadParser`1')
- [ProduceParser\`2](#T-ParserObjects-Parsers-ProduceParser`2 'ParserObjects.Parsers.ProduceParser`2')
- [ReplaceableParser\`2](#T-ParserObjects-Parsers-ReplaceableParser`2 'ParserObjects.Parsers.ReplaceableParser`2')
- [RuleParser\`2](#T-ParserObjects-Parsers-RuleParser`2 'ParserObjects.Parsers.RuleParser`2')
- [SequenceEnumerable\`1](#T-ParserObjects-Sequences-SequenceEnumerable`1 'ParserObjects.Sequences.SequenceEnumerable`1')
- [SequenceExtensions](#T-ParserObjects-SequenceExtensions 'ParserObjects.SequenceExtensions')
  - [AsEnumerable\`\`1(input)](#M-ParserObjects-SequenceExtensions-AsEnumerable``1-ParserObjects-ISequence{``0}- 'ParserObjects.SequenceExtensions.AsEnumerable``1(ParserObjects.ISequence{``0})')
  - [Parse\`\`2(input,parse)](#M-ParserObjects-SequenceExtensions-Parse``2-ParserObjects-ISequence{``0},System-Func{ParserObjects-ISequence{``0},ParserObjects-IResult{``1}}- 'ParserObjects.SequenceExtensions.Parse``2(ParserObjects.ISequence{``0},System.Func{ParserObjects.ISequence{``0},ParserObjects.IResult{``1}})')
  - [Select\`\`2(input,map)](#M-ParserObjects-SequenceExtensions-Select``2-ParserObjects-ISequence{``0},System-Func{``0,``1}- 'ParserObjects.SequenceExtensions.Select``2(ParserObjects.ISequence{``0},System.Func{``0,``1})')
  - [Where\`\`1(input,predicate)](#M-ParserObjects-SequenceExtensions-Where``1-ParserObjects-ISequence{``0},System-Func{``0,System-Boolean}- 'ParserObjects.SequenceExtensions.Where``1(ParserObjects.ISequence{``0},System.Func{``0,System.Boolean})')
- [SqlStyleParserMethods](#T-ParserObjects-SqlStyleParserMethods 'ParserObjects.SqlStyleParserMethods')
  - [Comment()](#M-ParserObjects-SqlStyleParserMethods-Comment 'ParserObjects.SqlStyleParserMethods.Comment')
- [StreamCharacterSequence](#T-ParserObjects-Sequences-StreamCharacterSequence 'ParserObjects.Sequences.StreamCharacterSequence')
- [StringCharacterSequence](#T-ParserObjects-Sequences-StringCharacterSequence 'ParserObjects.Sequences.StringCharacterSequence')
- [StringExtensions](#T-ParserObjects-Sequences-StringExtensions 'ParserObjects.Sequences.StringExtensions')
  - [ToCharacterSequence(str)](#M-ParserObjects-Sequences-StringExtensions-ToCharacterSequence-System-String- 'ParserObjects.Sequences.StringExtensions.ToCharacterSequence(System.String)')
- [TransformParser\`3](#T-ParserObjects-Parsers-TransformParser`3 'ParserObjects.Parsers.TransformParser`3')
- [TrieExtensions](#T-ParserObjects-TrieExtensions 'ParserObjects.TrieExtensions')
  - [Add(readOnlyTrie,value)](#M-ParserObjects-TrieExtensions-Add-ParserObjects-IInsertableTrie{System-Char,System-String},System-String- 'ParserObjects.TrieExtensions.Add(ParserObjects.IInsertableTrie{System.Char,System.String},System.String)')
  - [AddMany(readOnlyTrie,values)](#M-ParserObjects-TrieExtensions-AddMany-ParserObjects-IInsertableTrie{System-Char,System-String},System-String[]- 'ParserObjects.TrieExtensions.AddMany(ParserObjects.IInsertableTrie{System.Char,System.String},System.String[])')
  - [ToParser\`\`2(readOnlyTrie)](#M-ParserObjects-TrieExtensions-ToParser``2-ParserObjects-IReadOnlyTrie{``0,``1}- 'ParserObjects.TrieExtensions.ToParser``2(ParserObjects.IReadOnlyTrie{``0,``1})')
- [TrieParser\`2](#T-ParserObjects-Parsers-TrieParser`2 'ParserObjects.Parsers.TrieParser`2')

<a name='T-ParserObjects-Utility-AlwaysFullRingBuffer`1'></a>
## AlwaysFullRingBuffer\`1 `type`

##### Namespace

ParserObjects.Utility

##### Summary

Simplified ring-buffer implementation which makes several assumptions for simplicity

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T |  |

<a name='T-ParserObjects-Parsers-AndParser`1'></a>
## AndParser\`1 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

Tests several parsers sequentially. If all of them succeed return Success. If any Fail, return
Failure. Consumes input but returns no explicit output.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |

<a name='T-ParserObjects-Parsers-AnyParser`1'></a>
## AnyParser\`1 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

Matches any input item that isn't the end of input. Consumes exactly one input item.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T |  |

<a name='T-ParserObjects-Visitors-BnfStringifyVisitor'></a>
## BnfStringifyVisitor `type`

##### Namespace

ParserObjects.Visitors

##### Summary

Parser-visitor to traverse the parser-graph and attempt to produce a string of approximate
pseudo-BNF to describe the grammar. Proper execution of this visitor depends on parsers having
the .Name value set. If you have custom parser types you can create a subclass of this visitor
type with signature 'public VisitChild(MyParserType parser, State state)' and it should dispatch to
them as required.

<a name='T-ParserObjects-CPlusPlusStyleParserMethods'></a>
## CPlusPlusStyleParserMethods `type`

##### Namespace

ParserObjects

<a name='M-ParserObjects-CPlusPlusStyleParserMethods-Comment'></a>
### Comment() `method`

##### Summary

C++-style comment '//' ...

##### Returns



##### Parameters

This method has no parameters.

<a name='T-ParserObjects-CStyleParserMethods'></a>
## CStyleParserMethods `type`

##### Namespace

ParserObjects

<a name='M-ParserObjects-CStyleParserMethods-Comment'></a>
### Comment() `method`

##### Summary

C-style comment with '/*' ... '*/' delimiters

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-CStyleParserMethods-Double'></a>
### Double() `method`

##### Summary

C-style float/double literal returned as a parsed Double

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-CStyleParserMethods-DoubleString'></a>
### DoubleString() `method`

##### Summary

C-style Double literal returned as a string

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-CStyleParserMethods-HexadecimalInteger'></a>
### HexadecimalInteger() `method`

##### Summary

C-style hexadecimal literal returned as a parsed integer

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-CStyleParserMethods-HexadecimalString'></a>
### HexadecimalString() `method`

##### Summary

C-style hexadecimal literal returned as a string

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-CStyleParserMethods-Identifier'></a>
### Identifier() `method`

##### Summary

C-style Identifier

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-CStyleParserMethods-Integer'></a>
### Integer() `method`

##### Summary

C-style Integer literal returned as a parsed Int32

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-CStyleParserMethods-IntegerString'></a>
### IntegerString() `method`

##### Summary

C-style integer literal returned as a string

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-CStyleParserMethods-UnsignedInteger'></a>
### UnsignedInteger() `method`

##### Summary

C-style Unsigned Integer literal returned as a parsed Int32

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-CStyleParserMethods-UnsignedIntegerString'></a>
### UnsignedIntegerString() `method`

##### Summary

C-style unsigned integer literal returned as a string

##### Returns



##### Parameters

This method has no parameters.

<a name='T-ParserObjects-Parsers-DeferredParser`2'></a>
## DeferredParser\`2 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

Looks up a parser at Parse() time, to avoid circular references in the grammar

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |
| TInput |  |

<a name='T-ParserObjects-Parsers-EmptyParser`1'></a>
## EmptyParser\`1 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

The empty parser, consumes no input and always returns success

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |

<a name='T-ParserObjects-Parsers-EndParser`1'></a>
## EndParser\`1 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

Matches at the end of the input sequence. Fails if the input sequence is at any point besides the
end.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |

<a name='T-ParserObjects-Sequences-EnumerableExtensions'></a>
## EnumerableExtensions `type`

##### Namespace

ParserObjects.Sequences

<a name='M-ParserObjects-Sequences-EnumerableExtensions-ToSequence``1-System-Collections-Generic-IEnumerable{``0},``0-'></a>
### ToSequence\`\`1(enumerable,endValue) `method`

##### Summary

Wrap the enumerable as a sequence.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| enumerable | [System.Collections.Generic.IEnumerable{\`\`0}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IEnumerable 'System.Collections.Generic.IEnumerable{``0}') |  |
| endValue | [\`\`0](#T-``0 '``0') | An end value to return when the sequence is exhausted |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T |  |

<a name='M-ParserObjects-Sequences-EnumerableExtensions-ToSequence``1-System-Collections-Generic-IEnumerable{``0},System-Func{``0}-'></a>
### ToSequence\`\`1(enumerable,getEndValue) `method`

##### Summary

Wrap the enumerable as a sequence

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| enumerable | [System.Collections.Generic.IEnumerable{\`\`0}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IEnumerable 'System.Collections.Generic.IEnumerable{``0}') |  |
| getEndValue | [System.Func{\`\`0}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{``0}') | Get a value to return when the sequence is exhausted |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T |  |

<a name='T-ParserObjects-Sequences-EnumerableSequence`1'></a>
## EnumerableSequence\`1 `type`

##### Namespace

ParserObjects.Sequences

##### Summary

Wraps an IEnumerable as an ISequence. Makes the items from the enumerable usable in parse operations

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T |  |

<a name='T-ParserObjects-Parsers-FailParser`2'></a>
## FailParser\`2 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

Returns unconditional failure

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='T-ParserObjects-Sequences-FilterSequence`1'></a>
## FilterSequence\`1 `type`

##### Namespace

ParserObjects.Sequences

##### Summary

Filter a sequence to only return items which match a predicate

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T |  |

<a name='T-ParserObjects-Visitors-FindParserVisitor'></a>
## FindParserVisitor `type`

##### Namespace

ParserObjects.Visitors

##### Summary

Parser-visitor type to traverse the parser tree and find matching parser nodes.

<a name='M-ParserObjects-Visitors-FindParserVisitor-Named-System-String,ParserObjects-IParser-'></a>
### Named(name,root) `method`

##### Summary

Search for a parser with the given Name. Returns only the first result in case of duplicates

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| name | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |
| root | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |

<a name='M-ParserObjects-Visitors-FindParserVisitor-OfType``1-ParserObjects-IParser-'></a>
### OfType\`\`1(root) `method`

##### Summary

Search for all parsers of the given type. Returns all results.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| root | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TParser |  |

<a name='M-ParserObjects-Visitors-FindParserVisitor-Replace-ParserObjects-IParser,System-Func{ParserObjects-IReplaceableParserUntyped,System-Boolean},ParserObjects-IParser-'></a>
### Replace(root,predicate,replacement) `method`

##### Summary

Search for ReplaceableParsers matching a predicate and attempt to replace their contents with the
replacement parser if it is found. The replacement parser must be non-null and of the correct
type. Replaces all matching instances.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| root | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |
| predicate | [System.Func{ParserObjects.IReplaceableParserUntyped,System.Boolean}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{ParserObjects.IReplaceableParserUntyped,System.Boolean}') |  |
| replacement | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |

<a name='M-ParserObjects-Visitors-FindParserVisitor-Replace-ParserObjects-IParser,System-String,ParserObjects-IParser-'></a>
### Replace(root,name,replacement) `method`

##### Summary

Search for ReplaceableParsers with the given name and attempt to replace their contents with the
replacement parser. The replacement parser must be non-null and of the correct type. Replaces
all matching instances.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| root | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |
| name | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |
| replacement | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |

<a name='M-ParserObjects-Visitors-FindParserVisitor-Replace``2-ParserObjects-IParser,System-Func{ParserObjects-IReplaceableParserUntyped,System-Boolean},System-Func{ParserObjects-IParser{``0,``1},ParserObjects-IParser{``0,``1}}-'></a>
### Replace\`\`2(root,predicate,transform) `method`

##### Summary

Search for ReplaceableParsers matching a predicate and attempt to transform the contents using
the given transformation. The contents of the ReplaceableParser will be replaced with the
transformed result if it is new and valid.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| root | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |
| predicate | [System.Func{ParserObjects.IReplaceableParserUntyped,System.Boolean}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{ParserObjects.IReplaceableParserUntyped,System.Boolean}') |  |
| transform | [System.Func{ParserObjects.IParser{\`\`0,\`\`1},ParserObjects.IParser{\`\`0,\`\`1}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{ParserObjects.IParser{``0,``1},ParserObjects.IParser{``0,``1}}') |  |

<a name='M-ParserObjects-Visitors-FindParserVisitor-Replace``2-ParserObjects-IParser,System-String,System-Func{ParserObjects-IParser{``0,``1},ParserObjects-IParser{``0,``1}}-'></a>
### Replace\`\`2(root,name,transform) `method`

##### Summary

Search for ReplaceableParsers with the given name and attempt to transform the contents using
the given transformation. The contents of the ReplaceableParser will be replaced with the
transformed result if it is new and valid.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| root | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |
| name | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |
| transform | [System.Func{ParserObjects.IParser{\`\`0,\`\`1},ParserObjects.IParser{\`\`0,\`\`1}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{ParserObjects.IParser{``0,``1},ParserObjects.IParser{``0,``1}}') |  |

<a name='T-ParserObjects-Parsers-FirstParser`2'></a>
## FirstParser\`2 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

Takes a list of parsers and attempts each one in order. Returns as soon as the first parser
succeeds

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |
| TInput |  |

<a name='T-ParserObjects-Parsers-FuncParser`2'></a>
## FuncParser\`2 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

Invokes a delegate to perform the parse

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='T-ParserObjects-IInsertableTrie`2'></a>
## IInsertableTrie\`2 `type`

##### Namespace

ParserObjects

<a name='M-ParserObjects-IInsertableTrie`2-Add-System-Collections-Generic-IEnumerable{`0},`1-'></a>
### Add(keys,result) `method`

##### Summary

Given a composite key and a value, insert the value at the location described by the key

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| keys | [System.Collections.Generic.IEnumerable{\`0}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IEnumerable 'System.Collections.Generic.IEnumerable{`0}') |  |
| result | [\`1](#T-`1 '`1') |  |

<a name='T-ParserObjects-IParser'></a>
## IParser `type`

##### Namespace

ParserObjects

##### Summary

Parser base type.

<a name='P-ParserObjects-IParser-Name'></a>
### Name `property`

##### Summary

The name of the parser. This value is only used for bookkeeping information and does not have
an affect on the parse.

<a name='M-ParserObjects-IParser-GetChildren'></a>
### GetChildren() `method`

##### Summary

Get a list of child parsers, if any.

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-IParser-ReplaceChild-ParserObjects-IParser,ParserObjects-IParser-'></a>
### ReplaceChild(find,replace) `method`

##### Summary

Return a parser exactly like this parser but with one of it's children replaced. If no
replacement is made, this parser may be returned.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| find | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |
| replace | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |

<a name='T-ParserObjects-IParser`1'></a>
## IParser\`1 `type`

##### Namespace

ParserObjects

##### Summary

Parser object which allows getting the result without type information

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |

<a name='M-ParserObjects-IParser`1-Parse-ParserObjects-ParseState{`0}-'></a>
### Parse(state) `method`

##### Summary

Attempt to parse the input sequence and produce an output result of type object. If the parse
fails, it is expected that this method will return the input sequence to the state it was at
before the parse was attempted.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| state | [ParserObjects.ParseState{\`0}](#T-ParserObjects-ParseState{`0} 'ParserObjects.ParseState{`0}') |  |

<a name='T-ParserObjects-IParser`2'></a>
## IParser\`2 `type`

##### Namespace

ParserObjects

##### Summary

Parser with explicit input and output types.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='M-ParserObjects-IParser`2-Parse-ParserObjects-ParseState{`0}-'></a>
### Parse(state) `method`

##### Summary

Attempt to parse the input sequence and produce an output result. If the parse fails, it is
expected that this method will return the input sequence to the state it was at before the
parse was attempted.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| state | [ParserObjects.ParseState{\`0}](#T-ParserObjects-ParseState{`0} 'ParserObjects.ParseState{`0}') |  |

<a name='T-ParserObjects-IReadOnlyTrie`2'></a>
## IReadOnlyTrie\`2 `type`

##### Namespace

ParserObjects

##### Summary

A trie type which allows using a composite key to search for values

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TKey |  |
| TResult |  |

<a name='M-ParserObjects-IReadOnlyTrie`2-Get-System-Collections-Generic-IEnumerable{`0}-'></a>
### Get(keys) `method`

##### Summary

Given a composite key, search for a value at that location in the trie

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| keys | [System.Collections.Generic.IEnumerable{\`0}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IEnumerable 'System.Collections.Generic.IEnumerable{`0}') |  |

<a name='M-ParserObjects-IReadOnlyTrie`2-Get-ParserObjects-ISequence{`0}-'></a>
### Get(keys) `method`

##### Summary

Given a sequence, treat the items in that sequence as elements of a composite key. Return a
value from the trie which successfully consumes the most amount of input items.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| keys | [ParserObjects.ISequence{\`0}](#T-ParserObjects-ISequence{`0} 'ParserObjects.ISequence{`0}') |  |

<a name='M-ParserObjects-IReadOnlyTrie`2-GetAllPatterns'></a>
### GetAllPatterns() `method`

##### Summary

Get all the pattern sequences in the trie. This operation may iterate over the entire trie so
the results should be cached if possible.

##### Returns



##### Parameters

This method has no parameters.

<a name='T-ParserObjects-IReplaceableParserUntyped'></a>
## IReplaceableParserUntyped `type`

##### Namespace

ParserObjects

##### Summary

A parser which has an in-place replaceable child. Used to identify parsers which can participate in
certain find/replace operations

<a name='P-ParserObjects-IReplaceableParserUntyped-ReplaceableChild'></a>
### ReplaceableChild `property`

##### Summary

The child parser which can be replaced without cloning

<a name='M-ParserObjects-IReplaceableParserUntyped-SetParser-ParserObjects-IParser-'></a>
### SetParser(parser) `method`

##### Summary

Set the new child parser without cloning

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| parser | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |

<a name='T-ParserObjects-IResult'></a>
## IResult `type`

##### Namespace

ParserObjects

<a name='P-ParserObjects-IResult-Location'></a>
### Location `property`

##### Summary

The approximate location of the successful parse in the input sequence. On failure, this
value is undefined and may show the location of the start of the attempt, the location at
which failure occured, null, or some other value.

<a name='P-ParserObjects-IResult-Success'></a>
### Success `property`

##### Summary

Returns true if the parse succeeded, false otherwise.

<a name='T-ParserObjects-IResult`1'></a>
## IResult\`1 `type`

##### Namespace

ParserObjects

##### Summary

Result object from a Parse operation

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TValue |  |

<a name='P-ParserObjects-IResult`1-Value'></a>
### Value `property`

##### Summary

The produced value from the successful parse. If Success is false, this value is undefined.

<a name='M-ParserObjects-IResult`1-Transform``1-System-Func{`0,``0}-'></a>
### Transform\`\`1(transform) `method`

##### Summary

Transforms the Value of the result to a new form

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| transform | [System.Func{\`0,\`\`0}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{`0,``0}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |

<a name='T-ParserObjects-ISequence'></a>
## ISequence `type`

##### Namespace

ParserObjects

<a name='P-ParserObjects-ISequence-CurrentLocation'></a>
### CurrentLocation `property`

##### Summary

The approximate location from the source data where the current input item was located, if
available.

<a name='P-ParserObjects-ISequence-IsAtEnd'></a>
### IsAtEnd `property`

##### Summary

True if the sequence is at the end and no more values may be retrieved. False if the sequence
is exhausted and no more values are available.

<a name='T-ParserObjects-ISequence`1'></a>
## ISequence\`1 `type`

##### Namespace

ParserObjects

##### Summary

An input sequence of items. Similar to IEnumerable/IEnumerator but with the ability to rewind and
put back items which are not needed.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T |  |

<a name='M-ParserObjects-ISequence`1-GetNext'></a>
### GetNext() `method`

##### Summary

Get the next value from the sequence or a default value if the sequence is at the end, and
increments the location

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-ISequence`1-Peek'></a>
### Peek() `method`

##### Summary

Gets the next value off the sequence but does not advance the location

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-ISequence`1-PutBack-`0-'></a>
### PutBack(value) `method`

##### Summary

Put back the given value to the head of the sequence. This value does not need to be a value
which previously has been taken off the sequence.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| value | [\`0](#T-`0 '`0') |  |

<a name='T-ParserObjects-Parsers-IfParser`2'></a>
## IfParser\`2 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

Attempts to match a predicate condition and, on success, invokes a parser.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='T-ParserObjects-Utility-InsertOnlyTrie`2'></a>
## InsertOnlyTrie\`2 `type`

##### Namespace

ParserObjects.Utility

##### Summary

Trie implementation which allows inserts of values but not updates of values. Once a value is
inserted into the trie, it cannot be removed or modified

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TKey |  |
| TResult |  |

<a name='T-ParserObjects-JavaScriptStyleParserMethods'></a>
## JavaScriptStyleParserMethods `type`

##### Namespace

ParserObjects

<a name='M-ParserObjects-JavaScriptStyleParserMethods-Number'></a>
### Number() `method`

##### Summary

JavaScript-style number literal returned as a parsed Double

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-JavaScriptStyleParserMethods-NumberString'></a>
### NumberString() `method`

##### Summary

JavaScript-style number literal, returned as a string

##### Returns



##### Parameters

This method has no parameters.

<a name='T-ParserObjects-Parsers-LeftValueParser`2'></a>
## LeftValueParser\`2 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

A parser for holding a parsed result from left application. Do not use this type
directly.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='T-ParserObjects-Location'></a>
## Location `type`

##### Namespace

ParserObjects

##### Summary

An approximate description of the location in the data source where an item is located. Notice
that some types of input may not make this information precisely knowable.

<a name='P-ParserObjects-Location-Column'></a>
### Column `property`

##### Summary

The index of the item in the current file and line

<a name='P-ParserObjects-Location-FileName'></a>
### FileName `property`

##### Summary

The name of the file, if the input source is a file

<a name='P-ParserObjects-Location-Line'></a>
### Line `property`

##### Summary

The line number, if the input is on multiple lines

<a name='T-ParserObjects-Sequences-MapSequence`2'></a>
## MapSequence\`2 `type`

##### Namespace

ParserObjects.Sequences

##### Summary

A sequence decorator which takes items from the input sequence and transforms them. Notice
that when using MapSequence, you should not directly access the underlying sequence
anymore. Data may be lost, because items put back to the MapSequence cannot be un-mapped
and put back to the underlying sequence.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='T-ParserObjects-Parsers-MatchPatternParser`1'></a>
## MatchPatternParser\`1 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

Given a literal sequence of values, pull values off the input sequence to match. If the entire
series matches, return it

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T |  |

<a name='T-ParserObjects-Parsers-MatchPredicateParser`1'></a>
## MatchPredicateParser\`1 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

Returns the next input item if it satisfies a predicate, failure otherwise.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T |  |

<a name='T-ParserObjects-Parsers-NegativeLookaheadParser`1'></a>
## NegativeLookaheadParser\`1 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

Negative lookahead parser. Tests the input to see if the inner parser matches. Return 
success if the parser does not match, fail otherwise. Consumes no input.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |

<a name='T-ParserObjects-Parsers-NotParser`1'></a>
## NotParser\`1 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

Invokes a parser and inverses the result success status. If the parser succeeds, return 
Failure. Otherwise returns Success. Consumes no input in either case and returns no output.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |

<a name='T-ParserObjects-Parsers-OrParser`1'></a>
## OrParser\`1 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

Tests several parsers sequentially, returning Success if any parser succeeds, Failure
otherwise. Consumes input but returns no explicit output.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |

<a name='T-ParserObjects-Sequences-ParseResultSequence`2'></a>
## ParseResultSequence\`2 `type`

##### Namespace

ParserObjects.Sequences

##### Summary

An adaptor to change output values from an IParser into an ISequence of results

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='T-ParserObjects-ParserCombinatorExtensions'></a>
## ParserCombinatorExtensions `type`

##### Namespace

ParserObjects

##### Summary

IParser extension methods for building combinators using fluent syntax

<a name='M-ParserObjects-ParserCombinatorExtensions-Examine``2-ParserObjects-IParser{``0,``1},System-Action{ParserObjects-Parsers-Examine{``0,``1}-Context},System-Action{ParserObjects-Parsers-Examine{``0,``1}-Context}-'></a>
### Examine\`\`2(parser,before,after) `method`

##### Summary

Invoke callbacks before and after a parse

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| parser | [ParserObjects.IParser{\`\`0,\`\`1}](#T-ParserObjects-IParser{``0,``1} 'ParserObjects.IParser{``0,``1}') |  |
| before | [System.Action{ParserObjects.Parsers.Examine{\`\`0,\`\`1}.Context}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{ParserObjects.Parsers.Examine{``0,``1}.Context}') |  |
| after | [System.Action{ParserObjects.Parsers.Examine{\`\`0,\`\`1}.Context}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{ParserObjects.Parsers.Examine{``0,``1}.Context}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='M-ParserObjects-ParserCombinatorExtensions-FollowedBy``2-ParserObjects-IParser{``0,``1},ParserObjects-IParser{``0}-'></a>
### FollowedBy\`\`2(p,lookahead) `method`

##### Summary

Zero-length assertion that the given parser's result is followed by another sequence.
The lookahead sequence is matched but not consumed

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{\`\`0,\`\`1}](#T-ParserObjects-IParser{``0,``1} 'ParserObjects.IParser{``0,``1}') |  |
| lookahead | [ParserObjects.IParser{\`\`0}](#T-ParserObjects-IParser{``0} 'ParserObjects.IParser{``0}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='M-ParserObjects-ParserCombinatorExtensions-ListCharToString-ParserObjects-IParser{System-Char,System-Char},System-Boolean-'></a>
### ListCharToString(p,atLeastOne) `method`

##### Summary

Given a parser which parses characters, parse a list of characters and return the sequence as a
string

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{System.Char,System.Char}](#T-ParserObjects-IParser{System-Char,System-Char} 'ParserObjects.IParser{System.Char,System.Char}') |  |
| atLeastOne | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') |  |

<a name='M-ParserObjects-ParserCombinatorExtensions-ListCharToString-ParserObjects-IParser{System-Char,System-Char},System-Int32,System-Nullable{System-Int32}-'></a>
### ListCharToString(p,minimum,maximum) `method`

##### Summary

Given a parser which parsers characters, parse a list of characters and return
the result as a string. Supports limits for minimum and maximum numbers of
characters to parse.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{System.Char,System.Char}](#T-ParserObjects-IParser{System-Char,System-Char} 'ParserObjects.IParser{System.Char,System.Char}') |  |
| minimum | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') |  |
| maximum | [System.Nullable{System.Int32}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.Int32}') |  |

<a name='M-ParserObjects-ParserCombinatorExtensions-ListSeparatedBy``3-ParserObjects-IParser{``0,``2},ParserObjects-IParser{``0,``1},System-Boolean-'></a>
### ListSeparatedBy\`\`3(p,separator,atLeastOne) `method`

##### Summary

Returns a list of results from the given parser separated by a separator pattern. Continues until
the item or separator parser return failure. Returns an enumerable of results.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{\`\`0,\`\`2}](#T-ParserObjects-IParser{``0,``2} 'ParserObjects.IParser{``0,``2}') |  |
| separator | [ParserObjects.IParser{\`\`0,\`\`1}](#T-ParserObjects-IParser{``0,``1} 'ParserObjects.IParser{``0,``1}') |  |
| atLeastOne | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TSeparator |  |
| TOutput |  |

<a name='M-ParserObjects-ParserCombinatorExtensions-ListSeparatedBy``3-ParserObjects-IParser{``0,``2},ParserObjects-IParser{``0,``1},System-Int32,System-Nullable{System-Int32}-'></a>
### ListSeparatedBy\`\`3(p,separator,minimum,maximum) `method`

##### Summary

Returns a list of results from the given parser separated by a separator
pattern. Continues until the item or separator pattern return failure, or
the minimum/maximum counts are not satisfied. Returns an enumeration of results

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{\`\`0,\`\`2}](#T-ParserObjects-IParser{``0,``2} 'ParserObjects.IParser{``0,``2}') |  |
| separator | [ParserObjects.IParser{\`\`0,\`\`1}](#T-ParserObjects-IParser{``0,``1} 'ParserObjects.IParser{``0,``1}') |  |
| minimum | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') |  |
| maximum | [System.Nullable{System.Int32}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.Int32}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TSeparator |  |
| TOutput |  |

<a name='M-ParserObjects-ParserCombinatorExtensions-ListStringsToString-ParserObjects-IParser{System-Char,System-String},System-Boolean-'></a>
### ListStringsToString(p,atLeastOne) `method`

##### Summary

Given a parser which parses strings, parse a list of strings and return the sequence as a joined
string

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{System.Char,System.String}](#T-ParserObjects-IParser{System-Char,System-String} 'ParserObjects.IParser{System.Char,System.String}') |  |
| atLeastOne | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') |  |

<a name='M-ParserObjects-ParserCombinatorExtensions-List``2-ParserObjects-IParser{``0,``1},System-Boolean-'></a>
### List\`\`2(p,atLeastOne) `method`

##### Summary

Returns a list of results from the given parser. Continues to parse until the parser returns
failure. Returns an enumerable of results.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{\`\`0,\`\`1}](#T-ParserObjects-IParser{``0,``1} 'ParserObjects.IParser{``0,``1}') |  |
| atLeastOne | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='M-ParserObjects-ParserCombinatorExtensions-List``2-ParserObjects-IParser{``0,``1},System-Int32,System-Nullable{System-Int32}-'></a>
### List\`\`2(p,minimum,maximum) `method`

##### Summary

Returns a list of results from the given parser, with limits. Continues to
parse until the parser returns failure or the maximum number of results is
reached.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{\`\`0,\`\`1}](#T-ParserObjects-IParser{``0,``1} 'ParserObjects.IParser{``0,``1}') |  |
| minimum | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') |  |
| maximum | [System.Nullable{System.Int32}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.Int32}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='M-ParserObjects-ParserCombinatorExtensions-Map``3-ParserObjects-IParser{``0,``1},System-Func{``1,``2}-'></a>
### Map\`\`3(parser,transform) `method`

##### Summary

Transform the output of the given parser. Synonym for Transform

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| parser | [ParserObjects.IParser{\`\`0,\`\`1}](#T-ParserObjects-IParser{``0,``1} 'ParserObjects.IParser{``0,``1}') |  |
| transform | [System.Func{\`\`1,\`\`2}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{``1,``2}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TMiddle |  |
| TOutput |  |

<a name='M-ParserObjects-ParserCombinatorExtensions-NotFollowedBy``2-ParserObjects-IParser{``0,``1},ParserObjects-IParser{``0}-'></a>
### NotFollowedBy\`\`2(p,lookahead) `method`

##### Summary

Zero-length assertion that the given parser's match result is not followed by a lookahead pattern.
The lookahead is compared but no input is consumed to match it.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{\`\`0,\`\`1}](#T-ParserObjects-IParser{``0,``1} 'ParserObjects.IParser{``0,``1}') |  |
| lookahead | [ParserObjects.IParser{\`\`0}](#T-ParserObjects-IParser{``0} 'ParserObjects.IParser{``0}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='M-ParserObjects-ParserCombinatorExtensions-Optional``2-ParserObjects-IParser{``0,``1},System-Func{``1}-'></a>
### Optional\`\`2(p,getDefault) `method`

##### Summary

The results of the given parser are optional. If the given parser fails, a default value will
be provided

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{\`\`0,\`\`1}](#T-ParserObjects-IParser{``0,``1} 'ParserObjects.IParser{``0,``1}') |  |
| getDefault | [System.Func{\`\`1}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{``1}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='M-ParserObjects-ParserCombinatorExtensions-Optional``2-ParserObjects-IParser{``0,``1},System-Func{ParserObjects-ISequence{``0},``1}-'></a>
### Optional\`\`2(p,getDefault) `method`

##### Summary

The results of the given parser are optiona. If the given parser fails, a default value will be
provided

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{\`\`0,\`\`1}](#T-ParserObjects-IParser{``0,``1} 'ParserObjects.IParser{``0,``1}') |  |
| getDefault | [System.Func{ParserObjects.ISequence{\`\`0},\`\`1}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{ParserObjects.ISequence{``0},``1}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='M-ParserObjects-ParserCombinatorExtensions-Replaceable``2-ParserObjects-IParser{``0,``1}-'></a>
### Replaceable\`\`2(p) `method`

##### Summary

Make this parser replaceable

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{\`\`0,\`\`1}](#T-ParserObjects-IParser{``0,``1} 'ParserObjects.IParser{``0,``1}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='M-ParserObjects-ParserCombinatorExtensions-Replaceable``2-ParserObjects-IParser{``0,``1},System-String-'></a>
### Replaceable\`\`2(p,name) `method`

##### Summary

Make this parser replaceable. Gives the parser a name so that it can be easily
found and replaced

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{\`\`0,\`\`1}](#T-ParserObjects-IParser{``0,``1} 'ParserObjects.IParser{``0,``1}') |  |
| name | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='M-ParserObjects-ParserCombinatorExtensions-Transform``3-ParserObjects-IParser{``0,``1},System-Func{``1,``2}-'></a>
### Transform\`\`3(parser,transform) `method`

##### Summary

Transform the output of the given parser to a new value

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| parser | [ParserObjects.IParser{\`\`0,\`\`1}](#T-ParserObjects-IParser{``0,``1} 'ParserObjects.IParser{``0,``1}') |  |
| transform | [System.Func{\`\`1,\`\`2}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{``1,``2}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TMiddle |  |
| TOutput |  |

<a name='T-ParserObjects-ParserExtensions'></a>
## ParserExtensions `type`

##### Namespace

ParserObjects

<a name='M-ParserObjects-ParserExtensions-Named``1-``0,System-String-'></a>
### Named\`\`1(parser,name) `method`

##### Summary

Specify a name for the parser with function syntax.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| parser | [\`\`0](#T-``0 '``0') |  |
| name | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TParser |  |

<a name='M-ParserObjects-ParserExtensions-ToBnf-ParserObjects-IParser-'></a>
### ToBnf(parser) `method`

##### Summary

Attempt to describe the parser as a string of pseudo-BNF. This feature depends on parsers having
a .Name value set. If you are using custom IParser implementations you will need to use a custom
BnfStringifyVisitor subclass to account for it.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| parser | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |

<a name='M-ParserObjects-ParserExtensions-ToSequence``2-ParserObjects-IParser{``0,``1},ParserObjects-ISequence{``0}-'></a>
### ToSequence\`\`2(parser,input) `method`

##### Summary

Convert a parser and it's input sequence into a new sequence of parse result values

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| parser | [ParserObjects.IParser{\`\`0,\`\`1}](#T-ParserObjects-IParser{``0,``1} 'ParserObjects.IParser{``0,``1}') |  |
| input | [ParserObjects.ISequence{\`\`0}](#T-ParserObjects-ISequence{``0} 'ParserObjects.ISequence{``0}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='T-ParserObjects-ParserFindReplaceExtensions'></a>
## ParserFindReplaceExtensions `type`

##### Namespace

ParserObjects

<a name='M-ParserObjects-ParserFindReplaceExtensions-FindNamed-ParserObjects-IParser,System-String-'></a>
### FindNamed(root,name) `method`

##### Summary

Recurse the tree searching for a parser with the given name. Returns the first matching result.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| root | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |
| name | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |

<a name='M-ParserObjects-ParserFindReplaceExtensions-Replace-ParserObjects-IParser,System-Func{ParserObjects-IParser,System-Boolean},ParserObjects-IParser-'></a>
### Replace(root,predicate,replacement) `method`

##### Summary

Given a parser tree, replace all children of ReplaceableParsers matching the given predicate with
the provided replacement parser.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| root | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |
| predicate | [System.Func{ParserObjects.IParser,System.Boolean}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{ParserObjects.IParser,System.Boolean}') |  |
| replacement | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |

<a name='M-ParserObjects-ParserFindReplaceExtensions-Replace-ParserObjects-IParser,ParserObjects-IParser,ParserObjects-IParser-'></a>
### Replace(root,find,replacement) `method`

##### Summary

Given a parser tree, find a ReplaceableParser with a child which is reference equal to the given
find parser, and replaces it with the given replacement parser.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| root | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |
| find | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |
| replacement | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |

<a name='M-ParserObjects-ParserFindReplaceExtensions-Replace-ParserObjects-IParser,System-String,ParserObjects-IParser-'></a>
### Replace(root,name,replacement) `method`

##### Summary

Given a parser tree, find a ReplaceableParser with the given name and replace it's child parser
with the given replacement parser

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| root | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |
| name | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |
| replacement | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |

<a name='M-ParserObjects-ParserFindReplaceExtensions-Replace``2-ParserObjects-IParser,System-Func{ParserObjects-IParser,System-Boolean},System-Func{ParserObjects-IParser{``0,``1},ParserObjects-IParser{``0,``1}}-'></a>
### Replace\`\`2(root,predicate,transform) `method`

##### Summary

Given a parser tree, find a ReplaceableParsers matching a predicate and attempt to transform
the contents using the given transformation. The contents of the ReplaceableParser will be
replaced with the transformed result if it is new and valid.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| root | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |
| predicate | [System.Func{ParserObjects.IParser,System.Boolean}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{ParserObjects.IParser,System.Boolean}') |  |
| transform | [System.Func{ParserObjects.IParser{\`\`0,\`\`1},ParserObjects.IParser{\`\`0,\`\`1}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{ParserObjects.IParser{``0,``1},ParserObjects.IParser{``0,``1}}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='M-ParserObjects-ParserFindReplaceExtensions-Replace``2-ParserObjects-IParser,System-String,System-Func{ParserObjects-IParser{``0,``1},ParserObjects-IParser{``0,``1}}-'></a>
### Replace\`\`2(root,name,transform) `method`

##### Summary

Given a parser tree, find a ReplaceableParser matching a predicate and attempt to transform
the contents using the given transformation. The contents of the ReplaceableParser will be
replaced with the transformed result if it is new and valid.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| root | [ParserObjects.IParser](#T-ParserObjects-IParser 'ParserObjects.IParser') |  |
| name | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |
| transform | [System.Func{ParserObjects.IParser{\`\`0,\`\`1},ParserObjects.IParser{\`\`0,\`\`1}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{ParserObjects.IParser{``0,``1},ParserObjects.IParser{``0,``1}}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='T-ParserObjects-ParserLogicalExtensions'></a>
## ParserLogicalExtensions `type`

##### Namespace

ParserObjects

<a name='M-ParserObjects-ParserLogicalExtensions-And``1-ParserObjects-IParser{``0},ParserObjects-IParser{``0}[]-'></a>
### And\`\`1(p1,parsers) `method`

##### Summary

Parse the given parser and all additional parsers sequentially. Consumes input but returns no
output. Will probably be used by Positive- or Negative-lookahead or If.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p1 | [ParserObjects.IParser{\`\`0}](#T-ParserObjects-IParser{``0} 'ParserObjects.IParser{``0}') |  |
| parsers | [ParserObjects.IParser{\`\`0}[]](#T-ParserObjects-IParser{``0}[] 'ParserObjects.IParser{``0}[]') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |

<a name='M-ParserObjects-ParserLogicalExtensions-If``2-ParserObjects-IParser{``0,``1},ParserObjects-IParser{``0}-'></a>
### If\`\`2(parser,predicate) `method`

##### Summary

Attempt to parse with a predicate parser, consuming no input. If the predicate parser succeeds,
parse with the given parser.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| parser | [ParserObjects.IParser{\`\`0,\`\`1}](#T-ParserObjects-IParser{``0,``1} 'ParserObjects.IParser{``0,``1}') |  |
| predicate | [ParserObjects.IParser{\`\`0}](#T-ParserObjects-IParser{``0} 'ParserObjects.IParser{``0}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='M-ParserObjects-ParserLogicalExtensions-Not``1-ParserObjects-IParser{``0}-'></a>
### Not\`\`1(p1) `method`

##### Summary

Parses with the given parser, inverting the result so Success becomes Failure and Failure becomes
Success. Consumes input but returns no output. Will probably be used by Positive- or
Negative-lookahead or If.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p1 | [ParserObjects.IParser{\`\`0}](#T-ParserObjects-IParser{``0} 'ParserObjects.IParser{``0}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |

<a name='M-ParserObjects-ParserLogicalExtensions-Or``1-ParserObjects-IParser{``0},ParserObjects-IParser{``0}[]-'></a>
### Or\`\`1(p1,parsers) `method`

##### Summary

Attempts to parse with each parser successively, returning Success if any parser succeeds
or Failure if none do. Consumes input but returns no output. Will probably be used by
Positive- or Negative-lookahed or If

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p1 | [ParserObjects.IParser{\`\`0}](#T-ParserObjects-IParser{``0} 'ParserObjects.IParser{``0}') |  |
| parsers | [ParserObjects.IParser{\`\`0}[]](#T-ParserObjects-IParser{``0}[] 'ParserObjects.IParser{``0}[]') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |

<a name='M-ParserObjects-ParserLogicalExtensions-Then``2-ParserObjects-IParser{``0},ParserObjects-IParser{``0,``1}-'></a>
### Then\`\`2(predicate,parser) `method`

##### Summary

Attempt to parse with a predicate parser, consuming no input. If the predicate parser succeeds,
parse with the given parser. This is the same operation as If with different order of operands.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| predicate | [ParserObjects.IParser{\`\`0}](#T-ParserObjects-IParser{``0} 'ParserObjects.IParser{``0}') |  |
| parser | [ParserObjects.IParser{\`\`0,\`\`1}](#T-ParserObjects-IParser{``0,``1} 'ParserObjects.IParser{``0,``1}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='T-ParserObjects-ParserMatchParseExtensions'></a>
## ParserMatchParseExtensions `type`

##### Namespace

ParserObjects

##### Summary

General-purpose extensions for IParser and descendents

<a name='M-ParserObjects-ParserMatchParseExtensions-CanMatch-ParserObjects-IParser{System-Char},System-String-'></a>
### CanMatch(parser,input) `method`

##### Summary

Convenience method for parsers which act on character sequences. Attempts a parse but does not
consume any input. Returns true if the parse would succeed, false otherwise.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| parser | [ParserObjects.IParser{System.Char}](#T-ParserObjects-IParser{System-Char} 'ParserObjects.IParser{System.Char}') |  |
| input | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |

<a name='M-ParserObjects-ParserMatchParseExtensions-CanMatch``1-ParserObjects-IParser{``0},ParserObjects-ISequence{``0}-'></a>
### CanMatch\`\`1(parser,input) `method`

##### Summary

Attempts a parse but does not consume any input. Instead it returns a boolean true if the parse
succeeded or false otherwise.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| parser | [ParserObjects.IParser{\`\`0}](#T-ParserObjects-IParser{``0} 'ParserObjects.IParser{``0}') |  |
| input | [ParserObjects.ISequence{\`\`0}](#T-ParserObjects-ISequence{``0} 'ParserObjects.ISequence{``0}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |

<a name='M-ParserObjects-ParserMatchParseExtensions-Parse``1-ParserObjects-IParser{System-Char,``0},System-String-'></a>
### Parse\`\`1(parser,s) `method`

##### Summary

Convenience method for parser which act on character sequences. Parse the given input string
and return the first value or failure.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| parser | [ParserObjects.IParser{System.Char,\`\`0}](#T-ParserObjects-IParser{System-Char,``0} 'ParserObjects.IParser{System.Char,``0}') |  |
| s | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |

<a name='T-ParserObjects-ParserMethods'></a>
## ParserMethods `type`

##### Namespace

ParserObjects

<a name='M-ParserObjects-ParserMethods-CamelCase'></a>
### CamelCase() `method`

##### Summary

Parses a CamelCase identifier and returns the list of individual strings in
the identifier. Parses lowerCamelCase and UpperCamelCase

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-ParserMethods-CharacterString-System-String-'></a>
### CharacterString(pattern) `method`

##### Summary

Convenience method to match a literal sequence of characters and return the
result as a string.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| pattern | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |

<a name='M-ParserObjects-ParserMethods-DelimitedStringWithEscapedDelimiters-System-Char,System-Char,System-Char-'></a>
### DelimitedStringWithEscapedDelimiters(openStr,closeStr,escapeStr) `method`

##### Summary

A parser for delimited strings. Returns the string literal with open sequence, close sequence,
and internal escape sequences

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| openStr | [System.Char](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Char 'System.Char') |  |
| closeStr | [System.Char](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Char 'System.Char') |  |
| escapeStr | [System.Char](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Char 'System.Char') |  |

<a name='M-ParserObjects-ParserMethods-Digit'></a>
### Digit() `method`

##### Summary

Parses a single digit 0-9

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-ParserMethods-DigitString'></a>
### DigitString() `method`

##### Summary

Parses digits in series and returns them as a string

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-ParserMethods-DoubleQuotedString'></a>
### DoubleQuotedString() `method`

##### Summary

Double-quoted string literal, with backslash-escaped quotes. The returned string is the string
literal with quotes and escapes

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-ParserMethods-HexadecimalDigit'></a>
### HexadecimalDigit() `method`

##### Summary

Returns a single hexadecimal digit: 0-9, a-f, A-F

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-ParserMethods-HexadecimalString'></a>
### HexadecimalString() `method`

##### Summary

Returns a sequence of at least one hexadecimal digits and returns them as a string.

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-ParserMethods-Line'></a>
### Line() `method`

##### Summary

Parses a line of text until a newline or end of input. Newline not included.

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-ParserMethods-NonZeroDigit'></a>
### NonZeroDigit() `method`

##### Summary

Parses a single non-zero digit 1-9

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-ParserMethods-OptionalWhitespace'></a>
### OptionalWhitespace() `method`

##### Summary

Parses an optional series of whitespace characters and returns them as a string

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-ParserMethods-PrefixedLine-System-String-'></a>
### PrefixedLine(prefix) `method`

##### Summary

Parses a line of text, starting with a prefix and going until a newline or end
of input. Newline not included.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| prefix | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |

<a name='M-ParserObjects-ParserMethods-Regex-System-String-'></a>
### Regex(pattern) `method`

##### Summary

Creates a parser which attempts to match the given regular expression from the current
position of the input stream

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| pattern | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |

<a name='M-ParserObjects-ParserMethods-SingleQuotedString'></a>
### SingleQuotedString() `method`

##### Summary

Single-quoted string literal, with backslash-escaped quotes. The returned string is the string
literal with quotes and escapes

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-ParserMethods-StrippedDelimitedStringWithEscapedDelimiters-System-Char,System-Char,System-Char-'></a>
### StrippedDelimitedStringWithEscapedDelimiters(openStr,closeStr,escapeStr) `method`

##### Summary

A parser for delimited strings. Returns the string literal, stripped of open sequence, close
sequence, and internal escape sequences.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| openStr | [System.Char](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Char 'System.Char') |  |
| closeStr | [System.Char](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Char 'System.Char') |  |
| escapeStr | [System.Char](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Char 'System.Char') |  |

<a name='M-ParserObjects-ParserMethods-StrippedDoubleQuotedString'></a>
### StrippedDoubleQuotedString() `method`

##### Summary

Double-quoted string with backslash-escaped quotes. The returned string is the string without
quotes and without internal escape sequences

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-ParserMethods-StrippedSingleQuotedString'></a>
### StrippedSingleQuotedString() `method`

##### Summary

Single-quoted string with backslash-escaped quotes. The returned string is the string without
quotes and without internal escape sequences

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-ParserMethods-Whitespace'></a>
### Whitespace() `method`

##### Summary

Parses a series of required whitespace characters and returns them as a string

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-ParserMethods-WhitespaceCharacter'></a>
### WhitespaceCharacter() `method`

##### Summary

Parses a single character of whitespace (' ', '\t', '\r', '\n','\v', etc)

##### Returns



##### Parameters

This method has no parameters.

<a name='T-ParserObjects-ParserMethods`1'></a>
## ParserMethods\`1 `type`

##### Namespace

ParserObjects

##### Summary

Parser methods for building combinators using declarative syntax

<a name='M-ParserObjects-ParserMethods`1-And-ParserObjects-IParser{`0}[]-'></a>
### And(parsers) `method`

##### Summary

Tests several parsers sequentially. Returns success if they all succeed, otherwise
returns failure. Consumes input but returns no explicit output.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| parsers | [ParserObjects.IParser{\`0}[]](#T-ParserObjects-IParser{`0}[] 'ParserObjects.IParser{`0}[]') |  |

<a name='M-ParserObjects-ParserMethods`1-Any'></a>
### Any() `method`

##### Summary

Matches anywhere in the sequence except at the end, and consumes 1 token of input

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-ParserMethods`1-Combine-ParserObjects-IParser{`0}[]-'></a>
### Combine(parsers) `method`

##### Summary

Given a list of parsers, parse each in sequence and return a list of object
results on success.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| parsers | [ParserObjects.IParser{\`0}[]](#T-ParserObjects-IParser{`0}[] 'ParserObjects.IParser{`0}[]') |  |

<a name='M-ParserObjects-ParserMethods`1-Deferred``1-System-Func{ParserObjects-IParser{`0,``0}}-'></a>
### Deferred\`\`1(getParser) `method`

##### Summary

Get a reference to a parser dynamically. Avoids circular dependencies in the grammar

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| getParser | [System.Func{ParserObjects.IParser{\`0,\`\`0}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{ParserObjects.IParser{`0,``0}}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-Empty'></a>
### Empty() `method`

##### Summary

The empty parser, consumers no input and always returns success at any point.

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-ParserMethods`1-End'></a>
### End() `method`

##### Summary

Matches affirmatively at the end of the input, fails everywhere else.

##### Returns



##### Parameters

This method has no parameters.

<a name='M-ParserObjects-ParserMethods`1-Examine``1-ParserObjects-IParser{`0,``0},System-Action{ParserObjects-Parsers-Examine{`0,``0}-Context},System-Action{ParserObjects-Parsers-Examine{`0,``0}-Context}-'></a>
### Examine\`\`1(parser,before,after) `method`

##### Summary

Invoke callbacks before and after a parse

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| parser | [ParserObjects.IParser{\`0,\`\`0}](#T-ParserObjects-IParser{`0,``0} 'ParserObjects.IParser{`0,``0}') |  |
| before | [System.Action{ParserObjects.Parsers.Examine{\`0,\`\`0}.Context}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{ParserObjects.Parsers.Examine{`0,``0}.Context}') |  |
| after | [System.Action{ParserObjects.Parsers.Examine{\`0,\`\`0}.Context}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{ParserObjects.Parsers.Examine{`0,``0}.Context}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-Fail``1-System-String-'></a>
### Fail\`\`1() `method`

##### Summary

A parser which unconditionally returns failure.

##### Returns



##### Parameters

This method has no parameters.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-First``1-ParserObjects-IParser{`0,``0}[]-'></a>
### First\`\`1(parsers) `method`

##### Summary

Return the result of the first parser which succeeds

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| parsers | [ParserObjects.IParser{\`0,\`\`0}[]](#T-ParserObjects-IParser{`0,``0}[] 'ParserObjects.IParser{`0,``0}[]') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-Function``1-ParserObjects-ParserFunction{`0,``0}-'></a>
### Function\`\`1(func) `method`

##### Summary

Invoke a function callback to perform the parse at the current location in the input
stream

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| func | [ParserObjects.ParserFunction{\`0,\`\`0}](#T-ParserObjects-ParserFunction{`0,``0} 'ParserObjects.ParserFunction{`0,``0}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-If``1-ParserObjects-IParser{`0},ParserObjects-IParser{`0,``0}-'></a>
### If\`\`1(predicate,onSuccess) `method`

##### Summary

Tests the predicate parser, consuming no input. If the predicate succeeds, perform the parse.
Otherwise return Failure.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| predicate | [ParserObjects.IParser{\`0}](#T-ParserObjects-IParser{`0} 'ParserObjects.IParser{`0}') |  |
| onSuccess | [ParserObjects.IParser{\`0,\`\`0}](#T-ParserObjects-IParser{`0,``0} 'ParserObjects.IParser{`0,``0}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-LeftApply``1-ParserObjects-IParser{`0,``0},System-Func{ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0,``0}},ParserObjects-Parsers-ApplyArity-'></a>
### LeftApply\`\`1(left,getRight,arity) `method`

##### Summary

A left-associative parser where the left item is parsed unconditionally, and the result of the
left parser is applied to the right parser. This new result is then treated as the 'left' value
for the next iteration of the right parser. This can be used when many rules have a common prefix
and you don't want to backtrack through the prefix on every attempt.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| left | [ParserObjects.IParser{\`0,\`\`0}](#T-ParserObjects-IParser{`0,``0} 'ParserObjects.IParser{`0,``0}') |  |
| getRight | [System.Func{ParserObjects.IParser{\`0,\`\`0},ParserObjects.IParser{\`0,\`\`0}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{ParserObjects.IParser{`0,``0},ParserObjects.IParser{`0,``0}}') |  |
| arity | [ParserObjects.Parsers.ApplyArity](#T-ParserObjects-Parsers-ApplyArity 'ParserObjects.Parsers.ApplyArity') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-List``1-ParserObjects-IParser{`0,``0},System-Boolean-'></a>
### List\`\`1(p,atLeastOne) `method`

##### Summary

Parse a list of items.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{\`0,\`\`0}](#T-ParserObjects-IParser{`0,``0} 'ParserObjects.IParser{`0,``0}') |  |
| atLeastOne | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the list must have at least one element or the parse fails. If
false, an empty list returns success. |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-List``1-ParserObjects-IParser{`0,``0},System-Int32,System-Nullable{System-Int32}-'></a>
### List\`\`1(p,minimum,maximum) `method`

##### Summary

Parse a list of items with defined minimum and maximum quantities.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{\`0,\`\`0}](#T-ParserObjects-IParser{`0,``0} 'ParserObjects.IParser{`0,``0}') |  |
| minimum | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') |  |
| maximum | [System.Nullable{System.Int32}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.Int32}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-Match-System-Func{`0,System-Boolean}-'></a>
### Match(predicate) `method`

##### Summary

Test the next input value and return it, if it matches the predicate

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| predicate | [System.Func{\`0,System.Boolean}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{`0,System.Boolean}') |  |

<a name='M-ParserObjects-ParserMethods`1-Match-`0-'></a>
### Match(pattern) `method`

##### Summary

Get the next input value and return it if it .Equals() to the given value

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| pattern | [\`0](#T-`0 '`0') |  |

<a name='M-ParserObjects-ParserMethods`1-Match-System-Collections-Generic-IEnumerable{`0}-'></a>
### Match(pattern) `method`

##### Summary

Get the next few input values and compare them one-by-one against an ordered sequence of test
values. If every value in the sequence matches, return the sequence as a list.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| pattern | [System.Collections.Generic.IEnumerable{\`0}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IEnumerable 'System.Collections.Generic.IEnumerable{`0}') |  |

<a name='M-ParserObjects-ParserMethods`1-MatchAny-System-Collections-Generic-IEnumerable{System-String}-'></a>
### MatchAny(patterns) `method`

##### Summary

Optimized implementation of First() which returns an input which matches any of the given pattern
strings. Uses a Trie internally to greedily match the longest matching input sequence

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| patterns | [System.Collections.Generic.IEnumerable{System.String}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IEnumerable 'System.Collections.Generic.IEnumerable{System.String}') |  |

<a name='M-ParserObjects-ParserMethods`1-NegativeLookahead-ParserObjects-IParser{`0}-'></a>
### NegativeLookahead(p) `method`

##### Summary

Zero-length assertion that the given pattern does not match from the current position. No
input is consumed

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{\`0}](#T-ParserObjects-IParser{`0} 'ParserObjects.IParser{`0}') |  |

<a name='M-ParserObjects-ParserMethods`1-Not-ParserObjects-IParser{`0}-'></a>
### Not(p1) `method`

##### Summary

Invoke the given parser and invert the result. On Success return Failure, on Failure return
Success. Consumes input but returns no output.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p1 | [ParserObjects.IParser{\`0}](#T-ParserObjects-IParser{`0} 'ParserObjects.IParser{`0}') |  |

<a name='M-ParserObjects-ParserMethods`1-Optional``1-ParserObjects-IParser{`0,``0},System-Func{``0}-'></a>
### Optional\`\`1(p,getDefault) `method`

##### Summary

Attempt to parse an item and return a default value otherwise

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{\`0,\`\`0}](#T-ParserObjects-IParser{`0,``0} 'ParserObjects.IParser{`0,``0}') |  |
| getDefault | [System.Func{\`\`0}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{``0}') |  |

<a name='M-ParserObjects-ParserMethods`1-Optional``1-ParserObjects-IParser{`0,``0},System-Func{ParserObjects-ISequence{`0},``0}-'></a>
### Optional\`\`1(p,getDefault) `method`

##### Summary

Attempt to parse an item and return a default value otherwise

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{\`0,\`\`0}](#T-ParserObjects-IParser{`0,``0} 'ParserObjects.IParser{`0,``0}') |  |
| getDefault | [System.Func{ParserObjects.ISequence{\`0},\`\`0}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{ParserObjects.ISequence{`0},``0}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-Or-ParserObjects-IParser{`0}[]-'></a>
### Or(parsers) `method`

##### Summary

Tests several parsers sequentially. Returns Success if any parser succeeds, returns
Failure otherwise. Consumes input but returns no explicit output.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| parsers | [ParserObjects.IParser{\`0}[]](#T-ParserObjects-IParser{`0}[] 'ParserObjects.IParser{`0}[]') |  |

<a name='M-ParserObjects-ParserMethods`1-PositiveLookahead-ParserObjects-IParser{`0}-'></a>
### PositiveLookahead(p) `method`

##### Summary

Zero-length assertion that the given pattern matches from the current position. No input is
consumed.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{\`0}](#T-ParserObjects-IParser{`0} 'ParserObjects.IParser{`0}') |  |

<a name='M-ParserObjects-ParserMethods`1-Produce``1-System-Func{``0}-'></a>
### Produce\`\`1(produce) `method`

##### Summary

Produce a value without consuming anything out of the input sequence

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| produce | [System.Func{\`\`0}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{``0}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-Produce``1-System-Func{ParserObjects-ISequence{`0},``0}-'></a>
### Produce\`\`1(produce) `method`

##### Summary

Produce a value given the current state of the input sequence.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| produce | [System.Func{ParserObjects.ISequence{\`0},\`\`0}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{ParserObjects.ISequence{`0},``0}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-Replaceable``1-ParserObjects-IParser{`0,``0}-'></a>
### Replaceable\`\`1(defaultParser) `method`

##### Summary

Serves as a placeholder in the parser tree where an in-place replacement can be made.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| defaultParser | [ParserObjects.IParser{\`0,\`\`0}](#T-ParserObjects-IParser{`0,``0} 'ParserObjects.IParser{`0,``0}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-RightApply``2-ParserObjects-IParser{`0,``1},ParserObjects-IParser{`0,``0},System-Func{``1,``0,``1,``1},System-Func{ParserObjects-ISequence{`0},``1}-'></a>
### RightApply\`\`2(item,middle,produce,getMissingRight) `method`

##### Summary

a right-associative parser where the parser attempts to parse a sequence of items and middles
recursively: self := <item> (<middle> <self>)*.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| item | [ParserObjects.IParser{\`0,\`\`1}](#T-ParserObjects-IParser{`0,``1} 'ParserObjects.IParser{`0,``1}') |  |
| middle | [ParserObjects.IParser{\`0,\`\`0}](#T-ParserObjects-IParser{`0,``0} 'ParserObjects.IParser{`0,``0}') |  |
| produce | [System.Func{\`\`1,\`\`0,\`\`1,\`\`1}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{``1,``0,``1,``1}') |  |
| getMissingRight | [System.Func{ParserObjects.ISequence{\`0},\`\`1}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{ParserObjects.ISequence{`0},``1}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMiddle |  |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-Rule``10-ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0,``1},ParserObjects-IParser{`0,``2},ParserObjects-IParser{`0,``3},ParserObjects-IParser{`0,``4},ParserObjects-IParser{`0,``5},ParserObjects-IParser{`0,``6},ParserObjects-IParser{`0,``7},ParserObjects-IParser{`0,``8},System-Func{``0,``1,``2,``3,``4,``5,``6,``7,``8,``9}-'></a>
### Rule\`\`10(p1,p2,p3,p4,p5,p6,p7,p8,p9,produce) `method`

##### Summary

Parse a sequence of productions and reduce them into a single output. If any item fails, rollback
all and return failure

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p1 | [ParserObjects.IParser{\`0,\`\`0}](#T-ParserObjects-IParser{`0,``0} 'ParserObjects.IParser{`0,``0}') |  |
| p2 | [ParserObjects.IParser{\`0,\`\`1}](#T-ParserObjects-IParser{`0,``1} 'ParserObjects.IParser{`0,``1}') |  |
| p3 | [ParserObjects.IParser{\`0,\`\`2}](#T-ParserObjects-IParser{`0,``2} 'ParserObjects.IParser{`0,``2}') |  |
| p4 | [ParserObjects.IParser{\`0,\`\`3}](#T-ParserObjects-IParser{`0,``3} 'ParserObjects.IParser{`0,``3}') |  |
| p5 | [ParserObjects.IParser{\`0,\`\`4}](#T-ParserObjects-IParser{`0,``4} 'ParserObjects.IParser{`0,``4}') |  |
| p6 | [ParserObjects.IParser{\`0,\`\`5}](#T-ParserObjects-IParser{`0,``5} 'ParserObjects.IParser{`0,``5}') |  |
| p7 | [ParserObjects.IParser{\`0,\`\`6}](#T-ParserObjects-IParser{`0,``6} 'ParserObjects.IParser{`0,``6}') |  |
| p8 | [ParserObjects.IParser{\`0,\`\`7}](#T-ParserObjects-IParser{`0,``7} 'ParserObjects.IParser{`0,``7}') |  |
| p9 | [ParserObjects.IParser{\`0,\`\`8}](#T-ParserObjects-IParser{`0,``8} 'ParserObjects.IParser{`0,``8}') |  |
| produce | [System.Func{\`\`0,\`\`1,\`\`2,\`\`3,\`\`4,\`\`5,\`\`6,\`\`7,\`\`8,\`\`9}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{``0,``1,``2,``3,``4,``5,``6,``7,``8,``9}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T1 |  |
| T2 |  |
| T3 |  |
| T4 |  |
| T5 |  |
| T6 |  |
| T7 |  |
| T8 |  |
| T9 |  |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-Rule``3-ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0,``1},System-Func{``0,``1,``2}-'></a>
### Rule\`\`3(p1,p2,produce) `method`

##### Summary

Parse a sequence of productions and reduce them into a single output. If any item fails, rollback
all and return failure

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p1 | [ParserObjects.IParser{\`0,\`\`0}](#T-ParserObjects-IParser{`0,``0} 'ParserObjects.IParser{`0,``0}') |  |
| p2 | [ParserObjects.IParser{\`0,\`\`1}](#T-ParserObjects-IParser{`0,``1} 'ParserObjects.IParser{`0,``1}') |  |
| produce | [System.Func{\`\`0,\`\`1,\`\`2}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{``0,``1,``2}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T2 |  |
| TOutput |  |
| T1 |  |

<a name='M-ParserObjects-ParserMethods`1-Rule``4-ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0,``1},ParserObjects-IParser{`0,``2},System-Func{``0,``1,``2,``3}-'></a>
### Rule\`\`4(p1,p2,p3,produce) `method`

##### Summary

Parse a sequence of productions and reduce them into a single output. If any item fails, rollback
all and return failure

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p1 | [ParserObjects.IParser{\`0,\`\`0}](#T-ParserObjects-IParser{`0,``0} 'ParserObjects.IParser{`0,``0}') |  |
| p2 | [ParserObjects.IParser{\`0,\`\`1}](#T-ParserObjects-IParser{`0,``1} 'ParserObjects.IParser{`0,``1}') |  |
| p3 | [ParserObjects.IParser{\`0,\`\`2}](#T-ParserObjects-IParser{`0,``2} 'ParserObjects.IParser{`0,``2}') |  |
| produce | [System.Func{\`\`0,\`\`1,\`\`2,\`\`3}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{``0,``1,``2,``3}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T1 |  |
| T2 |  |
| T3 |  |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-Rule``5-ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0,``1},ParserObjects-IParser{`0,``2},ParserObjects-IParser{`0,``3},System-Func{``0,``1,``2,``3,``4}-'></a>
### Rule\`\`5(p1,p2,p3,p4,produce) `method`

##### Summary

Parse a sequence of productions and reduce them into a single output. If any item fails, rollback
all and return failure

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p1 | [ParserObjects.IParser{\`0,\`\`0}](#T-ParserObjects-IParser{`0,``0} 'ParserObjects.IParser{`0,``0}') |  |
| p2 | [ParserObjects.IParser{\`0,\`\`1}](#T-ParserObjects-IParser{`0,``1} 'ParserObjects.IParser{`0,``1}') |  |
| p3 | [ParserObjects.IParser{\`0,\`\`2}](#T-ParserObjects-IParser{`0,``2} 'ParserObjects.IParser{`0,``2}') |  |
| p4 | [ParserObjects.IParser{\`0,\`\`3}](#T-ParserObjects-IParser{`0,``3} 'ParserObjects.IParser{`0,``3}') |  |
| produce | [System.Func{\`\`0,\`\`1,\`\`2,\`\`3,\`\`4}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{``0,``1,``2,``3,``4}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T1 |  |
| T2 |  |
| T3 |  |
| T4 |  |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-Rule``6-ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0,``1},ParserObjects-IParser{`0,``2},ParserObjects-IParser{`0,``3},ParserObjects-IParser{`0,``4},System-Func{``0,``1,``2,``3,``4,``5}-'></a>
### Rule\`\`6(p1,p2,p3,p4,p5,produce) `method`

##### Summary

Parse a sequence of productions and reduce them into a single output. If any item fails, rollback
all and return failure

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p1 | [ParserObjects.IParser{\`0,\`\`0}](#T-ParserObjects-IParser{`0,``0} 'ParserObjects.IParser{`0,``0}') |  |
| p2 | [ParserObjects.IParser{\`0,\`\`1}](#T-ParserObjects-IParser{`0,``1} 'ParserObjects.IParser{`0,``1}') |  |
| p3 | [ParserObjects.IParser{\`0,\`\`2}](#T-ParserObjects-IParser{`0,``2} 'ParserObjects.IParser{`0,``2}') |  |
| p4 | [ParserObjects.IParser{\`0,\`\`3}](#T-ParserObjects-IParser{`0,``3} 'ParserObjects.IParser{`0,``3}') |  |
| p5 | [ParserObjects.IParser{\`0,\`\`4}](#T-ParserObjects-IParser{`0,``4} 'ParserObjects.IParser{`0,``4}') |  |
| produce | [System.Func{\`\`0,\`\`1,\`\`2,\`\`3,\`\`4,\`\`5}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{``0,``1,``2,``3,``4,``5}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T1 |  |
| T2 |  |
| T3 |  |
| T4 |  |
| T5 |  |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-Rule``7-ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0,``1},ParserObjects-IParser{`0,``2},ParserObjects-IParser{`0,``3},ParserObjects-IParser{`0,``4},ParserObjects-IParser{`0,``5},System-Func{``0,``1,``2,``3,``4,``5,``6}-'></a>
### Rule\`\`7(p1,p2,p3,p4,p5,p6,produce) `method`

##### Summary

Parse a sequence of productions and reduce them into a single output. If any item fails, rollback
all and return failure

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p1 | [ParserObjects.IParser{\`0,\`\`0}](#T-ParserObjects-IParser{`0,``0} 'ParserObjects.IParser{`0,``0}') |  |
| p2 | [ParserObjects.IParser{\`0,\`\`1}](#T-ParserObjects-IParser{`0,``1} 'ParserObjects.IParser{`0,``1}') |  |
| p3 | [ParserObjects.IParser{\`0,\`\`2}](#T-ParserObjects-IParser{`0,``2} 'ParserObjects.IParser{`0,``2}') |  |
| p4 | [ParserObjects.IParser{\`0,\`\`3}](#T-ParserObjects-IParser{`0,``3} 'ParserObjects.IParser{`0,``3}') |  |
| p5 | [ParserObjects.IParser{\`0,\`\`4}](#T-ParserObjects-IParser{`0,``4} 'ParserObjects.IParser{`0,``4}') |  |
| p6 | [ParserObjects.IParser{\`0,\`\`5}](#T-ParserObjects-IParser{`0,``5} 'ParserObjects.IParser{`0,``5}') |  |
| produce | [System.Func{\`\`0,\`\`1,\`\`2,\`\`3,\`\`4,\`\`5,\`\`6}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{``0,``1,``2,``3,``4,``5,``6}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T1 |  |
| T2 |  |
| T3 |  |
| T4 |  |
| T5 |  |
| T6 |  |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-Rule``8-ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0,``1},ParserObjects-IParser{`0,``2},ParserObjects-IParser{`0,``3},ParserObjects-IParser{`0,``4},ParserObjects-IParser{`0,``5},ParserObjects-IParser{`0,``6},System-Func{``0,``1,``2,``3,``4,``5,``6,``7}-'></a>
### Rule\`\`8(p1,p2,p3,p4,p5,p6,p7,produce) `method`

##### Summary

Parse a sequence of productions and reduce them into a single output. If any item fails, rollback
all and return failure

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p1 | [ParserObjects.IParser{\`0,\`\`0}](#T-ParserObjects-IParser{`0,``0} 'ParserObjects.IParser{`0,``0}') |  |
| p2 | [ParserObjects.IParser{\`0,\`\`1}](#T-ParserObjects-IParser{`0,``1} 'ParserObjects.IParser{`0,``1}') |  |
| p3 | [ParserObjects.IParser{\`0,\`\`2}](#T-ParserObjects-IParser{`0,``2} 'ParserObjects.IParser{`0,``2}') |  |
| p4 | [ParserObjects.IParser{\`0,\`\`3}](#T-ParserObjects-IParser{`0,``3} 'ParserObjects.IParser{`0,``3}') |  |
| p5 | [ParserObjects.IParser{\`0,\`\`4}](#T-ParserObjects-IParser{`0,``4} 'ParserObjects.IParser{`0,``4}') |  |
| p6 | [ParserObjects.IParser{\`0,\`\`5}](#T-ParserObjects-IParser{`0,``5} 'ParserObjects.IParser{`0,``5}') |  |
| p7 | [ParserObjects.IParser{\`0,\`\`6}](#T-ParserObjects-IParser{`0,``6} 'ParserObjects.IParser{`0,``6}') |  |
| produce | [System.Func{\`\`0,\`\`1,\`\`2,\`\`3,\`\`4,\`\`5,\`\`6,\`\`7}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{``0,``1,``2,``3,``4,``5,``6,``7}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T1 |  |
| T2 |  |
| T3 |  |
| T4 |  |
| T5 |  |
| T6 |  |
| T7 |  |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-Rule``9-ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0,``1},ParserObjects-IParser{`0,``2},ParserObjects-IParser{`0,``3},ParserObjects-IParser{`0,``4},ParserObjects-IParser{`0,``5},ParserObjects-IParser{`0,``6},ParserObjects-IParser{`0,``7},System-Func{``0,``1,``2,``3,``4,``5,``6,``7,``8}-'></a>
### Rule\`\`9(p1,p2,p3,p4,p5,p6,p7,p8,produce) `method`

##### Summary

Parse a sequence of productions and reduce them into a single output. If any item fails, rollback
all and return failure

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p1 | [ParserObjects.IParser{\`0,\`\`0}](#T-ParserObjects-IParser{`0,``0} 'ParserObjects.IParser{`0,``0}') |  |
| p2 | [ParserObjects.IParser{\`0,\`\`1}](#T-ParserObjects-IParser{`0,``1} 'ParserObjects.IParser{`0,``1}') |  |
| p3 | [ParserObjects.IParser{\`0,\`\`2}](#T-ParserObjects-IParser{`0,``2} 'ParserObjects.IParser{`0,``2}') |  |
| p4 | [ParserObjects.IParser{\`0,\`\`3}](#T-ParserObjects-IParser{`0,``3} 'ParserObjects.IParser{`0,``3}') |  |
| p5 | [ParserObjects.IParser{\`0,\`\`4}](#T-ParserObjects-IParser{`0,``4} 'ParserObjects.IParser{`0,``4}') |  |
| p6 | [ParserObjects.IParser{\`0,\`\`5}](#T-ParserObjects-IParser{`0,``5} 'ParserObjects.IParser{`0,``5}') |  |
| p7 | [ParserObjects.IParser{\`0,\`\`6}](#T-ParserObjects-IParser{`0,``6} 'ParserObjects.IParser{`0,``6}') |  |
| p8 | [ParserObjects.IParser{\`0,\`\`7}](#T-ParserObjects-IParser{`0,``7} 'ParserObjects.IParser{`0,``7}') |  |
| produce | [System.Func{\`\`0,\`\`1,\`\`2,\`\`3,\`\`4,\`\`5,\`\`6,\`\`7,\`\`8}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{``0,``1,``2,``3,``4,``5,``6,``7,``8}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T1 |  |
| T2 |  |
| T3 |  |
| T4 |  |
| T5 |  |
| T6 |  |
| T7 |  |
| T8 |  |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-SeparatedList``1-ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0},System-Boolean-'></a>
### SeparatedList\`\`1(p,separator,atLeastOne) `method`

##### Summary

Parse a list of items separated by a separator pattern.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{\`0,\`\`0}](#T-ParserObjects-IParser{`0,``0} 'ParserObjects.IParser{`0,``0}') |  |
| separator | [ParserObjects.IParser{\`0}](#T-ParserObjects-IParser{`0} 'ParserObjects.IParser{`0}') |  |
| atLeastOne | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | True if the list must contain at least one element or failure. False
if an empty list can be returned. |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-SeparatedList``1-ParserObjects-IParser{`0,``0},ParserObjects-IParser{`0},System-Int32,System-Nullable{System-Int32}-'></a>
### SeparatedList\`\`1(p,separator,minimum,maximum) `method`

##### Summary

Parse a list of items separated by a separator pattern, with minimum and
maximum item counts

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| p | [ParserObjects.IParser{\`0,\`\`0}](#T-ParserObjects-IParser{`0,``0} 'ParserObjects.IParser{`0,``0}') |  |
| separator | [ParserObjects.IParser{\`0}](#T-ParserObjects-IParser{`0} 'ParserObjects.IParser{`0}') |  |
| minimum | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') |  |
| maximum | [System.Nullable{System.Int32}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.Int32}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-Transform``2-ParserObjects-IParser{`0,``0},System-Func{``0,``1}-'></a>
### Transform\`\`2(parser,transform) `method`

##### Summary

Transform one node into another node to fit into the grammar

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| parser | [ParserObjects.IParser{\`0,\`\`0}](#T-ParserObjects-IParser{`0,``0} 'ParserObjects.IParser{`0,``0}') |  |
| transform | [System.Func{\`\`0,\`\`1}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{``0,``1}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |
| TMiddle |  |

<a name='M-ParserObjects-ParserMethods`1-Trie``1-ParserObjects-IReadOnlyTrie{`0,``0}-'></a>
### Trie\`\`1(readOnlyTrie) `method`

##### Summary

Look up sequences of inputs in an ITrie to greedily find the longest matching sequence

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| readOnlyTrie | [ParserObjects.IReadOnlyTrie{\`0,\`\`0}](#T-ParserObjects-IReadOnlyTrie{`0,``0} 'ParserObjects.IReadOnlyTrie{`0,``0}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |

<a name='M-ParserObjects-ParserMethods`1-Trie``1-System-Action{ParserObjects-IInsertableTrie{`0,``0}}-'></a>
### Trie\`\`1(setupTrie) `method`

##### Summary

Lookup sequences of inputs in an ITrie to greedily find the longest matching sequence.
Provides a trie instance and a callback to populate it with values

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| setupTrie | [System.Action{ParserObjects.IInsertableTrie{\`0,\`\`0}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{ParserObjects.IInsertableTrie{`0,``0}}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |

<a name='T-ParserObjects-Parsers-PositiveLookaheadParser`1'></a>
## PositiveLookaheadParser\`1 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

Does a lookahead to see if there is a match. Returns a success or failure result, but does not
consume any actual input

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |

<a name='T-ParserObjects-Parsers-ProduceParser`2'></a>
## ProduceParser\`2 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

Parser to produce an output node unconditionally. Consumes no input.
This is used to provide a default node value

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |
| TInput |  |

<a name='T-ParserObjects-Parsers-ReplaceableParser`2'></a>
## ReplaceableParser\`2 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

Delegates to an internal parser, and allows the internal parser to be replaced in-place without
returning a new instance or causing a tree rewrite.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='T-ParserObjects-Parsers-RuleParser`2'></a>
## RuleParser\`2 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

Parses a list of steps in sequence and produces a single output as a combination of outputs of
each step. Succeeds or fails as an atomic unit.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |
| TInput |  |

<a name='T-ParserObjects-Sequences-SequenceEnumerable`1'></a>
## SequenceEnumerable\`1 `type`

##### Namespace

ParserObjects.Sequences

##### Summary

Adaptor to convert ISequence to IEnumerable

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T |  |

<a name='T-ParserObjects-SequenceExtensions'></a>
## SequenceExtensions `type`

##### Namespace

ParserObjects

<a name='M-ParserObjects-SequenceExtensions-AsEnumerable``1-ParserObjects-ISequence{``0}-'></a>
### AsEnumerable\`\`1(input) `method`

##### Summary

Convert the sequence to an IEnumerable

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| input | [ParserObjects.ISequence{\`\`0}](#T-ParserObjects-ISequence{``0} 'ParserObjects.ISequence{``0}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T |  |

<a name='M-ParserObjects-SequenceExtensions-Parse``2-ParserObjects-ISequence{``0},System-Func{ParserObjects-ISequence{``0},ParserObjects-IResult{``1}}-'></a>
### Parse\`\`2(input,parse) `method`

##### Summary

Use a custom callback function to parse the input sequence

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| input | [ParserObjects.ISequence{\`\`0}](#T-ParserObjects-ISequence{``0} 'ParserObjects.ISequence{``0}') |  |
| parse | [System.Func{ParserObjects.ISequence{\`\`0},ParserObjects.IResult{\`\`1}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{ParserObjects.ISequence{``0},ParserObjects.IResult{``1}}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='M-ParserObjects-SequenceExtensions-Select``2-ParserObjects-ISequence{``0},System-Func{``0,``1}-'></a>
### Select\`\`2(input,map) `method`

##### Summary

Transform a sequence of one type into a sequence of another type by applying a transformation
function to every element.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| input | [ParserObjects.ISequence{\`\`0}](#T-ParserObjects-ISequence{``0} 'ParserObjects.ISequence{``0}') |  |
| map | [System.Func{\`\`0,\`\`1}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{``0,``1}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

<a name='M-ParserObjects-SequenceExtensions-Where``1-ParserObjects-ISequence{``0},System-Func{``0,System-Boolean}-'></a>
### Where\`\`1(input,predicate) `method`

##### Summary

Filter elements in a sequence to only return items which match a predicate

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| input | [ParserObjects.ISequence{\`\`0}](#T-ParserObjects-ISequence{``0} 'ParserObjects.ISequence{``0}') |  |
| predicate | [System.Func{\`\`0,System.Boolean}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{``0,System.Boolean}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T |  |

<a name='T-ParserObjects-SqlStyleParserMethods'></a>
## SqlStyleParserMethods `type`

##### Namespace

ParserObjects

<a name='M-ParserObjects-SqlStyleParserMethods-Comment'></a>
### Comment() `method`

##### Summary

SQL-style comment '--' ....

##### Returns



##### Parameters

This method has no parameters.

<a name='T-ParserObjects-Sequences-StreamCharacterSequence'></a>
## StreamCharacterSequence `type`

##### Namespace

ParserObjects.Sequences

##### Summary

A sequence of characters read from a Stream, such as from a file

<a name='T-ParserObjects-Sequences-StringCharacterSequence'></a>
## StringCharacterSequence `type`

##### Namespace

ParserObjects.Sequences

##### Summary

A sequence of characters read from a string

<a name='T-ParserObjects-Sequences-StringExtensions'></a>
## StringExtensions `type`

##### Namespace

ParserObjects.Sequences

<a name='M-ParserObjects-Sequences-StringExtensions-ToCharacterSequence-System-String-'></a>
### ToCharacterSequence(str) `method`

##### Summary

Wrap the string as a sequence of characters

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| str | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |

<a name='T-ParserObjects-Parsers-TransformParser`3'></a>
## TransformParser\`3 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

Transforms the output of one parser into a different form based on context

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TOutput |  |
| TInput |  |
| TMiddle |  |

<a name='T-ParserObjects-TrieExtensions'></a>
## TrieExtensions `type`

##### Namespace

ParserObjects

<a name='M-ParserObjects-TrieExtensions-Add-ParserObjects-IInsertableTrie{System-Char,System-String},System-String-'></a>
### Add(readOnlyTrie,value) `method`

##### Summary

Convenience method to add a string value with char keys

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| readOnlyTrie | [ParserObjects.IInsertableTrie{System.Char,System.String}](#T-ParserObjects-IInsertableTrie{System-Char,System-String} 'ParserObjects.IInsertableTrie{System.Char,System.String}') |  |
| value | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |

<a name='M-ParserObjects-TrieExtensions-AddMany-ParserObjects-IInsertableTrie{System-Char,System-String},System-String[]-'></a>
### AddMany(readOnlyTrie,values) `method`

##### Summary

Convenience method to add strings to the trie with char keys

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| readOnlyTrie | [ParserObjects.IInsertableTrie{System.Char,System.String}](#T-ParserObjects-IInsertableTrie{System-Char,System-String} 'ParserObjects.IInsertableTrie{System.Char,System.String}') |  |
| values | [System.String[]](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String[] 'System.String[]') |  |

<a name='M-ParserObjects-TrieExtensions-ToParser``2-ParserObjects-IReadOnlyTrie{``0,``1}-'></a>
### ToParser\`\`2(readOnlyTrie) `method`

##### Summary

Wrap the Trie in a TrieParser

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| readOnlyTrie | [ParserObjects.IReadOnlyTrie{\`\`0,\`\`1}](#T-ParserObjects-IReadOnlyTrie{``0,``1} 'ParserObjects.IReadOnlyTrie{``0,``1}') |  |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TKey |  |
| TResult |  |

<a name='T-ParserObjects-Parsers-TrieParser`2'></a>
## TrieParser\`2 `type`

##### Namespace

ParserObjects.Parsers

##### Summary

Parser which wraps an ITrie to be able to return elements which match one of several possible
input patterns. Adaptor from ITrie to IParser.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TInput |  |
| TOutput |  |

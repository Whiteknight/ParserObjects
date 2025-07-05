using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ParserObjects.CodeGen
{
    [Generator]
    public class SourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource(
                    "Parsers.Rules.g.cs",
                    SourceText.From(new RulesGenerator().GetRulesDefs(), Encoding.UTF8)
                );
                ctx.AddSource(
                    "Tuples.Combine.g.cs",
                    SourceText.From(new TuplesCombineGenerator().GetTuplesDefs(), Encoding.UTF8)
                );
                ctx.AddSource(
                    "Tuples.First.g.cs",
                    SourceText.From(new TuplesFirstGenerator().GetTuplesDefs(), Encoding.UTF8)
                );
                ctx.AddSource(
                    "Tuples.Rule.g.cs",
                    SourceText.From(new TuplesRuleGenerator().GetTuplesDefs(), Encoding.UTF8)
                );
            });
        }
    }
}

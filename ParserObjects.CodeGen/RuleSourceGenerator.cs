using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ParserObjects.CodeGen
{
    [Generator]
    public class RuleSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx => ctx
                .AddSource(
                    "Parsers.Rules.g.cs",
                    SourceText.From(new RulesGenerator().GetRulesDefs(), Encoding.UTF8)
                )
            );
        }
    }
}

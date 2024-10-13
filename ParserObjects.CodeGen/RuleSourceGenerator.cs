using Microsoft.CodeAnalysis;

namespace ParserObjects.CodeGen
{
    [Generator]
    public class RuleSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource("Parsers.Rules.g.cs", new RulesGenerator().GetRulesDefs());
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
        }
    }
}

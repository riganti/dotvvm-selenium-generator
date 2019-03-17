using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Tools.SeleniumGenerator.Helpers;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class DotvvmControlGenerator : SeleniumGenerator<HtmlGenericControl>
    {
        public override DotvvmProperty[] NameProperties { get; } = { };
        public override bool CanUseControlContentForName => false;

        public override bool CanAddDeclarations(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            // check if node is user control
            return context.Control.DothtmlNode is DothtmlElementNode htmlNode 
                   && htmlNode.TagPrefix != null 
                   && htmlNode.TagPrefix != "dot";
        }

        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            var type = $"{context.UniqueName}PageObject";

            pageObject.MemberDeclarations.Add(GeneratePropertyForProxy(context.UniqueName, type));
            pageObject.ConstructorStatements.Add(RoslynGeneratingHelper.GenerateInitializerForTemplate(context.UniqueName, type));
        }
    }
}

using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class LiteralControlGenerator : SeleniumGenerator<Literal>
    {
        private static readonly DotvvmProperty[] nameProperties = new[] { Literal.TextProperty };

        public override DotvvmProperty[] NameProperties => nameProperties;

        public override bool CanUseControlContentForName => true;


        public override bool CanAddDeclarations(PageObjectDefinition pageObjectDefinition, SeleniumGeneratorContext context)
        {
            if (context.Control.TryGetProperty(Literal.RenderSpanElementProperty, out var setter))
            {
                if (((ResolvedPropertyValue) setter).Value as bool? == false)
                {
                    return false;
                }
            }

            return base.CanAddDeclarations(pageObjectDefinition, context);
        }

        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            var type = "DotVVM.Framework.Testing.SeleniumHelpers.Proxies.LiteralProxy";
            pageObject.Members.Add(GeneratePropertyForProxy(context, type));
            pageObject.ConstructorStatements.Add(GenerateInitializerForProxy(context, context.UniqueName, type));
        }

    }
}

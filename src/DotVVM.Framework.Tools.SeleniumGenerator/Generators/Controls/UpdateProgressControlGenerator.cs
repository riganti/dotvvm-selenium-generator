using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class UpdateProgressControlGenerator : SeleniumGenerator<UpdateProgress>
    {
        private static readonly DotvvmProperty[] nameProperties = { HtmlGenericControl.InnerTextProperty };

        public override DotvvmProperty[] NameProperties => nameProperties;
        public override bool CanUseControlContentForName => true;
        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            var type = "DotVVM.Framework.Testing.SeleniumHelpers.Proxies.UpdateProgressProxy";
            pageObject.Members.Add(GeneratePropertyForProxy(context, type));
            pageObject.ConstructorStatements.Add(GenerateInitializerForProxy(context, context.UniqueName, type));
        }
    }
}

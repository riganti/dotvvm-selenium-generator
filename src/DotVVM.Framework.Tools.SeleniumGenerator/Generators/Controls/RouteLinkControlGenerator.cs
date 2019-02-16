using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class RouteLinkControlGenerator : SeleniumGenerator<RouteLink>
    {
        public override DotvvmProperty[] NameProperties { get; } = { RouteLink.TextProperty, HtmlGenericControl.InnerTextProperty, RouteLink.RouteNameProperty };

        public override bool CanUseControlContentForName => true;
        protected override void AddDeclarationsCore(HelperDefinition helper, SeleniumGeneratorContext context)
        {
            var type = "DotVVM.Framework.Testing.SeleniumHelpers.Proxies.RouteLinkProxy";
            helper.Members.Add(GeneratePropertyForProxy(context, type));
            helper.ConstructorStatements.Add(GenerateInitializerForProxy(context, context.UniqueName, type));
        }
    }
}
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class GridViewControlGenerator : SeleniumGenerator<GridView>
    {
        private static readonly DotvvmProperty[] nameProperties = { ItemsControl.DataSourceProperty};

        public override DotvvmProperty[] NameProperties => nameProperties;

        public override bool CanUseControlContentForName => false;

        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            var type = "DotVVM.Framework.Testing.SeleniumHelpers.Proxies.GridViewProxy";
            pageObject.Members.Add(GeneratePropertyForProxy(context, type));
            pageObject.ConstructorStatements.Add(GenerateInitializerForProxy(context, context.UniqueName, type));
        }
    }
}

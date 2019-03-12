using System;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class ComboBoxControlGenerator : SeleniumGenerator<ComboBox>
    {
        private static readonly DotvvmProperty[] nameProperties = { Selector.SelectedValueProperty, SelectorBase.SelectionChangedProperty, ItemsControl.DataSourceProperty };
        public override DotvvmProperty[] NameProperties => nameProperties;

        public override bool CanUseControlContentForName => false;

        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            const string type = "DotVVM.Framework.Testing.SeleniumHelpers.Proxies.ComboBoxProxy";
            pageObject.Members.Add(GeneratePropertyForProxy(context, type));
            pageObject.ConstructorStatements.Add(GenerateInitializerForProxy(context, context.UniqueName, type));
        }
    }
}

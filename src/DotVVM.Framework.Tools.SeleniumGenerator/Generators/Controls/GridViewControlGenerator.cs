using System;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class GridViewControlGenerator : SeleniumGenerator<GridView>
    {
        private static readonly DotvvmProperty[] nameProperties = { ItemsControl.DataSourceProperty };

        public override DotvvmProperty[] NameProperties => nameProperties;

        public override bool CanUseControlContentForName => false;

        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            throw new NotImplementedException();
        }
    }
}

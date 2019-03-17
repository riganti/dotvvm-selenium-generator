using System;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls.GridViewControls
{
    public class GridViewTextColumnControlGenerator : SeleniumGenerator<GridViewTextColumn>
    {
        public override DotvvmProperty[] NameProperties { get; } = { GridViewTextColumn.ValueBindingProperty, GridViewColumn.HeaderTextProperty };
        public override bool CanUseControlContentForName => false;

        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            var type = $"{DefaultNamespace}.GridViewColumns.GridViewTextColumnProxy";
            AddPageObjectProperties(pageObject, context, type);
        }
    }
}

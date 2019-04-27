﻿using System;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing.SeleniumGenerator;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls.GridViewControls
{
    public class GridViewTextColumnControlGenerator : SeleniumGenerator<GridViewTextColumn>
    {
        public override DotvvmProperty[] NameProperties { get; } = { GridViewTextColumn.ValueBindingProperty, GridViewColumn.HeaderTextProperty };
        public override bool CanUseControlContentForName => false;

        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            const string type = "GridViewColumns.GridViewTextColumnProxy";
            AddPageObjectProperties(pageObject, context, type);
        }
    }
}

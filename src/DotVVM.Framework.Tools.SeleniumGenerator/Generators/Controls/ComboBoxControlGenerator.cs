﻿using System;
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
            var type = $"{DefaultNamespace}.ComboBoxProxy";
            AddPageObjectProperties(pageObject, context, type);
        }
    }
}

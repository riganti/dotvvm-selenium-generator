﻿using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class RadioButtonControlGenerator : SeleniumGenerator<RadioButton>
    {
        public override DotvvmProperty[] NameProperties { get; } = {CheckableControlBase.TextProperty, CheckableControlBase.CheckedValueProperty, RadioButton.CheckedProperty};
        public override bool CanUseControlContentForName => true;
        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            const string type = "DotVVM.Framework.Testing.SeleniumHelpers.Proxies.RadioButtonProxy";
            pageObject.Members.Add(GeneratePropertyForProxy(context, type));
            pageObject.ConstructorStatements.Add(GenerateInitializerForProxy(context, context.UniqueName, type));
        }
    }
}
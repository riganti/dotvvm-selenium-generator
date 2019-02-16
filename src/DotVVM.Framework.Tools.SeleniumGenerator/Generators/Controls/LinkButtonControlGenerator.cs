﻿using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class LinkButtonControlGenerator : SeleniumGenerator<LinkButton>
    {
        public override DotvvmProperty[] NameProperties { get; } = { ButtonBase.TextProperty, HtmlGenericControl.InnerTextProperty, ButtonBase.ClickProperty};
        public override bool CanUseControlContentForName => true;
        protected override void AddDeclarationsCore(HelperDefinition helper, SeleniumGeneratorContext context)
        {
            var type = "DotVVM.Framework.Testing.SeleniumHelpers.Proxies.LinkButtonProxy";
            helper.Members.Add(GeneratePropertyForProxy(context, type));
            helper.ConstructorStatements.Add(GenerateInitializerForProxy(context, context.UniqueName, type));
        }
    }
}
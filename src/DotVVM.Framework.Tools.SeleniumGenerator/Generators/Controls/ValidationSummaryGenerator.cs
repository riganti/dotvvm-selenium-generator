﻿using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class ValidationSummaryGenerator : SeleniumGenerator<ValidationSummary>
    {
        public override DotvvmProperty[] NameProperties { get; } = { DotvvmControl.IDProperty };
        public override bool CanUseControlContentForName { get; } = false;

        protected override void AddDeclarationsCore(HelperDefinition helper, SeleniumGeneratorContext context)
        {
            var type = "DotVVM.Framework.Testing.SeleniumHelpers.Proxies.ValidationSummaryProxy";
            helper.Members.Add(GeneratePropertyForProxy(context, type));
            helper.ConstructorStatements.Add(GenerateInitializerForProxy(context, context.UniqueName, type));
        }
    }
}

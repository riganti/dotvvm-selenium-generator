﻿using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class DataPagerControlGenerator : SeleniumGenerator<DataPager>
    {
        private static readonly DotvvmProperty[] nameProperties = { DataPager.DataSetProperty };
        public override DotvvmProperty[] NameProperties => nameProperties;

        public override bool CanUseControlContentForName => false;
        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            const string type = "DotVVM.Framework.Testing.SeleniumHelpers.Proxies.DataPagerProxy";
            pageObject.Members.Add(GeneratePropertyForProxy(context, type));
            pageObject.ConstructorStatements.Add(GenerateInitializerForProxy(context, context.UniqueName, type));
        }
    }
}
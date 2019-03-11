using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class ListBoxControlGenerator : SeleniumGenerator<ListBox>
    {
        public override DotvvmProperty[] NameProperties { get; }
        public override bool CanUseControlContentForName { get; }
        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            throw new NotImplementedException();
        }
    }
}

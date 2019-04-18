using DotVVM.Framework.Binding;
using DotVVM.Framework.Tools.SeleniumGenerator.Generators;
using SampleApp1.Controls;

namespace SampleApp1.SeleniumGenerators
{
    public class ControlBSeleniumGenerator : SeleniumGenerator<ControlB>
    {
        public override DotvvmProperty[] NameProperties { get; } = { };
        public override bool CanUseControlContentForName => false;
        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            const string type = "ControlBPageObject";
            AddPageObjectProperties(pageObject, context, type);
        }
    }
}

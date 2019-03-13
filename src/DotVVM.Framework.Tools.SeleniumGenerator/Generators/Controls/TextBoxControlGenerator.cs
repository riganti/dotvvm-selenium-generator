using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class TextBoxControlGenerator : SeleniumGenerator<TextBox>
    {
        private static readonly DotvvmProperty[] nameProperties = { TextBox.TextProperty, Validator.ValueProperty };

        public override DotvvmProperty[] NameProperties => nameProperties;

        public override bool CanUseControlContentForName => false;

        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            const string type = "DotVVM.Framework.Testing.SeleniumHelpers.Proxies.TextBoxProxy";
            pageObject.Members.Add(GeneratePropertyForProxy(context, type));
            pageObject.ConstructorStatements.Add(GenerateInitializerForProxy(context, context.UniqueName, type));
        }

    }
}

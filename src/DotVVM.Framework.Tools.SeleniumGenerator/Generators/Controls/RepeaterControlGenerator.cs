using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class RepeaterControlGenerator : SeleniumGenerator<Repeater>
    {
        private static readonly DotvvmProperty[] nameProperties = { ItemsControl.DataSourceProperty };

        public override DotvvmProperty[] NameProperties => nameProperties;

        public override bool CanUseControlContentForName => false;


        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            if (context.Control.TryGetProperty(Repeater.ItemTemplateProperty, out var itemTemplate))
            {
                var template = (ResolvedPropertyTemplate) itemTemplate;

                // generate child helper class
                var itemHelperName = context.UniqueName + "RepeaterHelper";
                context.Visitor.PushScope(new PageObjectDefinition { Name = itemHelperName });
                context.Visitor.VisitPropertyTemplate(template);
                pageObject.Children.Add(context.Visitor.PopScope());

                // generate property
                var type = "DotVVM.Framework.Testing.SeleniumHelpers.Proxies.RepeaterProxy";
                pageObject.Members.Add(GeneratePropertyForProxy(context, type, itemHelperName));
                pageObject.ConstructorStatements.Add(GenerateInitializerForProxy(context, context.UniqueName, type, itemHelperName));
            }
        }
    }
}

using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class GridViewControlGenerator : SeleniumGenerator<GridView>
    {
        private static readonly DotvvmProperty[] nameProperties = { ItemsControl.DataSourceProperty};

        public override DotvvmProperty[] NameProperties => nameProperties;

        public override bool CanUseControlContentForName => false;

        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            if (context.Control.TryGetProperty(GridView.ColumnsProperty, out var columnsTemplate))
            {
                var template = (ResolvedPropertyControlCollection) columnsTemplate;

                // generate child helper class
                var itemHelperName = context.UniqueName + "GridViewPageObject";
                context.Visitor.PushScope(new PageObjectDefinition { Name = itemHelperName });
                context.Visitor.VisitPropertyControlCollection(template);
                pageObject.Children.Add(context.Visitor.PopScope());

                // generate proxy
                const string type = "GridViewProxy";
                AddGenericPageObjectProperties(pageObject, context, type, itemHelperName);
            }
        }
    }
}

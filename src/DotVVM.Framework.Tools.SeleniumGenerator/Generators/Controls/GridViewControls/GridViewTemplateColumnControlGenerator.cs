﻿using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls.GridViewControls
{
    public class GridViewTemplateColumnControlGenerator : SeleniumGenerator<GridViewTemplateColumn>
    {
        public override DotvvmProperty[] NameProperties { get; } = { GridViewColumn.HeaderTextProperty };
        public override bool CanUseControlContentForName { get; } = true;
        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            if (context.Control.TryGetProperty(GridViewTemplateColumn.ContentTemplateProperty, out var contentTemplate))
            {
                var template = (ResolvedPropertyTemplate)contentTemplate;

                // generate child helper class
                var itemHelperName = context.UniqueName + "GridViewTemplateColumn";
                context.Visitor.PushScope(new PageObjectDefinition { Name = itemHelperName });
                context.Visitor.VisitPropertyTemplate(template);
                pageObject.Children.Add(context.Visitor.PopScope());

                // generate property
                var type = $"{DefaultNamespace}.GridViewColumns.GridViewTemplateColumnProxy";

                pageObject.Members.Add(GeneratePropertyForProxy(context.UniqueName, itemHelperName));
                pageObject.ConstructorStatements.Add(GenerateInitializerForTemplate(context.UniqueName, itemHelperName));
            }
        }
    }
}

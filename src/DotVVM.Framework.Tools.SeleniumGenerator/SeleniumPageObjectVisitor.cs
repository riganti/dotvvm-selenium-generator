﻿using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Tools.SeleniumGenerator.Configuration;
using DotVVM.Framework.Tools.SeleniumGenerator.Generators;
using DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls;
using DotVVM.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls.GridViewControls;

namespace DotVVM.Framework.Tools.SeleniumGenerator
{
    public class SeleniumPageObjectVisitor : ResolvedControlTreeVisitor
    {
        private Stack<PageObjectDefinition> HelperDefinitionsStack { get; } = new Stack<PageObjectDefinition>();

        private static readonly Dictionary<Type, ISeleniumGenerator> generators = new Dictionary<Type, ISeleniumGenerator>()
        {
            { typeof(TextBox), new TextBoxControlGenerator() },
            { typeof(CheckBox), new CheckBoxControlGenerator() },
            { typeof(Button), new ButtonControlGenerator() },
            { typeof(Literal), new LiteralControlGenerator() },
            { typeof(Repeater), new RepeaterControlGenerator() },
            { typeof(ValidationSummary), new ValidationSummaryControlGenerator() },
            { typeof(RadioButton), new RadioButtonControlGenerator() },
            { typeof(LinkButton), new LinkButtonControlGenerator() },
            { typeof(RouteLink), new RouteLinkControlGenerator() },
            { typeof(ComboBox), new ComboBoxControlGenerator() },
            { typeof(DataPager), new DataPagerControlGenerator() },
            { typeof(GridView), new GridViewControlGenerator() },
            { typeof(UpdateProgress), new UpdateProgressControlGenerator() },
            { typeof(FileUpload), new FileUploadControlGenerator() },
            { typeof(EmptyData), new EmptyDataControlGenerator() },
            { typeof(GridViewTextColumn), new GridViewTextColumnControlGenerator() },
            { typeof(GridViewCheckBoxColumn), new GridViewCheckBoxColumnControlGenerator() },
            { typeof(GridViewTemplateColumn), new GridViewTemplateColumnControlGenerator() },
            { typeof(HtmlGenericControl), new DotvvmControlGenerator() },
        };

        private Dictionary<Type, ISeleniumGenerator> DiscoverControlGenerators(SeleniumGeneratorOptions options)
        {
            return options.Assemblies
                 .SelectMany(a => a.GetLoadableTypes())
                 .Where(t => typeof(ISeleniumGenerator).IsAssignableFrom(t) && !t.IsAbstract)
                 .Select(t => (ISeleniumGenerator)Activator.CreateInstance(t))
                 .ToDictionary(t => t.ControlType, t => t);
        }


        public void PushScope(PageObjectDefinition definition)
        {
            HelperDefinitionsStack.Push(definition);
        }

        public PageObjectDefinition PopScope()
        {
            return HelperDefinitionsStack.Pop();
        }

        // TODO: if dotcontrol - use dotvvmControlGenerator
        public override void VisitControl(ResolvedControl control)
        {
            var helperDefinition = HelperDefinitionsStack.Peek();

            var dataContextNameSet = false;
            if (control.TryGetProperty(DotvvmBindableObject.DataContextProperty, out var property))
            {
                if (property is ResolvedPropertyBinding dataContextProperty)
                {
                    var dataContextName = dataContextProperty.Binding.Value;
                    helperDefinition.DataContextPrefixes.Add(dataContextName);
                    dataContextNameSet = true;
                }
            }

            if (generators.TryGetValue(control.Metadata.Type, out var generator))
            {
                // generate the content
                var context = new SeleniumGeneratorContext()
                {
                    Control = control,
                    UsedNames = helperDefinition.UsedNames,
                    Visitor = this,
                    Member = new MemberDefinition()
                };

                if (generator.CanAddDeclarations(helperDefinition, context))
                {
                    generator.AddDeclarations(helperDefinition, context);

                    helperDefinition.Members.Add(context.Member);

                    RemoveDataContextPrefix(dataContextNameSet, helperDefinition);
                    return;
                }
            }

            base.VisitControl(control);

            RemoveDataContextPrefix(dataContextNameSet, helperDefinition);
        }

        private void RemoveDataContextPrefix(bool dataContextNameSet, PageObjectDefinition helperDefinition)
        {
            if (dataContextNameSet)
            {
                helperDefinition.DataContextPrefixes.RemoveAt(helperDefinition.DataContextPrefixes.Count - 1);
            }
        }
    }
}

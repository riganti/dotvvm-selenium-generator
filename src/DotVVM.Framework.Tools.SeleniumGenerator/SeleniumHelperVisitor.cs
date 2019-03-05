using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Tools.SeleniumGenerator.Generators;
using DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator
{
    public class SeleniumHelperVisitor : ResolvedControlTreeVisitor
    {
        private Stack<HelperDefinition> HelperDefinitionsStack { get; } = new Stack<HelperDefinition>();

        private List<string> DataContextStack { get; set; } = new List<string>();

        private static Dictionary<Type, ISeleniumGenerator> generators = new Dictionary<Type, ISeleniumGenerator>()
        {
            { typeof(TextBox), new TextBoxGenerator() },
            { typeof(CheckBox), new CheckBoxGenerator() },
            { typeof(Button), new ButtonGenerator() },
            { typeof(Literal), new LiteralGenerator() },
            { typeof(Repeater), new RepeaterGenerator() },
            { typeof(ValidationSummary), new ValidationSummaryGenerator() },
            { typeof(RadioButton), new RadioButtonControlGenerator()},
            { typeof(LinkButton), new LinkButtonControlGenerator()},
            { typeof(RouteLink), new RouteLinkControlGenerator()},
        };


        public void PushScope(HelperDefinition definition)
        {
            HelperDefinitionsStack.Push(definition);
        }

        public HelperDefinition PopScope()
        {
            return HelperDefinitionsStack.Pop();
        }


        public override void VisitControl(ResolvedControl control)
        {
            var dataContextNameSet = false;
            if (control.TryGetProperty(DotvvmBindableObject.DataContextProperty, out var property))
            {
                if (property is ResolvedPropertyBinding dataContextProperty)
                {
                    var dataContextName = dataContextProperty.Binding.Value;
                    DataContextStack.Add(dataContextName);
                    dataContextNameSet = true;
                }
            }

            if (generators.TryGetValue(control.Metadata.Type, out var generator))
            {
                var helperDefinition = HelperDefinitionsStack.Peek();

                // generate the content
                var context = new SeleniumGeneratorContext() {
                    Control = control,
                    UsedNames = helperDefinition.UsedNames,
                    Visitor = this
                };

                if (generator.CanAddDeclarations(helperDefinition, context))
                {
                    generator.AddDeclarations(helperDefinition, context);
                    return;
                }
            }

            base.VisitControl(control);

            if (dataContextNameSet)
            {
                DataContextStack.RemoveAt(DataContextStack.Count - 1);
            }
        }
    }
}

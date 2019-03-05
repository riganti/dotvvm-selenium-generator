using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Tools.SeleniumGenerator.Configuration;
using DotVVM.Framework.Tools.SeleniumGenerator.Generators;
using DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls;
using DotVVM.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Tools.SeleniumGenerator
{
    public class SeleniumHelperVisitor : ResolvedControlTreeVisitor
    {
        private Stack<HelperDefinition> HelperDefinitionsStack { get; } = new Stack<HelperDefinition>();


        private static readonly Dictionary<Type, ISeleniumGenerator> Generators = new Dictionary<Type, ISeleniumGenerator>()
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

        private Dictionary<Type, ISeleniumGenerator> DiscoverControlGenerators(SeleniumGeneratorOptions options)
        {
            return options.Assemblies
                 .SelectMany(a => a.GetLoadableTypes())
                 .Where(t => typeof(ISeleniumGenerator).IsAssignableFrom(t) && !t.IsAbstract)
                 .Select(t => (ISeleniumGenerator)Activator.CreateInstance(t))
                 .ToDictionary(t => t.ControlType, t => t);
        }


        public void PushScope(HelperDefinition definition)
        {
            HelperDefinitionsStack.Push(definition);
        }

        public HelperDefinition PopScope()
        {
            return HelperDefinitionsStack.Pop();
        }


        // TODO: if dotcontrol - use dotvvmControlGenerator
        public override void VisitControl(ResolvedControl control)
        {
            // check if dataContext is set 
            // if yes push to DataContextPrefixes

            if (Generators.TryGetValue(control.Metadata.Type, out var generator))
            {
                var helperDefinition = HelperDefinitionsStack.Peek();

                // generate the content
                var context = new SeleniumGeneratorContext()
                {
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

            // pop from DataContextPrefixes if added
        }
    }
}

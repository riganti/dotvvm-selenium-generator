using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Tools.SeleniumGenerator.Configuration;
using DotVVM.Framework.Tools.SeleniumGenerator.Generators;
using DotVVM.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Tools.SeleniumGenerator.Extensions;
using DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator
{
    public class SeleniumPageObjectVisitor : ResolvedControlTreeVisitor
    {
        private Stack<PageObjectDefinition> HelperDefinitionsStack { get; } = new Stack<PageObjectDefinition>();

        private Dictionary<Type, ISeleniumGenerator> generators;

        public SeleniumPageObjectVisitor()
        {
            generators = new Dictionary<Type, ISeleniumGenerator>();
        }

        public void DiscoverControlGenerators(SeleniumGeneratorOptions options)
        {
            generators = GetControlGenerators(options);
        }

        private static Dictionary<Type, ISeleniumGenerator> GetControlGenerators(SeleniumGeneratorOptions options)
        {
            var customGenerators = options.CustomGenerators.ToDictionary(t => t.ControlType, t => t);

            var discoveredGenerators = options.Assemblies
                .SelectMany(a => a.GetLoadableTypes())
                .Where(t => typeof(ISeleniumGenerator).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(t => (ISeleniumGenerator)Activator.CreateInstance(t))
                .ToDictionary(t => t.ControlType, t => t);

            return customGenerators.AddRange(discoveredGenerators);
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
                    ExistingUsedSelectors = helperDefinition.ExistingUsedSelectors,
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
                helperDefinition.DataContextPrefixes.RemoveAt(helperDefinition.DataContextPrefixes.Count - 1);
            }
        }
    }
}

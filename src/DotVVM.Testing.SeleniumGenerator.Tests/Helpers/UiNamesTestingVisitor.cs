using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;

namespace DotVVM.Testing.SeleniumGenerator.Tests.Helpers
{
    public class UiNamesTestingVisitor : ResolvedControlTreeVisitor
    {
        private readonly List<(string, string)> controlSelectors = new List<(string, string)>();

        public override void VisitControl(ResolvedControl control)
        {
            var selector = TryGetNameFromProperty(control, UITests.NameProperty);
            if(selector != null) {

                controlSelectors.Add((control.Metadata.Name, selector));
            }

            base.VisitControl(control);
        }

        public List<(string, string)> GetResult()
        {
            return controlSelectors;
        }

        protected string TryGetNameFromProperty(ResolvedControl control, DotvvmProperty property)
        {
            if (control.TryGetProperty(property, out IAbstractPropertySetter setter))
            {
                switch (setter)
                {
                    case ResolvedPropertyValue propertySetter:
                        return propertySetter.Value?.ToString();

                    case ResolvedPropertyBinding propertyBinding:
                        return propertyBinding.Binding.Value;

                    default:
                        return "";
                }
            }

            return null;
        }
    }
}

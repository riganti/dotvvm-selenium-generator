using System.Collections.Generic;
using System.Reflection;
using DotVVM.Framework.Tools.SeleniumGenerator.Generators;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Configuration
{
    public class SeleniumGeneratorOptions
    {
        private readonly IList<Assembly> assemblies = new List<Assembly> { typeof(SeleniumGeneratorOptions).Assembly };
        private readonly IList<ISeleniumGenerator> customGenerators = new List<ISeleniumGenerator>();

        internal IEnumerable<Assembly> Assemblies => assemblies;
        internal IList<ISeleniumGenerator> CustomGenerators => customGenerators;

        public void AddAssembly(Assembly assembly) => assemblies.Add(assembly);

        public void AddCustomGenerator(ISeleniumGenerator generator) => customGenerators.Add(generator);
    }
}
using System.Collections.Generic;
using System.Reflection;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Configuration
{
    public class SeleniumGeneratorOptions
    {
        private readonly IList<Assembly> assemblies = new List<Assembly> { typeof(SeleniumGeneratorOptions).Assembly };

        internal IEnumerable<Assembly> Assemblies => assemblies;

        public void AddAssembly(Assembly assembly) => assemblies.Add(assembly);
    }
}
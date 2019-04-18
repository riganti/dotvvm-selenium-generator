using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotVVM.Testing.SeleniumGenerator.Tests.Helpers
{
    public static class RoslynHelpers
    {
        public static ITypeSymbol AssertPageObject(this Compilation compilation, string namespaceName, string className)
        {
            var pageObjectSymbol = compilation
                .GetSymbolsWithName(className)
                .OfType<ITypeSymbol>()
                .Single();

            Assert.AreEqual(namespaceName, pageObjectSymbol.ContainingSymbol.ToDisplayString());

            return pageObjectSymbol;
        }

        public static void AssertPublicProperty(this ITypeSymbol pageObject, Type propertyType, string propertyName)
        {
            var property = pageObject
                .GetMembers(propertyName)
                .OfType<IPropertySymbol>()
                .Single();

            Assert.AreEqual(propertyType.FullName, property.Type.ToDisplayString());
        }
    }
}

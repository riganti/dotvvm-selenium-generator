using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Tools.SeleniumGenerator.Generators;
using DotVVM.Framework.Binding;
using System.Collections.Generic;

namespace DotVVM.Testing.SeleniumGenerator.Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void SeleniumGenerator_TestRemovingNonIdentifierCharacters()
        {
            // initialize
            var myGenerator = new MySeleniumGeneratorTestClass();
            var testString = "__Ahoj1234@\'PTest Test";

            // do
            var resultString = myGenerator.RemoveNonIdentifierCharacters(testString);

            // assert
            var expectedString = "__Ahoj1234PTestTest";
            Assert.AreEqual(resultString, expectedString);
        }

        [TestMethod]
        public void SeleniumGenerator_TestAddDataContextPrefixesToName()
        {
            // initialize
            var myGenerator = new MySeleniumGeneratorTestClass();
            var dataContextPrefixes = new List<string> { "MyPage", "FirstTab", "Address" };
            var uniqueName = "City";

            // do
            var uniqueNameWithDataContextPrefixes = myGenerator.AddDataContextPrefixesToName(dataContextPrefixes, uniqueName);

            // assert
            var expectedResult = "MyPage_FirstTab_Address_City";
            Assert.AreEqual(expectedResult, uniqueNameWithDataContextPrefixes);
        }

        [TestMethod]
        public void SeleniumGenerator_TestNormalizeUniqueName()
        {
            // initialize
            var myGenerator = new MySeleniumGeneratorTestClass();
            var testString = "My superb String";

            // do
            var resultString = myGenerator.RemoveNonIdentifierCharacters(testString);

            // assert
            var expectedString = "MySuperbString";
            Assert.AreEqual(resultString, expectedString);
        }

        [TestMethod]
        public void SeleniumGenerator_()
        {

        }
    }


    // new class because of SeleniumGenerator is abstract class
    public class MySeleniumGeneratorTestClass : SeleniumGenerator<HtmlGenericControl>
    {
        public override DotvvmProperty[] NameProperties => throw new System.NotImplementedException();

        public override bool CanUseControlContentForName => throw new System.NotImplementedException();

        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}

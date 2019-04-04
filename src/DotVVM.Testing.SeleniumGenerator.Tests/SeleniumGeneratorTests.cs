using System.IO;
using System.Threading.Tasks;
using DotVVM.Framework.Testing.SeleniumHelpers.Proxies;
using DotVVM.Testing.SeleniumGenerator.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Testing.SeleniumGenerator.Tests
{
    [TestClass]
    public class SampleApp1Tests
    {
        private readonly string solutionDirectory;
        private readonly string webAppDirectory;
        private readonly string proxiesCsProjPath;

        public TestContext TestContext { get; set; }

        public SampleApp1Tests()
        {
            solutionDirectory = TestEnvironmentHelper.FindSolutionDirectory();
            webAppDirectory = Path.Combine(solutionDirectory, "Samples\\SampleApp1");
            proxiesCsProjPath = Path.Combine(solutionDirectory, "DotVVM.Framework.Testing.SeleniumHelpers\\DotVVM.Framework.Testing.SeleniumHelpers.csproj");
        }


        [TestMethod]
        public async Task SimplePage_CheckGeneratedProperties()
        {
            using (var workspace = new WebApplicationHost(TestContext, webAppDirectory))
            {
                workspace.ProcessMarkupFile("Views/SimplePage/Page.dothtml");

                // compile project
                workspace.FixReferencedProjectPath(proxiesCsProjPath);
                var compilation = await workspace.CompileAsync();

                // verify the class
                var pageObject = compilation.AssertPageObject("SampleApp1.Tests.PageObjects.SimplePage", "PagePageObject");
                pageObject.AssertPublicProperty(typeof(RadioButtonProxy), "Person");
                pageObject.AssertPublicProperty(typeof(RadioButtonProxy), "Company");
                pageObject.AssertPublicProperty(typeof(TextBoxProxy), "Name_FirstName");
                pageObject.AssertPublicProperty(typeof(TextBoxProxy), "Name_LastName");
                pageObject.AssertPublicProperty(typeof(ButtonProxy), "Click");
                pageObject.AssertPublicProperty(typeof(TextBoxProxy), "Address");
                pageObject.AssertPublicProperty(typeof(ComboBoxProxy), "CountryCode");
                pageObject.AssertPublicProperty(typeof(CheckBoxProxy), "IsEuVatPayer");
                pageObject.AssertPublicProperty(typeof(ButtonProxy), "CreateCompany");
                pageObject.AssertPublicProperty(typeof(LinkButtonProxy), "ResetForm");
                pageObject.AssertPublicProperty(typeof(LiteralProxy), "StatusMessage");
                pageObject.AssertPublicProperty(typeof(ValidationSummaryProxy), "ValidationSummary");
            }
        }

        [TestMethod]
        public async Task SimplePage_CheckGeneratedUiNames()
        {
            using (var workspace = new WebApplicationHost(TestContext, webAppDirectory))
            {
                workspace.ProcessMarkupFile("Views/SimplePage/Page.dothtml");

                // compile project
                workspace.FixReferencedProjectPath(proxiesCsProjPath);
                var compilation = await workspace.CompileAsync();

                // verify the class
                var pageObject = compilation.AssertPageObject("SampleApp1.Tests.PageObjects.SimplePage", "PagePageObject");
            }
        }
    }
}

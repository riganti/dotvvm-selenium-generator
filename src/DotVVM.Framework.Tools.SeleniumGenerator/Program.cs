using DotVVM.CommandLine.Core;
using DotVVM.CommandLine.Core.Metadata;
using DotVVM.CommandLine.Core.Templates;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Security;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.IO;

namespace DotVVM.Framework.Tools.SeleniumGenerator
{
    public class Program
    {
        private static readonly string PageObjectsText;

        public static void Main(string[] args)
        {
            var dotvvmProjectMetadata = new DotvvmProjectMetadata();

            // make sure the test directory exists
            if (string.IsNullOrEmpty(dotvvmProjectMetadata.UITestProjectPath))
            {
                var hintProjectName = $"..\\{dotvvmProjectMetadata.ProjectName}.Tests";
                dotvvmProjectMetadata.UITestProjectPath = ConsoleHelpers.AskForValue($"Enter the path to the test project\n(relative to DotVVM project directory, e.g. '{hintProjectName}'): ", hintProjectName);
            }

            var testProjectDirectory = dotvvmProjectMetadata.GetUITestProjectFullPath();
            if (!Directory.Exists(testProjectDirectory))
            {
                GenerateTestProject(testProjectDirectory);
            }

            // make sure we know the test project namespace
            if (string.IsNullOrEmpty(dotvvmProjectMetadata.UITestProjectRootNamespace))
            {
                dotvvmProjectMetadata.UITestProjectRootNamespace = Path.GetFileName(testProjectDirectory);
            }

            // generate the test stubs
            var name = args[0];
            var files = ExpandFileNames(name);

            foreach (var file in files)
            {
                Console.WriteLine($"Generating stub for {file}...");

                // determine full type name and target file
                var relativePath = PathHelpers.GetDothtmlFileRelativePath(dotvvmProjectMetadata, file);
                var relativeTypeName = $"{PathHelpers.TrimFileExtension(relativePath)}PageObject";
                var fullTypeName = $"{dotvvmProjectMetadata.UITestProjectRootNamespace}.{PageObjectsText}.{PathHelpers.CreateTypeNameFromPath(relativeTypeName)}";
                var targetFileName = Path.Combine(dotvvmProjectMetadata.UITestProjectPath, PageObjectsText, relativeTypeName + ".cs");

                //generate the file
                var generator = new SeleniumPageObjectGenerator();
                var config = new SeleniumGeneratorConfiguration()
                {
                    TargetNamespace = PathHelpers.GetNamespaceFromFullType(fullTypeName),
                    PageObjectName = PathHelpers.GetTypeNameFromFullType(fullTypeName),
                    PageObjectFileFullPath = targetFileName,
                    ViewFullPath = file
                };

                generator.ProcessMarkupFile(
                    DotvvmConfiguration.CreateDefault(services
                        => services.TryAddSingleton<IViewModelProtector, FakeViewModelProtector>()),
                    config);
            }
        }

        private static void GenerateTestProject(string projectDirectory)
        {
            var projectFileName = Path.GetFileName(projectDirectory);
            var testProjectPath = Path.Combine(projectDirectory, projectFileName + ".csproj");
            var fileContent = GetProjectFileTextContent();

            FileSystemHelpers.WriteFile(testProjectPath, fileContent);

            CreatePageObjectsDirectory(projectDirectory);
        }

        private static void CreatePageObjectsDirectory(string projectDirectory)
        {
            var objectsDirectory = Path.Combine(projectDirectory, PageObjectsText);
            if (!Directory.Exists(objectsDirectory))
            {
                Directory.CreateDirectory(objectsDirectory);
            }
        }

        private static string GetProjectFileTextContent()
        {
            var projectTemplate = new TestProjectTemplate();

            return projectTemplate.TransformText();
        }

        protected static string[] ExpandFileNames(string name)
        {
            // TODO: add wildcard support
            return new[] { Path.GetFullPath(name) };
        }
    }
}

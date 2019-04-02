using DotVVM.Framework.Configuration;
using DotVVM.Framework.Security;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.IO;
using System.Linq;
using DotVVM.CommandLine.Core;
using DotVVM.CommandLine.Core.Arguments;
using DotVVM.CommandLine.Core.Metadata;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DotVVM.Framework.Tools.SeleniumGenerator
{
    public class Program
    {
        private const string PageObjectsText = "PageObjects";

        public static void Main(string[] args)
        {
            var arguments = new Arguments(args);

            DotvvmProjectMetadata dotvvmProjectMetadata = null;
            if (string.Equals(arguments[0], "--json", StringComparison.CurrentCultureIgnoreCase))
            {

                dotvvmProjectMetadata = JsonConvert.DeserializeObject<DotvvmProjectMetadata>(args[1]);
                arguments.Consume(2);
            }
            else
            {
                Console.WriteLine(@"Provide correct metadata.");
            }

            IEnumerable<string> controlFiles = new List<string>();
            IEnumerable<string> viewFiles;

            if (arguments[0] != null)
            {
                viewFiles = GetViewsFiles(arguments);
            }
            else
            {
                // generate all views and user controls files if no argument was specified
                controlFiles = GetUserControlFiles(dotvvmProjectMetadata);
                viewFiles = GetViewFiles(dotvvmProjectMetadata);
            }

            // generate the test stubs
            GeneratePageObjects(dotvvmProjectMetadata, controlFiles, viewFiles);
        }

        private static void GeneratePageObjects(DotvvmProjectMetadata dotvvmProjectMetadata,
            IEnumerable<string> controlFiles,
            IEnumerable<string> viewFiles)
        {
            var dotvvmConfig = DotvvmConfiguration.CreateDefault(services => services.TryAddSingleton<IViewModelProtector, FakeViewModelProtector>());
            var generator = new SeleniumPageObjectGenerator();

            var allFiles = controlFiles.Concat(viewFiles);

            foreach (var file in allFiles)
            {
                if (File.Exists(file))
                {
                    Console.WriteLine($"Generating stub for {file}...");

                    // determine full type name and target file
                    var relativePath = PathHelpers.GetDothtmlFileRelativePath(dotvvmProjectMetadata, file);
                    var relativeTypeName = $"{PathHelpers.TrimFileExtension(relativePath)}PageObject";
                    var fullTypeName = $"{dotvvmProjectMetadata.UITestProjectRootNamespace}.{PageObjectsText}.{PathHelpers.CreateTypeNameFromPath(relativeTypeName)}";
                    var targetFileName = Path.Combine(dotvvmProjectMetadata.UITestProjectPath, PageObjectsText, relativeTypeName + ".cs");

                    var config = GetSeleniumGeneratorConfiguration(fullTypeName, targetFileName, file);

                    GeneratePageObject(generator, dotvvmConfig, config);
                }
            }
        }

        private static void GeneratePageObject(SeleniumPageObjectGenerator generator, DotvvmConfiguration dotvvmConfig, SeleniumGeneratorConfiguration config)
           => generator.ProcessMarkupFile(dotvvmConfig, config);

        private static IEnumerable<string> GetUserControlFiles(DotvvmProjectMetadata dotvvmProjectMetadata)
            => Directory.GetFiles(dotvvmProjectMetadata.ProjectDirectory, "*.dotcontrol", SearchOption.AllDirectories);

        private static IEnumerable<string> GetViewFiles(DotvvmProjectMetadata dotvvmProjectMetadata)
            => Directory.GetFiles(dotvvmProjectMetadata.ProjectDirectory, "*.dothtml", SearchOption.AllDirectories);

        private static IEnumerable<string> GetViewsFiles(Arguments args)
            => ExpandFileNames(args[0]);

        private static SeleniumGeneratorConfiguration GetSeleniumGeneratorConfiguration(string fullTypeName,
            string targetFileName, string file)
        {
            return new SeleniumGeneratorConfiguration()
            {
                TargetNamespace = PathHelpers.GetNamespaceFromFullType(fullTypeName),
                PageObjectName = PathHelpers.GetTypeNameFromFullType(fullTypeName),
                PageObjectFileFullPath = targetFileName,
                ViewFullPath = file
            };
        }

        protected static string[] ExpandFileNames(string name)
        {
            // TODO: add wildcard support
            return new[] { Path.GetFullPath(name) };
        }
    }
}

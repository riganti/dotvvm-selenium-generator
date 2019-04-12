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
using DotVVM.Utils.ConfigurationHost.Initialization;
using System.Reflection;

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

            var config = ConfigurationHost.InitDotVVM(Assembly.LoadFile(dotvvmProjectMetadata.WebAssemblyPath),
                dotvvmProjectMetadata.ProjectDirectory,
                services => services.TryAddSingleton<IViewModelProtector, FakeViewModelProtector>());

            IEnumerable<string> controlFiles = new List<string>();
            IEnumerable<string> viewFiles;

            if (arguments[0] != null)
            {
                viewFiles = GetViewsFiles(new[] { arguments[0] });
            }
            else
            {
                // generate all views and user controls files if no argument was specified
                viewFiles = config.RouteTable.Where(b => b.VirtualPath != null).Select(r => r.VirtualPath);
                controlFiles = config.Markup.Controls.Where(b => b.Src != null).Select(c => c.Src);
            }

            // generate the test stubs
            GeneratePageObjects(dotvvmProjectMetadata, controlFiles, viewFiles, config);

            Console.WriteLine(@"#$ Exit 0 - DotVVM Selenium Generator Ended");
            Environment.Exit(0);
        }

        private static void GeneratePageObjects(DotvvmProjectMetadata dotvvmProjectMetadata,
            IEnumerable<string> controlFiles,
            IEnumerable<string> viewFiles,
            DotvvmConfiguration dotvvmConfig)
        {
            var generator = new SeleniumPageObjectGenerator();

            var allFiles = controlFiles.Concat(viewFiles).Distinct();

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

        private static IEnumerable<string> GetViewsFiles(IEnumerable<string> filePaths)
        {
            return filePaths.Select(file => ExpandFileName(file));
        }

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

        protected static string ExpandFileName(string name)
        {
            // TODO: add wildcard support
            return Path.GetFullPath(name);
        }
    }
}

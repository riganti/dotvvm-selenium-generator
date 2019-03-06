﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buildalyzer;
using Buildalyzer.Workspaces;
using DotVVM.CommandLine.Commands;
using DotVVM.CommandLine.Commands.Implementation;
using DotVVM.CommandLine.Core.Metadata;
using DotVVM.CommandLine.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Testing.SeleniumGenerator.Tests.Helpers
{
    public class WebApplicationHost : IDisposable
    {
        private readonly TestContext testContext;
        private readonly string webApplicationTemplatePath;
        private bool initialized;
        private string workingDirectory;
        private string webAppName;
        private string webAppDirectory;
        private string testProjectName;
        private string testProjectCsproj;
        private string dotvvmJsonPath;
        private DotvvmProjectMetadata metadata;

        public string TestProjectDirectory { get; private set; }


        public WebApplicationHost(TestContext testContext, string webApplicationTemplatePath)
        {
            this.testContext = testContext;
            this.webApplicationTemplatePath = webApplicationTemplatePath;

            workingDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            webAppName = Path.GetFileName(webApplicationTemplatePath);
            webAppDirectory = Path.Combine(workingDirectory, webAppName);
            dotvvmJsonPath = Path.Combine(webAppDirectory, ".dotvvm.json");

            testProjectName = webAppName + ".Tests";
            TestProjectDirectory = Path.GetFullPath(Path.Combine(webAppDirectory, "..", testProjectName));
            testProjectCsproj = Path.Combine(TestProjectDirectory, testProjectName + ".csproj");
        }

        public void Initialize()
        {
            initialized = true;

            // prepare temp directories
            Directory.CreateDirectory(workingDirectory);
            Directory.CreateDirectory(webAppDirectory);

            // copy application in the working directory
            Process.Start("xcopy", $"/E \"{webApplicationTemplatePath}\" \"{webAppDirectory}\"").WaitForExit();

            // set test project path in .dotvvm.json
            var metadataService = new DotvvmProjectMetadataService();
            metadata = metadataService.LoadFromFile(dotvvmJsonPath);
            metadata.UITestProjectPath = $"../{testProjectName}";
            metadata.UITestProjectRootNamespace = testProjectName;

            // change current directory
            Environment.CurrentDirectory = webAppDirectory;
        }

        public void ProcessMarkupFile(string markupFilePath)
        {
            if (!initialized)
            {
                Initialize();
            }

            // process markup file
            var command = new GenerateUiTestStubCommand();
            command.Handle(new Arguments(new[] { markupFilePath }), metadata);
        }

        internal void FixReferencedProjectPath(string proxiesCsProjPath)
        {
            // TODO: remove this when we replace the proxies with NuGet package
            var csproj = File.ReadAllText(testProjectCsproj);
            csproj = csproj.Replace("..\\DotVVM.Framework.Testing.SeleniumHelpers.csproj", proxiesCsProjPath);
            File.WriteAllText(testProjectCsproj, csproj);
        }

        public async Task<Compilation> CompileAsync()
        {
            var manager = new AnalyzerManager();
            var project = manager.GetProject(testProjectCsproj);

            var workspace = new AdhocWorkspace();
            var roslynProject = project.AddToWorkspace(workspace);
            var compilation = await roslynProject.GetCompilationAsync();

            var diagnostics = compilation.GetDiagnostics();
            foreach (var diagnostic in diagnostics)
            {
                testContext.WriteLine(diagnostic.GetMessage());
            }

            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                Assert.Fail("Test project build failed!");
            }

            return compilation;
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(this.workingDirectory, true);
            }
            catch (IOException)
            {
            }
        }

    }
}
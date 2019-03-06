using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DotVVM.CommandLine.Core;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Tools.SeleniumGenerator.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Tools.SeleniumGenerator
{
    public class SeleniumPageObjectGenerator
    {
        public void ProcessMarkupFile(DotvvmConfiguration dotvvmConfiguration, SeleniumGeneratorConfiguration seleniumConfiguration)
        {
            // resolve control tree
            var viewTree = ResolveControlTree(seleniumConfiguration.ViewFullPath, dotvvmConfiguration);

            // TODO: resolve unique names in master pages
            var masterPageObjectDefinitions = ResolveMasterPages(dotvvmConfiguration, seleniumConfiguration, viewTree);

            var masterUsedUniqueNames = masterPageObjectDefinitions.SelectMany(m => m.UsedNames).ToHashSet();
            // var helper
            var helper = CreatePageObjectDefinition(seleniumConfiguration, viewTree, masterUsedUniqueNames);
            helper = CombineViewHelperDefinitions(helper, masterPageObjectDefinitions);

            // generate the class
            GenerateHelperClass(seleniumConfiguration, helper);

            // update view markup file
            UpdateMarkupFile(helper, seleniumConfiguration.ViewFullPath);
        }

        private PageObjectDefinition CombineViewHelperDefinitions(PageObjectDefinition pageObject,
            ICollection<MasterPageObjectDefinition> masterPageObjects)
        {
            if (masterPageObjects.Any())
            {
                var masterMembers = masterPageObjects.SelectMany(m => m.Members);
                var constructorExpressions = masterPageObjects.SelectMany(m => m.ConstructorStatements);

                pageObject.Members.AddRange(masterMembers);
                pageObject.ConstructorStatements.AddRange(constructorExpressions);
            }

            return pageObject;
        }

        private List<MasterPageObjectDefinition> ResolveMasterPages(
            DotvvmConfiguration dotvvmConfiguration,
            SeleniumGeneratorConfiguration seleniumConfiguration,
            IAbstractTreeRoot viewTree)
        {
            var pageObjectDefinitions = new List<MasterPageObjectDefinition>();
            CreateMasterPageObjectDefinitions(dotvvmConfiguration, seleniumConfiguration, viewTree, pageObjectDefinitions);

            foreach (var pageObjectDefinition in pageObjectDefinitions)
            {
                UpdateMarkupFile(pageObjectDefinition, pageObjectDefinition.MasterPageFullPath);
            }

            return pageObjectDefinitions;
        }

        private void CreateMasterPageObjectDefinitions(DotvvmConfiguration dotvvmConfiguration,
            SeleniumGeneratorConfiguration seleniumConfiguration,
            IAbstractTreeRoot viewTree,
            ICollection<MasterPageObjectDefinition> pageObjectDefinitions)
        {
            if (IsNestedInMasterPage(viewTree))
            {
                var masterPageFile = viewTree.Directives[ParserConstants.MasterPageDirective].FirstOrDefault();
                if (masterPageFile != null)
                {
                    var masterTree = ResolveControlTree(masterPageFile.Value, dotvvmConfiguration);
                    var masterPageObjectDefinition = GetMasterPageObjectDefinition(seleniumConfiguration, masterTree, masterPageFile);

                    pageObjectDefinitions.Add(masterPageObjectDefinition);

                    // recursion
                    CreateMasterPageObjectDefinitions(dotvvmConfiguration, seleniumConfiguration, masterTree, pageObjectDefinitions);
                }
            }
        }

        private MasterPageObjectDefinition GetMasterPageObjectDefinition(
            SeleniumGeneratorConfiguration seleniumConfiguration,
            IAbstractTreeRoot masterTree,
            IAbstractDirective masterPageFile)
        {
            var definition = CreatePageObjectDefinition(seleniumConfiguration, masterTree);
            return MapPageObjectDefinition(definition, masterPageFile);
        }

        private MasterPageObjectDefinition MapPageObjectDefinition(PageObjectDefinition definition, IAbstractDirective masterPageFile)
        {
            var masterDefinition = new MasterPageObjectDefinition();
            masterDefinition.Members.AddRange(definition.Members);
            masterDefinition.MarkupFileModifications.AddRange(definition.MarkupFileModifications);
            masterDefinition.ConstructorStatements.AddRange(definition.ConstructorStatements);
            masterDefinition.DataContextPrefixes.AddRange(definition.DataContextPrefixes);
            masterDefinition.Children.AddRange(definition.Children);
            masterDefinition.UsedNames.UnionWith(definition.UsedNames);
            masterDefinition.MasterPageFullPath = masterPageFile.Value;

            return masterDefinition;
        }

        private bool IsNestedInMasterPage(IAbstractTreeRoot view)
        {
            return view.Directives.ContainsKey(ParserConstants.MasterPageDirective);
        }

        private void UpdateMarkupFile(PageObjectDefinition pageObject, string viewPath)
        {
            var sb = new StringBuilder(File.ReadAllText(viewPath, Encoding.UTF8));
            foreach (var modification in pageObject.MarkupFileModifications.OrderByDescending(m => m.Position))
            {
                modification.Apply(sb);
            }

            File.WriteAllText(viewPath, sb.ToString(), Encoding.UTF8);
        }

        private void GenerateHelperClass(SeleniumGeneratorConfiguration seleniumConfiguration, PageObjectDefinition pageObject)
        {
            var tree = CSharpSyntaxTree.Create(
                SyntaxFactory.CompilationUnit().WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                    {
                        SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(seleniumConfiguration.TargetNamespace))
                            .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                            {
                                GenerateHelperClassContents(pageObject)
                            }))
                    }))
                    .NormalizeWhitespace()
            );

            FileSystemHelpers.WriteFile(seleniumConfiguration.HelperFileFullPath, tree.ToString(), false);
        }

        private static PageObjectDefinition CreatePageObjectDefinition(
            SeleniumGeneratorConfiguration seleniumConfiguration, IAbstractTreeRoot tree,
            HashSet<string> masterUsedUniqueNames = null)
        {
            var pageObjectDefinition = GetPageObjectDefinition(seleniumConfiguration, masterUsedUniqueNames);

            // traverse the tree
            var visitor = new SeleniumPageObjectVisitor();
            visitor.PushScope(pageObjectDefinition);
            visitor.VisitView((ResolvedTreeRoot)tree);
            return visitor.PopScope();
        }

        private static PageObjectDefinition GetPageObjectDefinition(SeleniumGeneratorConfiguration seleniumConfiguration,
            HashSet<string> masterUsedUniqueNames)
        {
            var pageObjectDefinition = new PageObjectDefinition { Name = seleniumConfiguration.HelperName };
            if (masterUsedUniqueNames != null)
            {
                pageObjectDefinition.UsedNames.UnionWith(masterUsedUniqueNames);
            }

            return pageObjectDefinition;
        }

        private MemberDeclarationSyntax GenerateHelperClassContents(PageObjectDefinition pageObjectDefinition)
        {
            return SyntaxFactory
                .ClassDeclaration(pageObjectDefinition.Name)
                .WithModifiers(GetClassModifiers())
                .WithBaseList(GetBaseTypeDeclaration())
                .WithMembers(SyntaxFactory.List(pageObjectDefinition.Members))
                .AddMembers(GetConstructor(pageObjectDefinition))
                .AddMembers(pageObjectDefinition.Children.Select(GenerateHelperClassContents).ToArray());
        }

        private static ConstructorDeclarationSyntax GetConstructor(PageObjectDefinition pageObjectDefinition)
        {
            return SyntaxFactory
                .ConstructorDeclaration(pageObjectDefinition.Name)
                .WithParameterList(GetConstructorMembers())
                .WithInitializer(GetBaseConstructorParameters())
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBody(SyntaxFactory.Block(pageObjectDefinition.ConstructorStatements));
        }

        private static ParameterListSyntax GetConstructorMembers()
        {
            return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(new[]
            {
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("webDriver"))
                             .WithType(SyntaxFactory.ParseTypeName("OpenQA.Selenium.IWebDriver")),
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("parentHelper"))
                             .WithType(SyntaxFactory.ParseTypeName("DotVVM.Framework.Testing.SeleniumHelpers.SeleniumHelperBase"))
                             .WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.IdentifierName("null"))),
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("selectorPrefix"))
                             .WithType(SyntaxFactory.ParseTypeName("System.String"))
                             .WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(""))))
            }));
        }

        private static ConstructorInitializerSyntax GetBaseConstructorParameters()
        {
            return SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[]
                                {
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("webDriver")),
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("parentHelper")),
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("selectorPrefix"))
                    })));
        }

        private static SyntaxTokenList GetClassModifiers()
        {
            return SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
        }

        private static BaseListSyntax GetBaseTypeDeclaration()
        {
            return SyntaxFactory.BaseList(SyntaxFactory.SeparatedList<BaseTypeSyntax>(new[]
            {
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("DotVVM.Framework.Testing.SeleniumHelpers.SeleniumHelperBase"))
            }));
        }

        private IAbstractTreeRoot ResolveControlTree(string filePath, DotvvmConfiguration dotvvmConfiguration)
        {
            var fileContent = File.ReadAllText(filePath, Encoding.UTF8);

            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(fileContent);

            var parser = new DothtmlParser();
            var rootNode = parser.Parse(tokenizer.Tokens);

            var treeResolver = dotvvmConfiguration.ServiceProvider.GetService<IControlTreeResolver>();
            return treeResolver.ResolveTree(rootNode, filePath);
        }
    }
}

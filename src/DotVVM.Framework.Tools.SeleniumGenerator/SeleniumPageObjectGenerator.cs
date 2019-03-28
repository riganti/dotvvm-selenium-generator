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

            var usedSelectors = GetUsedSelectors(viewTree);

            // resolve master pages of current page
            var masterPageObjectDefinitions = ResolveMasterPages(dotvvmConfiguration, seleniumConfiguration, viewTree);

            var allUsedNames = UnionUsedUniqueNames(masterPageObjectDefinitions, usedSelectors);

            var pageObject = CreatePageObjectDefinition(seleniumConfiguration, viewTree, allUsedNames);

            // combine all master page objects with current page object so we can generate page object class for all proxies
            pageObject = CombineViewHelperDefinitions(pageObject, masterPageObjectDefinitions);

            // generate the class
            GeneratePageObjectClass(seleniumConfiguration, pageObject);

            // update view markup file
            UpdateMarkupFile(pageObject, seleniumConfiguration.ViewFullPath);
        }

        private HashSet<string> UnionUsedUniqueNames(
            IEnumerable<MasterPageObjectDefinition> masterPageObjectDefinitions, 
            IEnumerable<string> usedSelectors)
        {
            var masterPagesUsedNames = masterPageObjectDefinitions.SelectMany(m => m.UsedNames);
            return masterPagesUsedNames.Union(usedSelectors).ToHashSet();
        }

        private HashSet<string> GetUsedSelectors(IAbstractTreeRoot viewTree)
        {
            // traverse the tree
            var visitor = new SeleniumSelectorsFinderVisitor();
            visitor.VisitView((ResolvedTreeRoot)viewTree);
            return visitor.GetResult();
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
                    var usedSelectors = GetUsedSelectors(masterTree);

                    var masterPageObjectDefinition = GetMasterPageObjectDefinition(seleniumConfiguration, masterTree, masterPageFile, usedSelectors);

                    pageObjectDefinitions.Add(masterPageObjectDefinition);

                    // recursion
                    CreateMasterPageObjectDefinitions(dotvvmConfiguration, seleniumConfiguration, masterTree, pageObjectDefinitions);
                }
            }
        }

        private MasterPageObjectDefinition GetMasterPageObjectDefinition(
            SeleniumGeneratorConfiguration seleniumConfiguration,
            IAbstractTreeRoot masterTree,
            IAbstractDirective masterPageFile, 
            HashSet<string> usedSelectors)
        {
            var definition = CreatePageObjectDefinition(seleniumConfiguration, masterTree, usedSelectors);
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

            UpdateMarkupFile(pageObject, sb);

            File.WriteAllText(viewPath, sb.ToString(), Encoding.UTF8);
        }

        private void UpdateMarkupFile(PageObjectDefinition pageObject, StringBuilder stringBuilder)
        {
            var allModifications = GetAllModifications(pageObject);

            foreach (var modification in allModifications.OrderByDescending(m => m.Position))
            {
                modification.Apply(stringBuilder);
            }
        }

        private IEnumerable<MarkupFileModification> GetAllModifications(PageObjectDefinition pageObject)
        {
            var modifications = pageObject.MarkupFileModifications;
            foreach (var child in pageObject.Children)
            {
                modifications.AddRange(GetAllModifications(child));
            }

            return modifications;
        }

        private void GeneratePageObjectClass(SeleniumGeneratorConfiguration seleniumConfiguration, PageObjectDefinition pageObject)
        {
            var tree = CSharpSyntaxTree.Create(
                SyntaxFactory.CompilationUnit()
                    .WithUsings(SyntaxFactory.SingletonList(
                        SyntaxFactory.UsingDirective(
                            SyntaxFactory.QualifiedName(
                                SyntaxFactory.QualifiedName(
                                    SyntaxFactory.QualifiedName(
                                        SyntaxFactory.IdentifierName("DotVVM"),
                                        SyntaxFactory.IdentifierName("Framework")),
                                    SyntaxFactory.IdentifierName("Testing")),
                                SyntaxFactory.IdentifierName("SeleniumHelpers")))))
                    .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                    {
                        SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(seleniumConfiguration.TargetNamespace))
                            .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                            {
                                GenerateHelperClassContents(pageObject)
                            }))
                    }))
                    .NormalizeWhitespace()
            );

            FileSystemHelpers.WriteFile(seleniumConfiguration.PageObjectFileFullPath, tree.ToString(), false);
        }

        private PageObjectDefinition CreatePageObjectDefinition(
            SeleniumGeneratorConfiguration seleniumConfiguration, IAbstractTreeRoot tree,
            HashSet<string> masterUsedUniqueSelectors = null)
        {
            var pageObjectDefinition = GetPageObjectDefinition(seleniumConfiguration, masterUsedUniqueSelectors);

            // traverse the tree
            var visitor = new SeleniumPageObjectVisitor();
            visitor.PushScope(pageObjectDefinition);
            visitor.VisitView((ResolvedTreeRoot)tree);
            return visitor.PopScope();
        }

        private PageObjectDefinition GetPageObjectDefinition(SeleniumGeneratorConfiguration seleniumConfiguration,
            HashSet<string> masterUsedUniqueSelectors)
        {
            var pageObjectDefinition = new PageObjectDefinition { Name = seleniumConfiguration.PageObjectName };
            if (masterUsedUniqueSelectors != null)
            {
                pageObjectDefinition.ExistingUsedSelectors.UnionWith(masterUsedUniqueSelectors);
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

        private ConstructorDeclarationSyntax GetConstructor(PageObjectDefinition pageObjectDefinition)
        {
            return SyntaxFactory
                .ConstructorDeclaration(pageObjectDefinition.Name)
                .WithParameterList(GetConstructorMembers())
                .WithInitializer(GetBaseConstructorParameters())
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBody(SyntaxFactory.Block(pageObjectDefinition.ConstructorStatements));
        }

        private ParameterListSyntax GetConstructorMembers()
        {
            return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(new[]
            {
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("webDriver"))
                             .WithType(SyntaxFactory.ParseTypeName("OpenQA.Selenium.IWebDriver")),
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("parentHelper"))
                             .WithType(SyntaxFactory.ParseTypeName("DotVVM.Framework.Testing.SeleniumHelpers.SeleniumHelperBase"))
                             .WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.IdentifierName("null"))),
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("parentSelector"))
                             .WithType(SyntaxFactory.ParseTypeName("DotVVM.Framework.Testing.SeleniumHelpers.PathSelector"))
                             .WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.IdentifierName("null")))
            }));
        }

        private ConstructorInitializerSyntax GetBaseConstructorParameters()
        {
            return SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[]
                                {
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("webDriver")),
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("parentHelper")),
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("parentSelector"))
                    })));
        }

        private SyntaxTokenList GetClassModifiers()
        {
            return SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
        }

        private BaseListSyntax GetBaseTypeDeclaration()
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

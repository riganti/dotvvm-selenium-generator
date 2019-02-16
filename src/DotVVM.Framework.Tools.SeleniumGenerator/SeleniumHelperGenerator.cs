﻿using System.IO;
using System.Linq;
using System.Text;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
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
    public class SeleniumHelperGenerator
    {

        public void ProcessMarkupFile(DotvvmConfiguration dotvvmConfiguration, SeleniumGeneratorConfiguration seleniumConfiguration)
        {
            // resolve control tree
            var tree = ResolveControlTree(seleniumConfiguration.ViewFullPath, dotvvmConfiguration);

            // var helper
            var helper = CreateHelperDefinition(seleniumConfiguration, tree);

            // generate the class
            GenerateHelperClass(seleniumConfiguration, helper);

            // update the markup file
            UpdateMarkupFile(seleniumConfiguration, helper);
        }

        private void UpdateMarkupFile(SeleniumGeneratorConfiguration seleniumConfiguration, HelperDefinition helper)
        {
            var sb = new StringBuilder(File.ReadAllText(seleniumConfiguration.ViewFullPath, Encoding.UTF8));
            foreach (var modification in helper.MarkupFileModifications.OrderByDescending(m => m.Position))
            {
                modification.Apply(sb);
            }
            File.WriteAllText(seleniumConfiguration.ViewFullPath, sb.ToString(), Encoding.UTF8);
        }

        private void GenerateHelperClass(SeleniumGeneratorConfiguration seleniumConfiguration, HelperDefinition helper)
        {
            var tree = CSharpSyntaxTree.Create(
                SyntaxFactory.CompilationUnit().WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                    {
                        SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(seleniumConfiguration.TargetNamespace))
                            .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
                            {
                                GenerateHelperClassContents(helper)
                            }))
                    }))
                    .NormalizeWhitespace()
            );

            File.WriteAllText(seleniumConfiguration.HelperFileFullPath, tree.ToString(), Encoding.UTF8);
        }

        private static HelperDefinition CreateHelperDefinition(SeleniumGeneratorConfiguration seleniumConfiguration, IAbstractTreeRoot tree)
        {
            // traverse the tree
            var visitor = new SeleniumHelperVisitor();
            visitor.PushScope(new HelperDefinition() { Name = seleniumConfiguration.HelperName });
            visitor.VisitView((ResolvedTreeRoot)tree);
            return visitor.PopScope();
        }

        private MemberDeclarationSyntax GenerateHelperClassContents(HelperDefinition helperDefinition)
        {
            return SyntaxFactory
                .ClassDeclaration(helperDefinition.Name)
                .WithModifiers(GetClassModifiers())
                .WithBaseList(GetBaseTypeDeclaration())
                .WithMembers(SyntaxFactory.List(helperDefinition.Members))
                .AddMembers(GetConstructor(helperDefinition))
                .AddMembers(helperDefinition.Children.Select(GenerateHelperClassContents).ToArray());
        }

        private static ConstructorDeclarationSyntax GetConstructor(HelperDefinition helperDefinition)
        {
            return SyntaxFactory
                .ConstructorDeclaration(helperDefinition.Name)
                .WithParameterList(GetConstructorMembers())
                .WithInitializer(GetBaseConstructorParameters())
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBody(SyntaxFactory.Block(helperDefinition.ConstructorStatements));
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

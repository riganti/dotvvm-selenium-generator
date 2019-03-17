using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Helpers
{
    internal static class RoslynGeneratingHelper
    {
        internal static StatementSyntax GenerateInitializerForProxy(string selector, string propertyName, string typeName, params string[] genericTypeNames)
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(propertyName),
                    SyntaxFactory.ObjectCreationExpression(ParseTypeName(typeName, genericTypeNames))
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[]
                            {
                                SyntaxFactory.Argument(SyntaxFactory.ThisExpression()),
                                SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(selector)))
                            }))
                        )
                )
            );
        }

        internal static TypeSyntax ParseTypeName(string typeName, params string[] genericTypeNames)
        {
            if (genericTypeNames.Length == 0)
            {
                return SyntaxFactory.ParseTypeName(typeName);
            }

            return SyntaxFactory.GenericName(typeName)
                .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(
                    genericTypeNames.Select(n => SyntaxFactory.ParseTypeName(n)))
                ));
        }

        // TODO: decide if proxy should be generated or using property like this
        internal static StatementSyntax GenerateInitializerForTemplate(string propertyName, string typeName)
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(propertyName),
                    SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(typeName))
                        .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("webDriver")),
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("parentHelper"))
                        })))
                )
            );
        }

        internal static MemberDeclarationSyntax GeneratePropertyForProxy(string uniqueName, string typeName, params string[] genericTypeNames)
        {
            return SyntaxFactory.PropertyDeclaration(
                    RoslynGeneratingHelper.ParseTypeName(typeName, genericTypeNames),
                    uniqueName
                )
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                );
        }
    }
}

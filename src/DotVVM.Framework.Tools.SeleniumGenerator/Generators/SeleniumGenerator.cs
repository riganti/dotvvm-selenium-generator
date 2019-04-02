using System;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Controls;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using Microsoft.CodeAnalysis;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators
{
    public abstract class SeleniumGenerator<TControl> : ISeleniumGenerator /*where TControl : DotvvmControl*/
    {
        protected const string DefaultNamespace = "DotVVM.Framework.Testing.SeleniumHelpers.Proxies";

        /// <summary>
        /// Gets a list of properties that can be used to determine the control name.
        /// </summary>
        public abstract DotvvmProperty[] NameProperties { get; }

        /// <summary>
        /// Gets a value indicating whether the content of the control can be used to generate the control name.
        /// </summary>
        public abstract bool CanUseControlContentForName { get; }


        public Type ControlType => typeof(TControl);

        /// <summary>
        /// Gets a list of declarations emitted by the control.
        /// </summary>
        public void AddDeclarations(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            string propertyName;

            var htmlName = TryGetNameFromProperty(context.Control, UITests.NameProperty);
            if (htmlName == null)
            {
                // determine the name
                propertyName = DetermineName(pageObject, context);
            }
            else
            {
                propertyName = htmlName;
            }

            // normalize name
            var normalizedName = RemoveNonIdentifierCharacters(propertyName);
            // make the name unique
            var uniqueName = MakePropertyNameUnique(context.UsedNames, normalizedName);

            context.UsedNames.Add(uniqueName);
            context.UniqueName = uniqueName;

            // determine the selector
            if (htmlName == null)
            {
                AddUITestNameProperty(pageObject, context, uniqueName);
            }

            context.UsedNames.Add(propertyName);

            AddDeclarationsCore(pageObject, context);
        }

        private string MakePropertyNameUnique(ICollection<string> usedNames, string selector)
        {
            if (usedNames.Contains(selector))
            {
                var index = 1;
                while (usedNames.Contains(selector + index))
                {
                    index++;
                }

                selector += index;
            }

            return selector;
        }

        private string RemoveUpperLetters(string selector)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < selector.Length; i++)
            {
                char c = selector[i];
                if (i != 0 && char.IsUpper(c))
                {
                    sb.Append('-');
                }

                sb.Append(char.ToLower(c));
            }

            return sb.ToString();
        }

        public virtual bool CanAddDeclarations(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            return true;
        }

        private void AddUITestNameProperty(PageObjectDefinition pageObject, SeleniumGeneratorContext context, string uniqueName)
        {
            // find end of the tag
            var token = context.Control.DothtmlNode.Tokens.First(t => t.Type == DothtmlTokenType.CloseTag || t.Type == DothtmlTokenType.Slash);

            pageObject.MarkupFileModifications.Add(new MarkupFileInsertText()
            {
                Text = " UITests.Name=\"" + uniqueName + "\"",
                Position = token.StartPosition
            });
        }

        protected virtual string DetermineName(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            // if selector is set, just read it and don't add data context prefixes
            //var shouldAddDataContextPrefixes = uniqueName == null;
            var shouldAddDataContextPrefixes = true;

            // if not found, use the name properties to determine the name

            string uniqueName = null;
            foreach (var nameProperty in NameProperties)
            {
                uniqueName = TryGetNameFromProperty(context.Control, nameProperty);
                if (uniqueName != null)
                {
                    uniqueName = NormalizeUniqueName(uniqueName);
                    break;
                }
            }

            // if not found, try to use the content of the control to determine the name
            if (uniqueName == null && CanUseControlContentForName)
            {
                uniqueName = GetTextFromContent(context.Control.Content);
            }

            // check if control is userControl and assign control's name as unique name
            if (uniqueName == null && context.Control.DothtmlNode is DothtmlElementNode htmlNode)
            {
                uniqueName = htmlNode.TagName;

                // not add DataContext when generating page object for user control
                shouldAddDataContextPrefixes = false;
            }

            // if not found, use control name
            if (uniqueName == null)
            {
                uniqueName = typeof(TControl).Name;
            }

            if (shouldAddDataContextPrefixes)
            {
                uniqueName = AddDataContextPrefixesToName(pageObject.DataContextPrefixes, uniqueName);
            }

            return uniqueName;
        }

        protected string AddDataContextPrefixesToName(IList<string> dataContextPrefixes, string uniqueName)
        {
            if (dataContextPrefixes.Any())
            {
                return $"{string.Join("_", dataContextPrefixes)}_{SetFirstLetterUp(uniqueName)}";
            }

            return SetFirstLetterUp(uniqueName);
        }

        private string SetFirstLetterUp(string uniqueName)
        {
            return uniqueName.First().ToString().ToUpper() + uniqueName.Substring(1);
        }

        private string NormalizeUniqueName(string uniqueName)
        {
            var normalizedName = RemoveDiacritics(uniqueName);
            var firstLetterOfName = normalizedName[0];

            // if first letter is numeric add underscore to the name
            if (char.IsDigit(firstLetterOfName))
            {
                normalizedName = normalizedName.Insert(0, "_");
            }
            else
            {
                char[] a = normalizedName.ToCharArray();
                a[0] = char.ToUpper(a[0]);
                normalizedName = new string(a);
            }

            return normalizedName;
        }

        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        protected string TryGetNameFromProperty(ResolvedControl control, DotvvmProperty property)
        {
            if (control.TryGetProperty(property, out IAbstractPropertySetter setter))
            {
                switch (setter)
                {
                    case ResolvedPropertyValue propertySetter:
                        return propertySetter.Value?.ToString();

                    case ResolvedPropertyBinding propertyBinding:
                        return propertyBinding.Binding.Value;
                }
            }
            return null;
        }

        private string RemoveNonIdentifierCharacters(string value)
        {
            var sb = new StringBuilder();
            var isLastLetterWhitespace = false;

            for (var i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    if (i == 0)
                    {
                        c = char.ToUpper(c);
                    }
                    else if (isLastLetterWhitespace)
                    {
                        c = char.ToUpper(c);
                        isLastLetterWhitespace = false;
                    }

                    sb.Append(c);
                }
                else if (char.IsWhiteSpace(c))
                {
                    isLastLetterWhitespace = true;
                }
            }

            if (sb.Length == 0)
            {
                return null;
            }

            if (char.IsDigit(sb[0]))
            {
                sb.Insert(0, '_');
            }

            return sb.ToString();
        }

        private string GetTextFromContent(IEnumerable<ResolvedControl> controls)
        {
            var sb = new StringBuilder();

            foreach (var control in controls)
            {
                if (control.Metadata.Type == typeof(Literal))
                {
                    sb.Append(TryGetNameFromProperty(control, Literal.TextProperty));
                }
                else if (control.Metadata.Type == typeof(HtmlGenericControl))
                {
                    sb.Append(TryGetNameFromProperty(control, HtmlGenericControl.InnerTextProperty));
                }
            }

            // ensure the text is not too long
            var text = RemoveNonIdentifierCharacters(sb.ToString());
            if (text?.Length > 20)
            {
                text = text.Substring(0, 20);
            }
            return text;
        }

        protected void AddPageObjectProperties(PageObjectDefinition pageObject, SeleniumGeneratorContext context, string type)
        {
            pageObject.Members.Add(GeneratePropertyForProxy(context.UniqueName, type));
            pageObject.ConstructorStatements.Add(GenerateInitializerForProxy(context, type));
        }

        protected void AddGenericPageObjectProperties(PageObjectDefinition pageObject,
            SeleniumGeneratorContext context,
            string type,
            string itemHelperName)
        {
            pageObject.Members.Add(GeneratePropertyForProxy(context.UniqueName, type, itemHelperName));
            pageObject.ConstructorStatements.Add(GenerateInitializerForProxy(context, type, itemHelperName));
        }

        private static TypeSyntax ParseTypeName(string typeName, params string[] genericTypeNames)
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

        protected MemberDeclarationSyntax GeneratePropertyForProxy(string uniqueName, string typeName, params string[] genericTypeNames)
        {
            return SyntaxFactory.PropertyDeclaration(
                    ParseTypeName(typeName, genericTypeNames),
                    uniqueName
                )
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                );
        }

        protected StatementSyntax GenerateInitializerForProxy(SeleniumGeneratorContext context, string typeName, params string[] genericTypeNames)
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(context.UniqueName),
                    SyntaxFactory.ObjectCreationExpression(ParseTypeName(typeName, genericTypeNames))
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[]
                            {
                                SyntaxFactory.Argument(SyntaxFactory.ThisExpression()),
                                SyntaxFactory.Argument(GetPathSelectorObjectInitialization(context.UniqueName))
                            }))
                        )
                )
            );
        }

        // TODO: decide if proxy should be generated or using property like this
        protected StatementSyntax GenerateInitializerForControl(string propertyName, string typeName)
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(propertyName),
                    SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(typeName))
                        .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("webDriver")),
                            SyntaxFactory.Argument(SyntaxFactory.ThisExpression()),
                            SyntaxFactory.Argument(GetPathSelectorObjectInitialization(propertyName))
                        })))
                )
            );
        }

        protected StatementSyntax GenerateInitializerForTemplate(string propertyName, string typeName)
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(propertyName),
                    SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(typeName))
                        .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("webDriver")),
                            SyntaxFactory.Argument(SyntaxFactory.ThisExpression()),
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("parentSelector"))
                        })))
                )
            );
        }

        private ObjectCreationExpressionSyntax GetPathSelectorObjectInitialization(string propertyName)
        {
            return SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.IdentifierName("PathSelector"))
                .WithInitializer(
                    SyntaxFactory.InitializerExpression(
                        SyntaxKind.ObjectInitializerExpression,
                        SyntaxFactory.SeparatedList<ExpressionSyntax>(new SyntaxNodeOrToken[]
                        {
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName("UiName"),
                                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(propertyName))),
                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName("Parent"),
                                SyntaxFactory.IdentifierName("parentSelector"))
                        })
                    )
                );
        }

        protected abstract void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context);

    }
}

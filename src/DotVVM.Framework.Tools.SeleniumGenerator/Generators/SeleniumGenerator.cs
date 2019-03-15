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
            // determine the name
            var propertyName = DetermineName(pageObject, context);

            // make the name unique
            var uniqueName = MakePropertyNameUnique(context, propertyName);

            context.UsedNames.Add(uniqueName);
            context.UniqueName = uniqueName;

            // determine the selector
            var selector = TryGetNameFromProperty(context.Control, UITests.NameProperty);
            if (selector == null)
            {
                selector = uniqueName;

                AddUITestNameProperty(pageObject, context, uniqueName);
            }
            context.Selector = selector;

            AddDeclarationsCore(pageObject, context);
        }

        private static string MakePropertyNameUnique(SeleniumGeneratorContext context, string propertyName)
        {
            if (context.UsedNames.Contains(propertyName))
            {
                var index = 1;
                while (context.UsedNames.Contains(propertyName + index))
                {
                    index++;
                }

                propertyName += index;
            }

            return propertyName;
        }

        public virtual bool CanAddDeclarations(PageObjectDefinition pageObjectDefinition, SeleniumGeneratorContext context)
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
            // get the name from the UITest.Name property
            var uniqueName = TryGetNameFromProperty(context.Control, UITests.NameProperty);

            // if selector is set, just read it and don't add data context prefixes
            var shouldAddDataContextPrefixes = uniqueName == null;

            // if not found, use the name properties to determine the name
            if (uniqueName == null)
            {
                foreach (var nameProperty in NameProperties)
                {
                    uniqueName = TryGetNameFromProperty(context.Control, nameProperty);
                    if (uniqueName != null)
                    {
                        uniqueName = NormalizeUniqueName(uniqueName);
                        break;
                    }
                }
            }

            // if not found, try to use the content of the control to determine the name
            if (uniqueName == null && CanUseControlContentForName)
            {
                uniqueName = GetTextFromContent(context.Control.Content);
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

        private string AddDataContextPrefixesToName(IList<string> dataContextPrefixes, string uniqueName)
        {
            if(dataContextPrefixes.Any())
            {
                uniqueName = $"{string.Join("", dataContextPrefixes)}{SetFirstLetterUp(uniqueName)}";
            }

            return uniqueName;
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
            IAbstractPropertySetter setter;
            if (control.TryGetProperty(property, out setter))
            {
                if (setter is ResolvedPropertyValue)
                {
                    return RemoveNonIdentifierCharacters(((ResolvedPropertyValue)setter).Value?.ToString());
                }
                else if (setter is ResolvedPropertyBinding)
                {
                    var binding = ((ResolvedPropertyBinding)setter).Binding;
                    return RemoveNonIdentifierCharacters(binding.Value);
                }
            }
            return null;
        }

        private string RemoveNonIdentifierCharacters(string value)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsLetterOrDigit(value[i]) || value[i] == '_')
                {
                    sb.Append(value[i]);
                }
            }

            if (sb.Length == 0)
            {
                return null;
            }
            else if (char.IsDigit(sb[0]))
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
            pageObject.ConstructorStatements.Add(GenerateInitializerForProxy(context, context.UniqueName, type));
        }

        protected void AddGenericPageObjectProperties(PageObjectDefinition pageObject, 
            SeleniumGeneratorContext context,
            string type, 
            string itemHelperName)
        {
            pageObject.Members.Add(GeneratePropertyForProxy(context.UniqueName, type, itemHelperName));
            pageObject.ConstructorStatements.Add(GenerateInitializerForProxy(context, context.UniqueName, type, itemHelperName));
        }

        private static TypeSyntax ParseTypeName(string typeName, params string[] genericTypeNames)
        {
            if (genericTypeNames.Length == 0)
            {
                return SyntaxFactory.ParseTypeName(typeName);
            }
            else
            {
                return SyntaxFactory.GenericName(typeName)
                    .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(
                            genericTypeNames.Select(n => SyntaxFactory.ParseTypeName(n)))
                    ));
            }
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

        protected StatementSyntax GenerateInitializerForProxy(SeleniumGeneratorContext context, string propertyName, string typeName, params string[] genericTypeNames)
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
                                SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(context.Selector)))
                            }))
                        )
                )
            );
        }

        protected abstract void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context);

    }
}

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators
{
    public class PageObjectDefinition
    {
        public string Name { get; set; }

        public List<string> DataContextPrefixes { get; set; } = new List<string>();
        public HashSet<string> UsedNames { get; } = new HashSet<string>();
        public HashSet<string> UsedSelectors { get; } = new HashSet<string>();

        public List<MemberDeclarationSyntax> MemberDeclarations { get; set; } = new List<MemberDeclarationSyntax>();

        public List<StatementSyntax> ConstructorStatements { get; set; } = new List<StatementSyntax>();

        public List<PageObjectDefinition> Children { get; } = new List<PageObjectDefinition>();

        public List<MarkupFileModification> MarkupFileModifications { get; set; } = new List<MarkupFileModification>();

        public List<MemberDefinition> Members { get; set; } = new List<MemberDefinition>();
    }
}

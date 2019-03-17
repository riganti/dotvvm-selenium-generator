using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators
{
    public class MemberDefinition
    {
        public string Selector { get; set; }
        public string Name { get; set; }
        public StatementSyntax ConstructorStatement { get; set; }
        public MarkupFileModification MarkupFileModification { get; set; }
        public MemberDeclarationSyntax MemberDeclaration { get; set; }
        public string MemberType { get; set; }
        public string[] GenericTypeNames { get; set; } = { };
    }
}
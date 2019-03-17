using System.Text;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators
{
    public class MarkupFileInsertText : MarkupFileModification
    {
        public string Selector { get; set; }

        public string Text => " UITests.Name=\"" + Selector + "\"";

        public override void Apply(StringBuilder markupFile)
        {
            markupFile.Insert(Position, Text);
        }
    }
}
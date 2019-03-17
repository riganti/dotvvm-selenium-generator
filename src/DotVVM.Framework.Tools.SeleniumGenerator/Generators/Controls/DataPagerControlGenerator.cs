using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Generators.Controls
{
    public class DataPagerControlGenerator : SeleniumGenerator<DataPager>
    {
        private static readonly DotvvmProperty[] nameProperties = { DataPager.DataSetProperty };
        public override DotvvmProperty[] NameProperties => nameProperties;

        public override bool CanUseControlContentForName => false;
        protected override void AddDeclarationsCore(PageObjectDefinition pageObject, SeleniumGeneratorContext context)
        {
            var type = $"{DefaultNamespace}.DataPagerProxy";
            context.Member.MemberType = type;
            AddPageObjectProperties(pageObject, context, type);
        }
    }
}

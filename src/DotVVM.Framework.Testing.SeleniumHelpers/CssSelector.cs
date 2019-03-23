namespace DotVVM.Framework.Testing.SeleniumHelpers
{
    public class CssSelector
    {
        public string UiName { get; set; }

        public int? Index { get; set; }

        public CssSelector Parent { get; set; }

        public override string ToString()
        {
            //TODO: after DataContextPrefixed will be working again
            //if (Parent != null)
            //{
            //    return Parent.ToString() + "_" + UiName;
            //}

            //return UiName;
            if (Index == null)
            {
                return $"//*[@data-uitest-name='{UiName}']";

            }
            else
            {
                return UiName;
            }
        }
    }
}

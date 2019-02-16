namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class RadioButtonProxy : WebElementProxyBase
    {
        public RadioButtonProxy(SeleniumHelperBase helper, string selector) : base(helper, selector)
        {
        }

        public bool IsSelected()
        {
            return FindElement().Selected;
        }

        public void Select()
        {
            FindElement().Click();
        }
    }
}
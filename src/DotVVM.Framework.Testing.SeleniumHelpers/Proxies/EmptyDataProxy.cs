namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class EmptyDataProxy : WebElementProxyBase
    {
        public EmptyDataProxy(SeleniumHelperBase helper, string selector) : base(helper, selector)
        {
        }

        public bool IsVisible()
        {
            return FindElement().Displayed;
        }
    }
}
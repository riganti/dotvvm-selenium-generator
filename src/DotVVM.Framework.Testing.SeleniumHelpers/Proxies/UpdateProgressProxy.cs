namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class UpdateProgressProxy : WebElementProxyBase
    {
        public UpdateProgressProxy(SeleniumHelperBase helper, CssSelector selector) : base(helper, selector)
        {
        }

        public bool IsUpdateInProgress()
        {
            return FindElement().Displayed;
        }
    }
}
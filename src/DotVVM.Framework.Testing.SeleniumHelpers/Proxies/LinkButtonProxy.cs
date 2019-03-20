using OpenQA.Selenium.Interactions;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class LinkButtonProxy : ButtonProxy
    {
        public LinkButtonProxy(SeleniumHelperBase helper, CssSelector selector) : base(helper, selector)
        {
        }
    }
}
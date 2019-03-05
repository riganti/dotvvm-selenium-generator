using System.Linq;
using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public abstract class WebElementProxyBase
    {
        public SeleniumHelperBase Helper { get; private set; }

        public string Selector { get; private set; }


        public WebElementProxyBase(SeleniumHelperBase helper, string selector)
        {
            Helper = helper;
            Selector = selector;
        }

        // TODO: rewrite - filter (check OneNote)
        // selector can't be string - use some object or list
        protected IWebElement FindElement()
        {
            var selector = Helper.BuildElementSelector(Selector);
            return Helper.WebDriver.FindElement(By.CssSelector(selector));
        }

    }
    
}
using System;
using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class RepeaterProxy<TItemHelper> : WebElementProxyBase where TItemHelper : SeleniumHelperBase
    {
        public RepeaterProxy(SeleniumHelperBase helper, CssSelector selector) : base(helper, selector)
        {
        }

        public int GetItemsCount()
        {
            return FindElement().FindElements(By.XPath("*")).Count;
        }

        // TODO: CssSelector class
        public TItemHelper GetItem(int index)
        {
            var selector = Helper.BuildElementSelector(Selector) + ">*:nth-child(" + (index + 1) + ")";

            return (TItemHelper) Activator.CreateInstance(typeof(TItemHelper), Helper.WebDriver, Helper, selector);
        }
    }
}
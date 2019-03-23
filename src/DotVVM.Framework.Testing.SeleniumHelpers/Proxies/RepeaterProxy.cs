using System;
using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class RepeaterProxy<TItemHelper> : WebElementProxyBase where TItemHelper : SeleniumHelperBase
    {
        public RepeaterProxy(SeleniumHelperBase helper, PathSelector selector) : base(helper, selector)
        {
        }

        public int GetItemsCount()
        {
            return FindElement().FindElements(By.XPath("*")).Count;
        }

        // TODO: CssSelector class
        public TItemHelper GetItem(int index)
        {
            var selector = $"{Helper.BuildElementSelector(Selector)}";

            var sel = new PathSelector
            {
                Index = index,
                Parent = Selector,
                UiName = selector
            };

            return (TItemHelper) Activator.CreateInstance(typeof(TItemHelper), Helper.WebDriver, Helper, sel);
        }
    }
}
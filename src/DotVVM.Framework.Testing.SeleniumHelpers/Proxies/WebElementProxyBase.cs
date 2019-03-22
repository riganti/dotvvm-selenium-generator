using System;
using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public abstract class WebElementProxyBase
    {
        public SeleniumHelperBase Helper { get; private set; }

        public CssSelector Selector { get; private set; }


        public WebElementProxyBase(SeleniumHelperBase helper, CssSelector selector)
        {
            Helper = helper;
            Selector = selector;
        }

        // TODO: rewrite - filter (check OneNote)
        // selector can't be string - use some object or list
        protected IWebElement FindElement()
        {
            var selector = Helper.BuildElementSelector(Selector);

            var elements = Helper.WebDriver.FindElements(By.CssSelector(selector));
            foreach(var element in elements)
            {
                if(Selector.Parent != null)
                {

                }
                else
                {
                    var parent = element.FindElement(By.XPath(".."));
                }
            }

            return elements[2];

            //return Helper.WebDriver.FindElement(By.CssSelector(selector));
        }

        public virtual bool IsVisible()
        {
            try
            {
                return FindElement().Displayed;
            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine(@"Element is not in page. " + e);
                return false;
            }
        }

        public virtual bool IsEnabled()
        {
            return FindElement().Enabled;
        }
    }
}
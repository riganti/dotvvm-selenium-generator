using System;
using System.Linq;
using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public abstract class WebElementProxyBase
    {
        private const string AttributeName = "data-uitest-name";

        public SeleniumHelperBase Helper { get; private set; }

        public CssSelector Selector { get; private set; }


        protected WebElementProxyBase(SeleniumHelperBase helper, CssSelector selector)
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
                var parentSelector = Selector.Parent;
                var isFound = true;

                var parents = element.FindElements(By.XPath("./ancestor::*[not(name()='body' or name()='html')] ")).Reverse();
                foreach (var parent in parents)
                {
                    var attribute = parent.GetAttribute(AttributeName);

                    if (parentSelector?.Index != null)
                    {
                        var neighbors = parent.FindElements(By.XPath(""));
;
                        attribute = $"[data-uitest-name={attribute}]>*:nth-child({neighbors.IndexOf(parent)})";
                    }

                    if (attribute != null && parentSelector == null)
                    {
                        isFound = false;
                        break;
                    }

                    if (attribute != null && parentSelector.UiName != attribute)
                    {
                        isFound = false;
                        break;
                    }
                    else
                    {
                        parentSelector = parentSelector.Parent;
                    }
                }

                if (isFound)
                {
                    return element;
                }
            }

            throw new NotFoundException();

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
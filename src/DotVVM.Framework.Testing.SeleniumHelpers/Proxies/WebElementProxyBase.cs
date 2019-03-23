using System;
using System.Linq;
using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public abstract class WebElementProxyBase
    {
        private const string AttributeName = "data-uitest-name";
        private const string AncestorString = "./ancestor::*[not(name()='body' or name()='html') and @data-uitest-name]";

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

            var elements = Helper.WebDriver.FindElements(By.XPath(selector));
            foreach(var element in elements)
            {
                CssSelector parentSelector = Selector.Parent;
                IWebElement childElement = element;
                string childAttribute = Selector.UiName;
                bool isFound = true;

                var parents = element.FindElements(By.XPath(AncestorString)).Reverse().ToList();
                foreach (var parent in parents)
                {
                    var attribute = parent.GetAttribute(AttributeName);

                    if (parentSelector?.Index != null)
                    {
                        var siblings = parent.FindElements(By.XPath($"./*[@{AttributeName}='{childAttribute}']"));
                        if (siblings.IndexOf(childElement) != parentSelector.Index)
                        {
                            isFound = false;
                            break;
                        }

                        // Refactor
                        attribute = $"//*[@{AttributeName}='{attribute}']";
                    }

                    //var isParentLast = parent.Equals(parents.Last());
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

                    parentSelector = parentSelector?.Parent;
                    childElement = parent;
                    childAttribute = attribute;
                }

                if (isFound)
                {
                    return element;
                }
            }

            throw new NotFoundException();
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
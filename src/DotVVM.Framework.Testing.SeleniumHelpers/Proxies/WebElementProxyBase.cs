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

        public PathSelector Selector { get; private set; }


        protected WebElementProxyBase(SeleniumHelperBase helper, PathSelector selector)
        {
            Helper = helper;
            Selector = selector;
        }

        // TODO: rewrite - filter (check OneNote)
        // selector can't be string - use some object or list
        protected IWebElement FindElement()
        {
            var elementSelector = Helper.BuildElementSelector(Selector);

            var elementsBySelector = Helper.WebDriver.FindElements(By.XPath(elementSelector));
            foreach(var element in elementsBySelector)
            {
                PathSelector parentSelector = Selector.Parent;
                IWebElement childElement = element;
                string childAttribute = Selector.UiName;
                bool isFound = true;

                var ancestors = element.FindElements(By.XPath(AncestorString)).Reverse();
                foreach (var ancestor in ancestors)
                {
                    var ancestorAttribute = ancestor.GetAttribute(AttributeName);

                    if (parentSelector?.Index != null)
                    {
                        var siblings = ancestor.FindElements(By.XPath($".//*[@{AttributeName}='{childAttribute}']"));
                        if (siblings.IndexOf(childElement) != parentSelector.Index)
                        {
                            isFound = false;
                            break;
                        }

                        //TODO: Refactor
                        ancestorAttribute = $"//*[@{AttributeName}='{ancestorAttribute}']";
                    }

                    //var isParentLast = parent.Equals(parents.Last());
                    if (ancestorAttribute != null && parentSelector == null)
                    {
                        isFound = false;
                        break;
                    }

                    if (ancestorAttribute != null && parentSelector.UiName != ancestorAttribute)
                    {
                        isFound = false;
                        break;
                    }

                    parentSelector = parentSelector?.Parent;
                    childElement = ancestor;
                    childAttribute = ancestorAttribute;
                }

                if (isFound)
                {
                    return element;
                }
            }

            throw new NoSuchElementException();
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
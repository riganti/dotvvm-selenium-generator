using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class DataPagerProxy : WebElementProxyBase
    {
        public DataPagerProxy(SeleniumHelperBase helper, string selector) : base(helper, selector)
        {
        }

        public bool IsVisible()
        {
            return FindElement().Displayed;
        }

        private IEnumerable<IWebElement> GetDataPagerListItems()
        {
            var element = FindElement();
            return element.FindElements(By.XPath(".//*"));
        }

        public void GoToFirstPage()
        {
            var firstItem = GetDataPagerListItems().First();
            if (firstItem.Enabled)
            {
                firstItem.Click();
            }
        }

        public void GoToLastPage()
        {
            var element = FindElement();
        }

        public void GoToPreviousPage()
        {
            var element = FindElement();
        }

        public void GoToNextPage()
        {
            var element = FindElement();
        }

        public void GoToPage(int pageNumber)
        {
            var element = FindElement();
        }
    }
}
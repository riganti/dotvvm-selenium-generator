using System;
using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class EmptyDataProxy : WebElementProxyBase
    {
        public EmptyDataProxy(SeleniumHelperBase helper, string selector) : base(helper, selector)
        {
        }

        public bool IsVisible()
        {
            try
            {
                return FindElement().Displayed;
            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine(@"EmptyData is not visible" + e);
                return false;
            }
        }
    }
}
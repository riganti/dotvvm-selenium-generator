using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class ComboBoxProxy : WebElementProxyBase
    {
        public ComboBoxProxy(SeleniumHelperBase helper, string selector) : base(helper, selector)
        {
        }

        public bool SelectPlaceholder()
        {
            return SelectOptionByValue("");
        }

        public bool SelectOptionByContent(string content)
        {
            var selectElement = GetSelectElement();

            try
            {
                selectElement.SelectByText(content);
            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine($@"ComboBox doesn't have option with text - {content}.");
                return false;
            }

            return true;
        }

        public bool SelectOptionByValue(string value)
        {
            var selectElement = GetSelectElement();

            try
            {
                selectElement.SelectByValue(value);
            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine($@"ComboBox doesn't have option with value - {value}.");
                return false;
            }

            return true;
        }

        public bool SelectOptionByIndex(int optionIndex)
        {
            var selectElement = GetSelectElement();

            try
            {
                selectElement.SelectByIndex(optionIndex);
            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine($@"ComboBox doesn't have option with index {optionIndex}.");
                return false;
            }

            return true;
        }

        public IWebElement GetSelectedOption()
        {
            var selectElement = GetSelectElement();

            return selectElement.SelectedOption;
        }

        private SelectElement GetSelectElement()
        {
            var element = FindElement();
            return new SelectElement(element);
        }

        private IEnumerable<IWebElement> GetComboBoxOptions(IWebElement element)
        {
            return element.FindElements(By.XPath(".//*"));
        }
    }
}   
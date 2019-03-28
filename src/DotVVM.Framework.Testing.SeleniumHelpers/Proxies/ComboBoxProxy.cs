using System;
using DotVVM.Framework.Testing.SeleniumHelpers.Proxies.Base;
using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class ComboBoxProxy : SelectBaseProxy
    {
        public ComboBoxProxy(SeleniumHelperBase helper, PathSelector selector) : base(helper, selector)
        {
        }

        public bool SelectPlaceholder()
        {
            return SelectOptionByValue("");
        }

        public virtual bool SelectOptionByValue(string value)
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
    }
}   
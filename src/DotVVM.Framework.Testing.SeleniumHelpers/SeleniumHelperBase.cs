using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.SeleniumHelpers
{
    public abstract class SeleniumHelperBase
    {

        public IWebDriver WebDriver { get; private set; }

        public SeleniumHelperBase ParentHelper { get; set; }

        public CssSelector ParentSelector { get; private set; }


        public SeleniumHelperBase(IWebDriver webDriver, SeleniumHelperBase parentHelper = null, CssSelector parentSelector = null)
        {
            WebDriver = webDriver;
            ParentHelper = parentHelper;
            ParentSelector = parentSelector;
        }

        public string BuildElementSelector(CssSelector elementUniqueName)
        {
            var selector = $"[data-uitest-name={elementUniqueName.UiName}]";

            if (ParentSelector == null)
            {
                return selector;
            }
            else
            {
                return ParentSelector.UiName + " " + selector;
            }
        }
        
    }
}

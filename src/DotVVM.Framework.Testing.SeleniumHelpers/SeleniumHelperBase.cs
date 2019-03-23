using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.SeleniumHelpers
{
    public abstract class SeleniumHelperBase
    {

        public IWebDriver WebDriver { get; private set; }

        public SeleniumHelperBase ParentHelper { get; set; }

        public PathSelector ParentSelector { get; private set; }


        public SeleniumHelperBase(IWebDriver webDriver, SeleniumHelperBase parentHelper = null, PathSelector parentSelector = null)
        {
            WebDriver = webDriver;
            ParentHelper = parentHelper;
            ParentSelector = parentSelector;
        }

        //TODO: build selector using parent's selector
        public string BuildElementSelector(PathSelector elementUniqueName)
        {
            var selector = $"//*[@data-uitest-name='{elementUniqueName.UiName}']";

            if (ParentSelector == null)
            {
                return selector;
            }
            else if (ParentSelector?.Index != null)
            {
                return $"{ParentSelector.ToString()}{elementUniqueName}";
            }
            else
            {
                return $"{ParentSelector}{selector}";
            }
        }
        
    }
}

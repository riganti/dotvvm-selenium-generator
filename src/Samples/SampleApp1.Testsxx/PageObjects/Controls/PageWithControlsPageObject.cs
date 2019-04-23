using DotVVM.Framework.Testing.SeleniumHelpers;
using DotVVM.Framework.Testing.SeleniumHelpers.Proxies;

namespace SampleApp1.Tests.PageObjects.Controls
{
    public class PageWithControlsPageObject : SeleniumHelperBase
    {
        public ControlAPageObject ControlA
        {
            get;
        }

        public RepeaterProxy<SectionsRepeaterPageObject> Sections
        {
            get;
        }

        public TextBoxProxy Name
        {
            get;
        }

        public ButtonProxy AddControlB
        {
            get;
        }

        public PageWithControlsPageObject(OpenQA.Selenium.IWebDriver webDriver, SeleniumHelperBase parentHelper = null, PathSelector parentSelector = null): base (webDriver, parentHelper, parentSelector)
        {
            ControlA = new ControlAPageObject(webDriver, this, new PathSelector{UiName = "ControlA", Parent = parentSelector});
            Sections = new RepeaterProxy<SectionsRepeaterPageObject>(this, new PathSelector{UiName = "Sections", Parent = parentSelector});
            Name = new TextBoxProxy(this, new PathSelector{UiName = "Name", Parent = parentSelector});
            AddControlB = new ButtonProxy(this, new PathSelector{UiName = "AddControlB", Parent = parentSelector});
        }

        public class SectionsRepeaterPageObject : SeleniumHelperBase
        {
            public SampleApp1.SeleniumGenerators.MyControlBPageObject ControlB
            {
                get;
            }

            public TextBoxProxy Language
            {
                get;
            }

            public SectionsRepeaterPageObject(OpenQA.Selenium.IWebDriver webDriver, SeleniumHelperBase parentHelper = null, PathSelector parentSelector = null): base (webDriver, parentHelper, parentSelector)
            {
                ControlB = new SampleApp1.SeleniumGenerators.MyControlBPageObject(this, new PathSelector{UiName = "ControlB", Parent = parentSelector});
                Language = new TextBoxProxy(this, new PathSelector{UiName = "Language", Parent = parentSelector});
            }
        }
    }
}
using DotVVM.Framework.Testing.SeleniumHelpers;
using DotVVM.Framework.Testing.SeleniumHelpers.Proxies;

namespace SampleApp1.Tests.PageObjects.SimplePage
{
    public class TestPagePageObject : SeleniumHelperBase
    {
        public RepeaterProxy<UsersRepeaterPageObject> Users
        {
            get;
        }

        public ButtonProxy Refresh
        {
            get;
        }

        public TestPagePageObject(OpenQA.Selenium.IWebDriver webDriver, SeleniumHelperBase parentHelper = null, PathSelector parentSelector = null): base (webDriver, parentHelper, parentSelector)
        {
            Users = new RepeaterProxy<UsersRepeaterPageObject>(this, new PathSelector{UiName = "Users", Parent = parentSelector});
            Refresh = new ButtonProxy(this, new PathSelector{UiName = "Refresh", Parent = parentSelector});
        }

        public class UsersRepeaterPageObject : SeleniumHelperBase
        {
            public LiteralProxy Name
            {
                get;
            }

            public UsersRepeaterPageObject(OpenQA.Selenium.IWebDriver webDriver, SeleniumHelperBase parentHelper = null, PathSelector parentSelector = null): base (webDriver, parentHelper, parentSelector)
            {
                Name = new LiteralProxy(this, new PathSelector{UiName = "Name", Parent = parentSelector});
            }
        }
    }
}
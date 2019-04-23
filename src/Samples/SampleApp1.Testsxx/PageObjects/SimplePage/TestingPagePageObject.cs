using DotVVM.Framework.Testing.SeleniumHelpers;

namespace SampleApp1.Tests.PageObjects.SimplePage
{
    public class TestingPagePageObject : DotVVM.Framework.Testing.SeleniumHelpers.SeleniumHelperBase
    {
        public DotVVM.Framework.Testing.SeleniumHelpers.Proxies.ButtonProxy PerformDifficultCalculation
        {
            get;
        }

        public DotVVM.Framework.Testing.SeleniumHelpers.Proxies.EmptyDataProxy Customers
        {
            get;
        }

        public DotVVM.Framework.Testing.SeleniumHelpers.Proxies.EmptyDataProxy CustomersXXX
        {
            get;
        }

        public DotVVM.Framework.Testing.SeleniumHelpers.Proxies.ListBoxProxy SelectedCustomerListBox
        {
            get;
        }

        public DotVVM.Framework.Testing.SeleniumHelpers.Proxies.GridViewProxy<Customers2GridViewPageObject> Customers2
        {
            get;
        }

        public DotVVM.Framework.Testing.SeleniumHelpers.Proxies.DataPagerProxy Customers3
        {
            get;
        }

        public DotVVM.Framework.Testing.SeleniumHelpers.Proxies.ComboBoxProxy SelectedCustomer
        {
            get;
        }

        public TestingPagePageObject(OpenQA.Selenium.IWebDriver webDriver, DotVVM.Framework.Testing.SeleniumHelpers.SeleniumHelperBase parentHelper = null, DotVVM.Framework.Testing.SeleniumHelpers.PathSelector parentSelector = null): base (webDriver, parentHelper, parentSelector)
        {
            PerformDifficultCalculation = new DotVVM.Framework.Testing.SeleniumHelpers.Proxies.ButtonProxy(this, new PathSelector{UiName = "PerformDifficultCalculation", Parent = parentSelector});
            Customers = new DotVVM.Framework.Testing.SeleniumHelpers.Proxies.EmptyDataProxy(this, new PathSelector{UiName = "Customers", Parent = parentSelector});
            CustomersXXX = new DotVVM.Framework.Testing.SeleniumHelpers.Proxies.EmptyDataProxy(this, new PathSelector{UiName = "CustomersXXX", Parent = parentSelector});
            SelectedCustomerListBox = new DotVVM.Framework.Testing.SeleniumHelpers.Proxies.ListBoxProxy(this, new PathSelector{UiName = "SelectedCustomerListBox", Parent = parentSelector});
            Customers2 = new DotVVM.Framework.Testing.SeleniumHelpers.Proxies.GridViewProxy<Customers2GridViewPageObject>(this, new PathSelector{UiName = "Customers2", Parent = parentSelector});
            Customers3 = new DotVVM.Framework.Testing.SeleniumHelpers.Proxies.DataPagerProxy(this, new PathSelector{UiName = "Customers3", Parent = parentSelector});
            SelectedCustomer = new DotVVM.Framework.Testing.SeleniumHelpers.Proxies.ComboBoxProxy(this, new PathSelector{UiName = "SelectedCustomer", Parent = parentSelector});
        }

        public class Customers2GridViewPageObject : DotVVM.Framework.Testing.SeleniumHelpers.SeleniumHelperBase
        {
            public DotVVM.Framework.Testing.SeleniumHelpers.Proxies.GridViewColumns.GridViewTextColumnProxy Id
            {
                get;
            }

            public DotVVM.Framework.Testing.SeleniumHelpers.Proxies.GridViewColumns.GridViewTextColumnProxy Name
            {
                get;
            }

            public PostalCodeGridViewTemplateColumn PostalCode
            {
                get;
            }

            public DotVVM.Framework.Testing.SeleniumHelpers.Proxies.GridViewColumns.GridViewCheckBoxColumnProxy IsChecked
            {
                get;
            }

            public Customers2GridViewPageObject(OpenQA.Selenium.IWebDriver webDriver, DotVVM.Framework.Testing.SeleniumHelpers.SeleniumHelperBase parentHelper = null, DotVVM.Framework.Testing.SeleniumHelpers.PathSelector parentSelector = null): base (webDriver, parentHelper, parentSelector)
            {
                Id = new DotVVM.Framework.Testing.SeleniumHelpers.Proxies.GridViewColumns.GridViewTextColumnProxy(this, new PathSelector{UiName = "Id", Parent = parentSelector});
                Name = new DotVVM.Framework.Testing.SeleniumHelpers.Proxies.GridViewColumns.GridViewTextColumnProxy(this, new PathSelector{UiName = "Name", Parent = parentSelector});
                PostalCode = new PostalCodeGridViewTemplateColumn(webDriver, this, parentSelector);
                IsChecked = new DotVVM.Framework.Testing.SeleniumHelpers.Proxies.GridViewColumns.GridViewCheckBoxColumnProxy(this, new PathSelector{UiName = "IsChecked", Parent = parentSelector});
            }

            public class PostalCodeGridViewTemplateColumn : DotVVM.Framework.Testing.SeleniumHelpers.SeleniumHelperBase
            {
                public DotVVM.Framework.Testing.SeleniumHelpers.Proxies.TextBoxProxy PostalCode
                {
                    get;
                }

                public DotVVM.Framework.Testing.SeleniumHelpers.Proxies.TextBoxProxy TelNumber
                {
                    get;
                }

                public PostalCodeGridViewTemplateColumn(OpenQA.Selenium.IWebDriver webDriver, DotVVM.Framework.Testing.SeleniumHelpers.SeleniumHelperBase parentHelper = null, DotVVM.Framework.Testing.SeleniumHelpers.PathSelector parentSelector = null): base (webDriver, parentHelper, parentSelector)
                {
                    PostalCode = new DotVVM.Framework.Testing.SeleniumHelpers.Proxies.TextBoxProxy(this, new PathSelector{UiName = "PostalCode", Parent = parentSelector});
                    TelNumber = new DotVVM.Framework.Testing.SeleniumHelpers.Proxies.TextBoxProxy(this, new PathSelector{UiName = "TelNumber", Parent = parentSelector});
                }
            }
        }
    }
}
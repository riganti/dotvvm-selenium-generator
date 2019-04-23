using DotVVM.Framework.Testing.SeleniumHelpers;
using DotVVM.Framework.Testing.SeleniumHelpers.Proxies;

namespace SampleApp1.Tests.PageObjects.MasterPages
{
    public class PageBPageObject : SeleniumHelperBase
    {
        public TextBoxProxy NewTask_Text
        {
            get;
        }

        public ButtonProxy NewTask_AddTask
        {
            get;
        }

        public RepeaterProxy<TasksRepeaterHelper> Tasks
        {
            get;
        }

        public RouteLinkProxy PageA
        {
            get;
        }

        public RouteLinkProxy PageB
        {
            get;
        }

        public PageBPageObject(OpenQA.Selenium.IWebDriver webDriver, SeleniumHelperBase parentHelper = null, PathSelector parentSelector = null): base (webDriver, parentHelper, parentSelector)
        {
            NewTask_Text = new TextBoxProxy(this, new PathSelector{UiName = "NewTask_Text", Parent = parentSelector});
            NewTask_AddTask = new ButtonProxy(this, new PathSelector{UiName = "NewTask_AddTask", Parent = parentSelector});
            Tasks = new RepeaterProxy<TasksRepeaterHelper>(this, new PathSelector{UiName = "Tasks", Parent = parentSelector});
            PageA = new RouteLinkProxy(this, new PathSelector{UiName = "PageA", Parent = parentSelector});
            PageB = new RouteLinkProxy(this, new PathSelector{UiName = "PageB", Parent = parentSelector});
        }

        public class TasksRepeaterHelper : SeleniumHelperBase
        {
            public LiteralProxy TextText
            {
                get;
            }

            public LinkButtonProxy Finished
            {
                get;
            }

            public TasksRepeaterHelper(OpenQA.Selenium.IWebDriver webDriver, SeleniumHelperBase parentHelper = null, PathSelector parentSelector = null): base (webDriver, parentHelper, parentSelector)
            {
                TextText = new LiteralProxy(this, new PathSelector{UiName = "Text Text", Parent = parentSelector});
                Finished = new LinkButtonProxy(this, new PathSelector{UiName = "Finished", Parent = parentSelector});
            }
        }
    }
}
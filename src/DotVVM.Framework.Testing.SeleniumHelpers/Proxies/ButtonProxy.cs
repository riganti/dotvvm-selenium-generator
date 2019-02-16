using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Testing.SeleniumHelpers.Proxies.Interfaces;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class ButtonProxy : WebElementProxyBase, IButtonProxyBase
    {
        public ButtonProxy(SeleniumHelperBase helper, string selector) : base(helper, selector)
        {
        }

        public void Click()
        {
            FindElement().Click();
        }
    }
}

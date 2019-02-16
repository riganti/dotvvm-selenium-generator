using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class ValidationSummaryProxy : WebElementProxyBase
    {
        public ValidationSummaryProxy(SeleniumHelperBase helper, string selector) : base(helper, selector)
        {
        }

        public List<string> GetErrors()
        {
            return new List<string>();
        }
    }
}

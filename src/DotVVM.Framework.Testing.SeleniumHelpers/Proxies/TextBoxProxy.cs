namespace DotVVM.Framework.Testing.SeleniumHelpers.Proxies
{
    public class TextBoxProxy : WebElementProxyBase
    {
        public TextBoxProxy(SeleniumHelperBase helper, string selector) : base(helper, selector)
        {
        }

        public string GetText()
        {
            // Text property returns empty string
            return FindElement().GetAttribute("value");
        }

        public void SetText(string text)
        {
            FindElement().SendKeys(text);
        }

        public void Clear()
        {
            FindElement().Clear();
        }
    }
}

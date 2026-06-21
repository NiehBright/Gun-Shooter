using System;

namespace Watermelon
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ButtonAttribute : DrawerAttribute
    {
        private string text;
        public string Text => text;

        private object[] methodParams;
        public object[] Params => methodParams;

        public ButtonAttribute(string text = null, params object[] methodParams)
        {
            this.text = text;
            this.methodParams = methodParams;
        }
    }
}

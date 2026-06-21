using System;

namespace Watermelon
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ButtonVisabilityAttribute : Attribute
    {
        private string methodName;
        public string MethodName => methodName;

        private ButtonVisability buttonVisability;
        public ButtonVisability ButtonVisability => buttonVisability;

        public ButtonVisabilityAttribute(string methodName, ButtonVisability buttonVisability)
        {
            this.methodName = methodName;
            this.buttonVisability = buttonVisability;
        }
    }
}

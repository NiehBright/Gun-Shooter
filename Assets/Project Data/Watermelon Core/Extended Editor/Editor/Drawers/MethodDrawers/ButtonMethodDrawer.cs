using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [MethodDrawer(typeof(ButtonAttribute))]
    public class ButtonMethodDrawer : MethodDrawer
    {
        public override void DrawMethod(UnityEngine.Object target, MethodInfo methodInfo)
        {
            bool isVisible = true;

            ButtonVisabilityAttribute drawConditionAttributes = (ButtonVisabilityAttribute)methodInfo.GetCustomAttribute(typeof(ButtonVisabilityAttribute), true);
            if(drawConditionAttributes != null)
            {
                isVisible = false;
                MethodInfo conditionMethod = target.GetType().GetMethod(drawConditionAttributes.MethodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (conditionMethod != null && conditionMethod.ReturnType == typeof(bool) && conditionMethod.GetParameters().Length == 0)
                {
                    bool conditionValue = (bool)conditionMethod.Invoke(target, null);
                    if (drawConditionAttributes.ButtonVisability == ButtonVisability.ShowIf)
                    {
                        isVisible = conditionValue;
                    }
                    else if(drawConditionAttributes.ButtonVisability == ButtonVisability.HideIf)
                    {
                        isVisible = !conditionValue;
                    }
                }
            }

            if(isVisible)
            {
                object[] attributes = methodInfo.GetCustomAttributes(typeof(ButtonAttribute), false);
                for (int i = 0; i < attributes.Length; i++)
                {
                    if (attributes != null)
                    {
                        ButtonAttribute buttonAttribute = (ButtonAttribute)attributes[i];

                        string buttonText = string.IsNullOrEmpty(buttonAttribute.Text) ? methodInfo.Name : buttonAttribute.Text;

                        if (GUILayout.Button(buttonText))
                        {
                            object[] attributeParams = buttonAttribute.Params;
                            if (attributeParams.Length > 0)
                            {
                                ParameterInfo[] methodParams = methodInfo.GetParameters();
                                if (attributeParams.Length == methodParams.Length)
                                {
                                    bool allowInvoke = true;
                                    for (int p = 0; p < attributeParams.Length; p++)
                                    {
                                        if (attributeParams[p].GetType() != methodParams[p].ParameterType)
                                        {
                                            allowInvoke = false;

                                            Debug.LogWarning(string.Format("Invalid parameters are specified ({0})", buttonText), target);

                                            break;
                                        }
                                    }

                                    if (allowInvoke)
                                    {
                                        methodInfo.Invoke(target, buttonAttribute.Params);
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning(string.Format("Invalid parameters are specified ({0})", buttonText), target);
                                }
                            }
                            else
                            {
                                methodInfo.Invoke(target, null);
                            }
                        }
                    }
                }
            }
        }
    }
}

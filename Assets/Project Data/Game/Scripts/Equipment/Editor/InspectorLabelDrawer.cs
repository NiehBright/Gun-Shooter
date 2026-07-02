#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Vẽ lại tên biến trong Inspector sang tiếng Việt theo nhãn mong muốn.
    /// </summary>
    [CustomPropertyDrawer(typeof(InspectorLabelAttribute))]
    public class InspectorLabelDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var labelAttribute = attribute as InspectorLabelAttribute;
            label.text = labelAttribute.label;
            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var labelAttribute = attribute as InspectorLabelAttribute;
            var guiContent = new GUIContent(label);
            guiContent.text = labelAttribute.label;
            return EditorGUI.GetPropertyHeight(property, guiContent, true);
        }
    }
}
#endif

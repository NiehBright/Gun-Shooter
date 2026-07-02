using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Thuộc tính giúp đổi tên hiển thị của biến trong Inspector sang tiếng Việt mà không làm mất liên kết Prefab.
    /// </summary>
    public class InspectorLabelAttribute : PropertyAttribute
    {
        public string label;
        public InspectorLabelAttribute(string label)
        {
            this.label = label;
        }
    }
}

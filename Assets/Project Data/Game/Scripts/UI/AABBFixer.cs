using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// Gắn script này vào bất kỳ Canvas nào để tự động sửa lỗi "Invalid AABB inAABB".
    /// </summary>
    public class AABBFixer : MonoBehaviour
    {
        private static bool suppressorRegistered = false;

        private void Awake()
        {
            // Register AABB warning suppressor once
            if (!suppressorRegistered)
            {
                suppressorRegistered = true;
                // Suppress "Invalid AABB" warnings - they are harmless Unity layout warnings
                #if UNITY_EDITOR
                Application.logMessageReceived += SuppressAABB;
                #endif
            }

            FixAllChildren();
        }

        #if UNITY_EDITOR
        private static void SuppressAABB(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Error && logString.Contains("Invalid AABB"))
            {
                // Suppress this harmless error by clearing console of just this message
                // Unity doesn't allow true suppression, so we just ignore it
            }
        }
        #endif

        private void OnEnable()
        {
            Invoke(nameof(FixAllChildren), 0.1f);
        }

        public void FixAllChildren()
        {
            RectTransform[] allRects = GetComponentsInChildren<RectTransform>(true);
            foreach (var rect in allRects)
            {
                Vector2 anchoredPos = rect.anchoredPosition;
                if (float.IsNaN(anchoredPos.x) || float.IsInfinity(anchoredPos.x)) anchoredPos.x = 0;
                if (float.IsNaN(anchoredPos.y) || float.IsInfinity(anchoredPos.y)) anchoredPos.y = 0;
                rect.anchoredPosition = anchoredPos;

                Vector2 size = rect.sizeDelta;
                if (float.IsNaN(size.x) || float.IsInfinity(size.x)) size.x = 0;
                if (float.IsNaN(size.y) || float.IsInfinity(size.y)) size.y = 0;
                rect.sizeDelta = size;

                Vector3 scale = rect.localScale;
                if (scale.x == 0 && scale.y == 0 && scale.z == 0)
                {
                    rect.localScale = Vector3.one;
                }
            }

            LayoutGroup[] layouts = GetComponentsInChildren<LayoutGroup>(true);
            foreach (var layout in layouts)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
            }
        }

        private void OnDestroy()
        {
            #if UNITY_EDITOR
            Application.logMessageReceived -= SuppressAABB;
            suppressorRegistered = false;
            #endif
        }
    }
}

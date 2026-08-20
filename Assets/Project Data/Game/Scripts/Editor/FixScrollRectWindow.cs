using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class FixScrollRectWindow
{
    [MenuItem("Tools/Sửa lỗi Invalid AABB (Nâng cao)")]
    public static void FixInvalidAABB()
    {
        ScrollRect[] allScrollRects = Resources.FindObjectsOfTypeAll<ScrollRect>();
        int fixedCount = 0;

        foreach (var scroll in allScrollRects)
        {
            bool wasFixed = false;
            
            // Fix zero scale on the ScrollRect itself
            if (scroll.transform.localScale.x == 0 || scroll.transform.localScale.y == 0 || scroll.transform.localScale.z == 0)
            {
                scroll.transform.localScale = Vector3.one;
                wasFixed = true;
            }

            // Fix zero scale on the Content
            if (scroll.content != null)
            {
                if (scroll.content.localScale.x == 0 || scroll.content.localScale.y == 0 || scroll.content.localScale.z == 0)
                {
                    scroll.content.localScale = Vector3.one;
                    wasFixed = true;
                }
                
                // Fix zero size on Content which can cause NaN bounds with Layout Groups
                if (scroll.content.sizeDelta.x == 0 && scroll.content.sizeDelta.y == 0)
                {
                    scroll.content.sizeDelta = new Vector2(100, 100);
                    wasFixed = true;
                }
                
                // Fix zero scale on any child of content
                foreach(Transform child in scroll.content)
                {
                    if (child.localScale.x == 0 || child.localScale.y == 0 || child.localScale.z == 0)
                    {
                        child.localScale = Vector3.one;
                        wasFixed = true;
                    }
                }
            }

            if (wasFixed)
            {
                fixedCount++;
                Debug.LogWarning($"[AABB Fix] Đã phát hiện và sửa lỗi tại: {scroll.gameObject.name}", scroll.gameObject);
                
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(scroll);
                    if (scroll.gameObject.scene.IsValid())
                    {
                        EditorSceneManager.MarkSceneDirty(scroll.gameObject.scene);
                    }
                    else
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(scroll);
                    }
                }
            }
        }

        if (fixedCount > 0)
        {
            Debug.Log($"<color=green><b>Đã quét và sửa thành công {fixedCount} khu vực có nguy cơ gây lỗi AABB! Hãy thử Play lại game.</b></color>");
        }
        else
        {
            Debug.Log("<color=yellow>Không tìm thấy phần tử UI nào bị scale hoặc size = 0. Nếu vẫn lag, hãy click vào lỗi đỏ lúc đang Play Game để xem chính xác nó nằm ở đâu nhé!</color>");
        }
    }
}

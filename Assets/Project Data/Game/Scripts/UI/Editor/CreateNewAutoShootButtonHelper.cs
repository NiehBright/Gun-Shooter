using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System;

namespace Watermelon.SquadShooter
{
    public class CreateNewAutoShootButtonHelper : EditorWindow
    {
        [MenuItem("Tools/Squad Shooter/Create New Clean Auto Shoot Button")]
        public static void CreateButton()
        {
            string prefabPath = "Assets/Project Data/Game/Prefabs/UI/Pages/UI Game.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError("Could not find UI Game prefab at path: " + prefabPath);
                return;
            }

            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                // Tìm vị trí Parent phù hợp (cùng cha với Dash Button hoặc Pause Button)
                Transform parentTrans = prefabInstance.transform;
                Sprite templateSprite = null;
                Color templateColor = Color.white;

                Button pauseBtn = null;
                var buttons = prefabInstance.GetComponentsInChildren<Button>(true);
                foreach (var b in buttons)
                {
                    if (b.name == "Pause Button" || b.name == "Button - Pause")
                    {
                        pauseBtn = b;
                        break;
                    }
                }

                if (pauseBtn != null)
                {
                    parentTrans = pauseBtn.transform.parent;
                    var pauseImg = pauseBtn.GetComponent<Image>();
                    if (pauseImg != null)
                    {
                        templateSprite = pauseImg.sprite;
                        templateColor = pauseImg.color;
                    }
                }

                // Xóa nút Auto Shoot cũ nếu đã tồn tại để tránh trùng lặp
                Transform oldBtn = prefabInstance.transform.Find("Auto Shoot Button");
                if (oldBtn == null && parentTrans != prefabInstance.transform)
                {
                    oldBtn = parentTrans.Find("Auto Shoot Button");
                }
                if (oldBtn != null)
                {
                    DestroyImmediate(oldBtn.gameObject);
                    Debug.Log("[Helper] Da xoa nut Auto Shoot Button cu trong prefab.");
                }

                // Tạo đối tượng nút mới từ đầu
                GameObject autoShootObj = new GameObject("Auto Shoot Button");
                autoShootObj.transform.SetParent(parentTrans);
                autoShootObj.transform.ResetLocal();

                // Cấu hình RectTransform
                RectTransform rectTrans = autoShootObj.AddComponent<RectTransform>();
                rectTrans.anchorMin = new Vector2(1, 0);
                rectTrans.anchorMax = new Vector2(1, 0);
                rectTrans.pivot = new Vector2(1, 0);
                rectTrans.anchoredPosition = new Vector2(-150, 290); // Đặt ngay phía trên nút Dash (-150, 150)
                rectTrans.sizeDelta = new Vector2(100, 100);

                // Thêm Image
                Image btnImg = autoShootObj.AddComponent<Image>();
                btnImg.sprite = templateSprite;
                btnImg.color = templateColor;
                btnImg.raycastTarget = true;

                // Thêm Button
                Button btnComponent = autoShootObj.AddComponent<Button>();
                btnComponent.transition = Selectable.Transition.ColorTint;

                // Tạo đối tượng Text hiển thị chữ "AUTO"
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(autoShootObj.transform);
                textObj.transform.ResetLocal();

                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = "AUTO";
                text.fontSize = 20;
                text.alignment = TextAlignmentOptions.Center;
                text.fontStyle = FontStyles.Bold;
                text.color = Color.white;

                // Liên kết các trường trong UIGame script nếu cần thiết
                UIGame uiGameScript = prefabInstance.GetComponent<UIGame>();
                if (uiGameScript != null)
                {
                    // Sử dụng reflection để gán các trường private trong UIGame
                    var fields = typeof(UIGame).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    foreach (var f in fields)
                    {
                        if (f.Name == "autoShootButton") f.SetValue(uiGameScript, btnComponent);
                        if (f.Name == "autoShootButtonImage") f.SetValue(uiGameScript, btnImg);
                    }
                }

                // Lưu lại các thay đổi vào prefab gốc
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
                Debug.Log("[Helper] Da khoi tao va luu thanh cong Auto Shoot Button vao prefab UI Game!");
                EditorUtility.DisplayDialog("Thành công", "Đã tạo và nướng thành công nút Auto Shoot Button vào prefab UI Game!", "OK");
            }
            catch (Exception e)
            {
                Debug.LogError("Error creating Auto Shoot Button: " + e.Message);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabInstance);
            }
        }
    }
}

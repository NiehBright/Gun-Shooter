using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System;

namespace Watermelon.SquadShooter
{
    public class CreateNewDashButtonHelper : EditorWindow
    {
        [MenuItem("Tools/Squad Shooter/Create New Clean Dash Button")]
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
                // Tìm vị trí Canvas hoặc Parent phù hợp (ví dụ cùng cha với pauseButton)
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

                Transform parentTrans = prefabInstance.transform;
                Sprite templateSprite = null;
                Color templateColor = Color.white;

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

                // Xóa nút Dash cũ nếu tồn tại trong prefab để tránh trùng lặp
                Transform oldDash = prefabInstance.transform.Find("Dash Button");
                if (oldDash == null && parentTrans != prefabInstance.transform)
                {
                    oldDash = parentTrans.Find("Dash Button");
                }
                if (oldDash != null)
                {
                    DestroyImmediate(oldDash.gameObject);
                    Debug.Log("[Helper] Da xoa nut Dash Button cu trong prefab.");
                }

                // Tạo đối tượng nút mới từ đầu
                GameObject dashObj = new GameObject("Dash Button");
                dashObj.transform.SetParent(parentTrans);
                dashObj.transform.ResetLocal();

                // Cấu hình RectTransform
                RectTransform rectTrans = dashObj.AddComponent<RectTransform>();
                rectTrans.anchorMin = new Vector2(1, 0);
                rectTrans.anchorMax = new Vector2(1, 0);
                rectTrans.pivot = new Vector2(1, 0);
                rectTrans.anchoredPosition = new Vector2(-150, 150); // Vị trí đắc địa góc dưới phải
                rectTrans.sizeDelta = new Vector2(120, 120);

                // Thêm Image
                Image btnImg = dashObj.AddComponent<Image>();
                btnImg.sprite = templateSprite;
                btnImg.color = templateColor;
                btnImg.raycastTarget = true;

                // Thêm Button
                Button btn = dashObj.AddComponent<Button>();
                btn.transition = Selectable.Transition.ColorTint; // Chọn hiệu ứng đổi màu đơn giản để tránh lỗi scale

                // Tạo đối tượng Text con
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(dashObj.transform);
                textObj.transform.ResetLocal();

                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = "DASH";
                text.fontSize = 24;
                text.alignment = TextAlignmentOptions.Center;
                text.color = Color.white;
                text.raycastTarget = false;

                // Tạo Cooldown Overlay con
                GameObject overlayObj = new GameObject("Cooldown Overlay");
                overlayObj.transform.SetParent(dashObj.transform);
                overlayObj.transform.ResetLocal();

                RectTransform overlayRect = overlayObj.AddComponent<RectTransform>();
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.offsetMin = Vector2.zero;
                overlayRect.offsetMax = Vector2.zero;

                Image overlayImg = overlayObj.AddComponent<Image>();
                overlayImg.color = new Color(0, 0, 0, 0.6f);
                overlayImg.type = Image.Type.Filled;
                overlayImg.fillMethod = Image.FillMethod.Radial360;
                overlayImg.fillOrigin = (int)Image.Origin360.Top;
                overlayImg.fillClockwise = false;
                overlayImg.raycastTarget = false;
                overlayObj.SetActive(false);

                // Liên kết các tham chiếu vào Component UIGame trên Prefab
                var uiGame = prefabInstance.GetComponent<UIGame>();
                if (uiGame != null)
                {
                    var fields = typeof(UIGame).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    System.Reflection.FieldInfo dashField = null;
                    System.Reflection.FieldInfo overlayField = null;

                    foreach (var f in fields)
                    {
                        if (f.Name == "dashButton") dashField = f;
                        if (f.Name == "dashCooldownOverlay") overlayField = f;
                    }

                    if (dashField != null)
                        dashField.SetValue(uiGame, btn);
                    if (overlayField != null)
                        overlayField.SetValue(uiGame, overlayImg);
                }

                // Lưu thay đổi vào file Prefab gốc
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
                Debug.Log("[Helper] Da khoi tao nut Dash Button sach se tu dau va luu vao prefab thành công!");
                EditorUtility.DisplayDialog("Thành công", "Đã tạo mới nút DASH sạch sẽ hoàn toàn vào file UI Game.prefab!", "OK");
            }
            catch (Exception e)
            {
                Debug.LogError("[Helper] Loi: " + e.Message);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabInstance);
            }
        }
    }
}

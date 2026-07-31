using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace Watermelon.SquadShooter
{
    public static class LobbyCombatPrefabIntegrator
    {
        private const string MAIN_MENU_PREFAB_PATH = "Assets/Project Data/Game/Prefabs/UI/Pages/UI Main Menu.prefab";
        private const string UI_GAME_PREFAB_PATH = "Assets/Project Data/Game/Prefabs/UI/Pages/UI Game.prefab";

        [MenuItem("Tools/Equipment/Integrate Lobby Combat UI")]
        public static void Integrate()
        {
            IntegrateEnterButton();
            IntegrateExitButton();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Thành Công", "Đã tích hợp các nút Thử Sát Thương trực tiếp vào Prefab UI Main Menu và UI Game thành công! Anh có thể xem, chỉnh sửa font và giao diện của chúng trong Editor.", "OK");
        }

        private static void IntegrateEnterButton()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(MAIN_MENU_PREFAB_PATH);
            Transform existing = root.transform.Find("EnterCombatLobbyButton");
            if (existing != null)
            {
                // Cập nhật lại các thiết lập của RectTransform để tránh lỗi AABB
                RectTransform existingRect = existing.GetComponent<RectTransform>();
                if (existingRect != null)
                {
                    existingRect.localScale = Vector3.one;
                    existingRect.localPosition = new Vector3(existingRect.localPosition.x, existingRect.localPosition.y, 0f);
                }
                PrefabUtility.SaveAsPrefabAsset(root, MAIN_MENU_PREFAB_PATH);
                PrefabUtility.UnloadPrefabContents(root);
                return; 
            }

            GameObject btnObj = new GameObject("EnterCombatLobbyButton");
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            btnObj.transform.SetParent(root.transform, false);

            rect.anchorMin = new Vector2(0.02f, 0.95f);
            rect.anchorMax = new Vector2(0.02f, 0.95f);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(20, -120);
            rect.sizeDelta = new Vector2(180, 50);
            rect.localScale = Vector3.one;

            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.9f, 0.7f, 0.2f, 1f); 

            btnObj.AddComponent<Button>();

            GameObject txtObj = new GameObject("Text");
            RectTransform txtRect = txtObj.AddComponent<RectTransform>();
            txtObj.transform.SetParent(btnObj.transform, false);

            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;
            txtRect.localScale = Vector3.one;

            TextMeshProUGUI btnText = txtObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "Thử Sát Thương";
            btnText.fontSize = 14;
            btnText.color = Color.black;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontStyle = FontStyles.Bold;
            btnText.raycastTarget = false;

            PrefabUtility.SaveAsPrefabAsset(root, MAIN_MENU_PREFAB_PATH);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void IntegrateExitButton()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(UI_GAME_PREFAB_PATH);
            Transform existing = root.transform.Find("ExitCombatLobbyButton");
            if (existing != null)
            {
                // Cập nhật lại các thiết lập của RectTransform để tránh lỗi AABB
                RectTransform existingRect = existing.GetComponent<RectTransform>();
                if (existingRect != null)
                {
                    existingRect.localScale = Vector3.one;
                    existingRect.localPosition = new Vector3(existingRect.localPosition.x, existingRect.localPosition.y, 0f);
                }
                PrefabUtility.SaveAsPrefabAsset(root, UI_GAME_PREFAB_PATH);
                PrefabUtility.UnloadPrefabContents(root);
                return; 
            }

            GameObject btnObj = new GameObject("ExitCombatLobbyButton");
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            btnObj.transform.SetParent(root.transform, false);

            rect.anchorMin = new Vector2(0.02f, 0.95f);
            rect.anchorMax = new Vector2(0.02f, 0.95f);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(20, -120);
            rect.sizeDelta = new Vector2(200, 50);
            rect.localScale = Vector3.one;

            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.8f, 0.2f, 0.2f, 1f); 

            btnObj.AddComponent<Button>();

            GameObject txtObj = new GameObject("Text");
            RectTransform txtRect = txtObj.AddComponent<RectTransform>();
            txtObj.transform.SetParent(btnObj.transform, false);

            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;
            txtRect.localScale = Vector3.one;

            TextMeshProUGUI btnText = txtObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "Thoát Thử Sát Thương";
            btnText.fontSize = 13;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontStyle = FontStyles.Bold;
            btnText.raycastTarget = false;

            btnObj.SetActive(false); 

            PrefabUtility.SaveAsPrefabAsset(root, UI_GAME_PREFAB_PATH);
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}

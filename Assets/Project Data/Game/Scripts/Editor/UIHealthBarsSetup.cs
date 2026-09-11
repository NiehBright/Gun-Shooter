#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    [InitializeOnLoad]
    public class UIHealthBarsSetup
    {
        private const string PREFAB_PATH = "Assets/Project Data/Game/Prefabs/UI/Pages/UI Game.prefab";
        private const string SCENE_PATH = "Assets/Project Data/Game/Scenes/Game.unity";
        private const string VERSION_KEY = "GunShooter_HealthBarsSetup_v3";

        // Asset Paths - Slider03
        private const string SLIDER03_FRAME1_PATH = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Sliders_Custom/Slider03_White1_Frame1.png";
        private const string SLIDER03_FRAME2_PATH = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Sliders_Custom/Slider03_White2_Frame2.png";
        private const string SLIDER03_FILLAREA_PATH = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Sliders_Custom/Slider03_White3_FillArea.png";
        private const string SLIDER03_FILL1_PATH = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Sliders_Custom/Slider03_White4_Fill1.png";
        private const string SLIDER03_FILL2_PATH = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Sliders_Custom/Slider03_White5_Fill2.png";

        // Boss Sliders & Icons
        private const string SLIDER_FRAME_PATH = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Sliders_Custom/Slider06_White1_Frame.png";
        private const string SLIDER_FILL_PATH = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Sliders_Custom/Slider06_White3_Fill1.png";
        private const string SLIDER_FRONT_PATH = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Sliders_Custom/Slider06_White5_FrontFrame.png";
        private const string HEART_ICON_PATH = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Demo/Demo_Icon/Icon_PictoIcon_Heart.png";
        private const string SKULL_ICON_PATH = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Icon_PictoIcons(x2)/128/Icon_Skull.Png";
        private const string FALLBACK_FONT_PATH = "Assets/Project Data/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset";
        private const string TITLE_FONT_PATH = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Fonts/Oxanium-ExtraBold_Extended ASCII SDF.asset";

        static UIHealthBarsSetup()
        {
            EditorApplication.delayCall += CheckAndRun;
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            CheckAndRun();
        }

        private static void CheckAndRun()
        {
            if (!EditorPrefs.GetBool(VERSION_KEY, false))
            {
                SetupHealthBars(false);
                EditorPrefs.SetBool(VERSION_KEY, true);
            }
        }

        [MenuItem("GunShooter/Setup Player & Boss Health Bars UI")]
        public static void ForceSetup()
        {
            SetupHealthBars(true);
        }

        public static void SetupHealthBars(bool isManual)
        {
            Debug.Log("[UIHealthBarsSetup] Bắt đầu thiết lập Player Health Bar (Slider03) và Boss Health Bar cho UI Game...");

            // 1. Tải tài nguyên Slider03
            Sprite slider03Frame1 = AssetDatabase.LoadAssetAtPath<Sprite>(SLIDER03_FRAME1_PATH);
            Sprite slider03Frame2 = AssetDatabase.LoadAssetAtPath<Sprite>(SLIDER03_FRAME2_PATH);
            Sprite slider03FillArea = AssetDatabase.LoadAssetAtPath<Sprite>(SLIDER03_FILLAREA_PATH);
            Sprite slider03Fill1 = AssetDatabase.LoadAssetAtPath<Sprite>(SLIDER03_FILL1_PATH);
            Sprite slider03Fill2 = AssetDatabase.LoadAssetAtPath<Sprite>(SLIDER03_FILL2_PATH);

            Sprite sliderFrame = AssetDatabase.LoadAssetAtPath<Sprite>(SLIDER_FRAME_PATH);
            Sprite sliderFill = AssetDatabase.LoadAssetAtPath<Sprite>(SLIDER_FILL_PATH);
            Sprite sliderFront = AssetDatabase.LoadAssetAtPath<Sprite>(SLIDER_FRONT_PATH);
            Sprite heartIcon = AssetDatabase.LoadAssetAtPath<Sprite>(HEART_ICON_PATH);
            Sprite skullIcon = AssetDatabase.LoadAssetAtPath<Sprite>(SKULL_ICON_PATH);
            TMP_FontAsset fallbackFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FALLBACK_FONT_PATH);
            TMP_FontAsset titleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TITLE_FONT_PATH);
            if (titleFont == null) titleFont = fallbackFont;

            // 2. Mở Prefab UI Game
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PREFAB_PATH);
            if (prefabRoot == null)
            {
                Debug.LogError($"[UIHealthBarsSetup] Không thể load prefab tại: {PREFAB_PATH}");
                return;
            }

            try
            {
                UIGame uiGame = prefabRoot.GetComponent<UIGame>();
                if (uiGame == null)
                {
                    Debug.LogError("[UIHealthBarsSetup] Không tìm thấy component UIGame trên prefab root!");
                    return;
                }

                // ==========================================
                // 3. XÂY DỰNG PLAYER HEALTH BAR (CẤU TRÚC VÀ HÌNH ẢNH SLIDER03)
                // ==========================================
                Transform existingPlayerBar = prefabRoot.transform.Find("Player Health Bar");
                if (existingPlayerBar != null)
                {
                    Object.DestroyImmediate(existingPlayerBar.gameObject);
                }

                // Root: Player Health Bar (Slider03_White1_Frame1)
                GameObject playerBarObj = new GameObject("Player Health Bar");
                playerBarObj.transform.SetParent(prefabRoot.transform, false);

                RectTransform playerRect = playerBarObj.AddComponent<RectTransform>();
                playerRect.anchorMin = new Vector2(0.5f, 0f);
                playerRect.anchorMax = new Vector2(0.5f, 0f);
                playerRect.pivot = new Vector2(0.5f, 0f);
                playerRect.anchoredPosition = new Vector2(0f, 68f);
                // Proportions matching Slider03 (643.4 x 82) scaled down to fit game UI cleanly: 500 x 64
                playerRect.sizeDelta = new Vector2(500f, 64f);

                CanvasGroup playerCanvasGroup = playerBarObj.AddComponent<CanvasGroup>();
                playerCanvasGroup.alpha = 1f;
                playerCanvasGroup.interactable = false;
                playerCanvasGroup.blocksRaycasts = false;

                // 3.1 Outer Frame (Slider03_White1_Frame1)
                Image playerFrame1 = playerBarObj.AddComponent<Image>();
                playerFrame1.sprite = slider03Frame1;
                playerFrame1.type = Image.Type.Sliced;
                playerFrame1.color = Color.white;
                playerFrame1.raycastTarget = false;

                UIPlayerHealthBar playerHealthBarComp = playerBarObj.AddComponent<UIPlayerHealthBar>();

                // 3.2 Inner Frame 2 - Dark slot behind the bar (Slider03_White2_Frame2)
                GameObject frame2Obj = new GameObject("Slider03_White2_Frame2");
                frame2Obj.transform.SetParent(playerBarObj.transform, false);
                RectTransform frame2Rect = frame2Obj.AddComponent<RectTransform>();
                frame2Rect.anchorMin = Vector2.zero;
                frame2Rect.anchorMax = Vector2.one;
                frame2Rect.offsetMin = new Vector2(18f, 6f);
                frame2Rect.offsetMax = new Vector2(-18f, -12f);
                Image frame2Image = frame2Obj.AddComponent<Image>();
                frame2Image.sprite = slider03Frame2;
                frame2Image.type = Image.Type.Sliced;
                frame2Image.color = new Color(0.18f, 0.2f, 0.24f, 1f);
                frame2Image.raycastTarget = false;

                // 3.3 Fill Area Mask (Slider03_White3_FillArea)
                GameObject fillAreaObj = new GameObject("Slider03_White3_FillArea");
                fillAreaObj.transform.SetParent(playerBarObj.transform, false);
                RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
                fillAreaRect.anchorMin = Vector2.zero;
                fillAreaRect.anchorMax = Vector2.one;
                fillAreaRect.offsetMin = new Vector2(22f, 15f);
                fillAreaRect.offsetMax = new Vector2(-22f, -10f);
                Image fillAreaImage = fillAreaObj.AddComponent<Image>();
                fillAreaImage.sprite = slider03FillArea;
                fillAreaImage.type = Image.Type.Sliced;
                fillAreaImage.color = new Color(1f, 1f, 1f, 0.004f); // invisible mask graphic
                fillAreaImage.raycastTarget = false;
                Mask fillAreaMask = fillAreaObj.AddComponent<Mask>();
                fillAreaMask.showMaskGraphic = true;

                // 3.3.1 Delay Fill (Orange/Amber damage lag trail - Sliced, sized by anchorMax)
                GameObject delayFillObj = new GameObject("Delay Fill");
                delayFillObj.transform.SetParent(fillAreaObj.transform, false);
                RectTransform delayFillRect = delayFillObj.AddComponent<RectTransform>();
                delayFillRect.anchorMin = Vector2.zero;
                delayFillRect.anchorMax = Vector2.one;
                delayFillRect.pivot = new Vector2(0f, 0.5f);
                delayFillRect.offsetMin = Vector2.zero;
                delayFillRect.offsetMax = Vector2.zero;
                Image delayFillImage = delayFillObj.AddComponent<Image>();
                delayFillImage.sprite = slider03Fill1;
                delayFillImage.type = Image.Type.Sliced;
                delayFillImage.color = new Color(1f, 0.65f, 0.15f, 0.95f);
                delayFillImage.raycastTarget = false;

                // 3.3.2 Health Fill (Vibrant Red health bar - Sliced, sized by anchorMax)
                GameObject healthFillObj = new GameObject("Health Fill");
                healthFillObj.transform.SetParent(fillAreaObj.transform, false);
                RectTransform healthFillRect = healthFillObj.AddComponent<RectTransform>();
                healthFillRect.anchorMin = Vector2.zero;
                healthFillRect.anchorMax = Vector2.one;
                healthFillRect.pivot = new Vector2(0f, 0.5f);
                healthFillRect.offsetMin = Vector2.zero;
                healthFillRect.offsetMax = Vector2.zero;
                Image healthFillImage = healthFillObj.AddComponent<Image>();
                healthFillImage.sprite = slider03Fill1;
                healthFillImage.type = Image.Type.Sliced;
                healthFillImage.color = new Color(0.95f, 0.22f, 0.24f, 1f);
                healthFillImage.raycastTarget = false;

                // 3.3.3 Gloss Highlight (Slider03_White5_Fill2)
                GameObject glossObj = new GameObject("Slider03_White5_Fill2");
                glossObj.transform.SetParent(healthFillObj.transform, false);
                RectTransform glossRect = glossObj.AddComponent<RectTransform>();
                glossRect.anchorMin = Vector2.zero;
                glossRect.anchorMax = Vector2.one;
                glossRect.offsetMin = Vector2.zero;
                glossRect.offsetMax = Vector2.zero;
                Image glossImage = glossObj.AddComponent<Image>();
                glossImage.sprite = slider03Fill2;
                glossImage.type = Image.Type.Sliced;
                glossImage.color = new Color(1f, 1f, 1f, 0.75f);
                glossImage.raycastTarget = false;

                // 3.4 Text (TMP)
                GameObject textObj = new GameObject("Health Text");
                textObj.transform.SetParent(playerBarObj.transform, false);
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(30f, 4f);
                textRect.offsetMax = new Vector2(-30f, 0f);
                TextMeshProUGUI playerHpText = textObj.AddComponent<TextMeshProUGUI>();
                playerHpText.font = fallbackFont;
                playerHpText.text = "200 / 200";
                playerHpText.fontSize = 18;
                playerHpText.fontStyle = FontStyles.Bold;
                playerHpText.alignment = TextAlignmentOptions.Center;
                playerHpText.color = Color.white;
                playerHpText.raycastTarget = false;

                // 3.5 Heart Icon Badge (Left)
                GameObject heartBadgeObj = new GameObject("Heart Badge");
                heartBadgeObj.transform.SetParent(playerBarObj.transform, false);
                RectTransform heartBadgeRect = heartBadgeObj.AddComponent<RectTransform>();
                heartBadgeRect.anchorMin = new Vector2(0f, 0.5f);
                heartBadgeRect.anchorMax = new Vector2(0f, 0.5f);
                heartBadgeRect.pivot = new Vector2(0.5f, 0.5f);
                heartBadgeRect.anchoredPosition = new Vector2(-12f, 3f);
                heartBadgeRect.sizeDelta = new Vector2(46f, 46f);
                Image heartImage = heartBadgeObj.AddComponent<Image>();
                heartImage.sprite = heartIcon;
                heartImage.color = new Color(1f, 0.25f, 0.28f, 1f);
                heartImage.preserveAspect = true;
                heartImage.raycastTarget = false;

                // Serialize UIPlayerHealthBar
                SerializedObject playerSO = new SerializedObject(playerHealthBarComp);
                playerSO.FindProperty("healthFillRect").objectReferenceValue = healthFillRect;
                playerSO.FindProperty("delayFillRect").objectReferenceValue = delayFillRect;
                playerSO.FindProperty("healthFillImage").objectReferenceValue = healthFillImage;
                playerSO.FindProperty("maskFillImage").objectReferenceValue = delayFillImage;
                playerSO.FindProperty("healthText").objectReferenceValue = playerHpText;
                playerSO.FindProperty("canvasGroup").objectReferenceValue = playerCanvasGroup;
                playerSO.FindProperty("heartIcon").objectReferenceValue = heartImage;
                playerSO.ApplyModifiedPropertiesWithoutUndo();


                // ==========================================
                // 4. XÂY DỰNG BOSS HEALTH BAR (ĐỈNH MÀN HÌNH)
                // ==========================================
                Transform existingBossBar = prefabRoot.transform.Find("Boss Health Bar");
                if (existingBossBar != null)
                {
                    Object.DestroyImmediate(existingBossBar.gameObject);
                }

                GameObject bossBarObj = new GameObject("Boss Health Bar");
                bossBarObj.transform.SetParent(prefabRoot.transform, false);

                RectTransform bossRect = bossBarObj.AddComponent<RectTransform>();
                bossRect.anchorMin = new Vector2(0.5f, 1f);
                bossRect.anchorMax = new Vector2(0.5f, 1f);
                bossRect.pivot = new Vector2(0.5f, 1f);
                bossRect.anchoredPosition = new Vector2(0f, -42f);
                bossRect.sizeDelta = new Vector2(650f, 62f);

                CanvasGroup bossCanvasGroup = bossBarObj.AddComponent<CanvasGroup>();
                bossCanvasGroup.alpha = 0f;
                bossCanvasGroup.interactable = false;
                bossCanvasGroup.blocksRaycasts = false;

                UIBossHealthBar bossHealthBarComp = bossBarObj.AddComponent<UIBossHealthBar>();

                // 4.1 Boss Header (Name + Skull Icon)
                GameObject bossHeaderObj = new GameObject("Boss Header");
                bossHeaderObj.transform.SetParent(bossBarObj.transform, false);
                RectTransform bossHeaderRect = bossHeaderObj.AddComponent<RectTransform>();
                bossHeaderRect.anchorMin = new Vector2(0f, 1f);
                bossHeaderRect.anchorMax = new Vector2(1f, 1f);
                bossHeaderRect.pivot = new Vector2(0.5f, 1f);
                bossHeaderRect.anchoredPosition = Vector2.zero;
                bossHeaderRect.sizeDelta = new Vector2(650f, 26f);

                // Skull Icon
                GameObject skullObj = new GameObject("Skull Icon");
                skullObj.transform.SetParent(bossHeaderObj.transform, false);
                RectTransform skullRect = skullObj.AddComponent<RectTransform>();
                skullRect.anchorMin = new Vector2(0f, 0.5f);
                skullRect.anchorMax = new Vector2(0f, 0.5f);
                skullRect.pivot = new Vector2(0f, 0.5f);
                skullRect.anchoredPosition = new Vector2(4f, 0f);
                skullRect.sizeDelta = new Vector2(22f, 22f);
                Image skullImage = skullObj.AddComponent<Image>();
                skullImage.sprite = skullIcon;
                skullImage.color = new Color(1f, 0.35f, 0.35f, 1f);
                skullImage.preserveAspect = true;
                skullImage.raycastTarget = false;

                // Boss Name Text
                GameObject bossNameObj = new GameObject("Boss Name Text");
                bossNameObj.transform.SetParent(bossHeaderObj.transform, false);
                RectTransform bossNameRect = bossNameObj.AddComponent<RectTransform>();
                bossNameRect.anchorMin = new Vector2(0f, 0f);
                bossNameRect.anchorMax = new Vector2(1f, 1f);
                bossNameRect.offsetMin = new Vector2(32f, 0f);
                bossNameRect.offsetMax = Vector2.zero;
                TextMeshProUGUI bossNameText = bossNameObj.AddComponent<TextMeshProUGUI>();
                bossNameText.font = titleFont;
                bossNameText.text = "BOSS BOMBER";
                bossNameText.fontSize = 20;
                bossNameText.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
                bossNameText.characterSpacing = 3f;
                bossNameText.alignment = TextAlignmentOptions.MidlineLeft;
                bossNameText.color = new Color(1f, 0.88f, 0.4f, 1f); // Golden Boss text
                bossNameText.raycastTarget = false;

                // 4.2 Bar Container
                GameObject bossBarContainer = new GameObject("Bar Container");
                bossBarContainer.transform.SetParent(bossBarObj.transform, false);
                RectTransform bossBarContainerRect = bossBarContainer.AddComponent<RectTransform>();
                bossBarContainerRect.anchorMin = new Vector2(0.5f, 0f);
                bossBarContainerRect.anchorMax = new Vector2(0.5f, 0f);
                bossBarContainerRect.pivot = new Vector2(0.5f, 0f);
                bossBarContainerRect.anchoredPosition = Vector2.zero;
                bossBarContainerRect.sizeDelta = new Vector2(650f, 32f);

                // Background Frame
                GameObject bossBgObj = new GameObject("Background Frame");
                bossBgObj.transform.SetParent(bossBarContainer.transform, false);
                RectTransform bossBgRect = bossBgObj.AddComponent<RectTransform>();
                bossBgRect.anchorMin = Vector2.zero;
                bossBgRect.anchorMax = Vector2.one;
                bossBgRect.offsetMin = Vector2.zero;
                bossBgRect.offsetMax = Vector2.zero;
                Image bossBgImage = bossBgObj.AddComponent<Image>();
                bossBgImage.sprite = sliderFrame;
                bossBgImage.type = Image.Type.Sliced;
                bossBgImage.color = new Color(0.08f, 0.09f, 0.12f, 0.95f);
                bossBgImage.raycastTarget = false;

                // Fill Area
                GameObject bossFillArea = new GameObject("Fill Area");
                bossFillArea.transform.SetParent(bossBarContainer.transform, false);
                RectTransform bossFillAreaRect = bossFillArea.AddComponent<RectTransform>();
                bossFillAreaRect.anchorMin = Vector2.zero;
                bossFillAreaRect.anchorMax = Vector2.one;
                bossFillAreaRect.offsetMin = new Vector2(6f, 5f);
                bossFillAreaRect.offsetMax = new Vector2(-6f, -5f);

                // Delay Mask
                GameObject bossMaskObj = new GameObject("Boss Mask (Damage Delay)");
                bossMaskObj.transform.SetParent(bossFillArea.transform, false);
                RectTransform bossMaskRect = bossMaskObj.AddComponent<RectTransform>();
                bossMaskRect.anchorMin = Vector2.zero;
                bossMaskRect.anchorMax = Vector2.one;
                bossMaskRect.offsetMin = Vector2.zero;
                bossMaskRect.offsetMax = Vector2.zero;
                Image bossMaskImage = bossMaskObj.AddComponent<Image>();
                bossMaskImage.sprite = sliderFill;
                bossMaskImage.type = Image.Type.Filled;
                bossMaskImage.fillMethod = Image.FillMethod.Horizontal;
                bossMaskImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                bossMaskImage.fillAmount = 1f;
                bossMaskImage.color = new Color(1f, 0.72f, 0.18f, 0.95f);
                bossMaskImage.raycastTarget = false;

                // Health Fill
                GameObject bossHealthFillObj = new GameObject("Boss Health Fill");
                bossHealthFillObj.transform.SetParent(bossFillArea.transform, false);
                RectTransform bossHealthFillRect = bossHealthFillObj.AddComponent<RectTransform>();
                bossHealthFillRect.anchorMin = Vector2.zero;
                bossHealthFillRect.anchorMax = Vector2.one;
                bossHealthFillRect.offsetMin = Vector2.zero;
                bossHealthFillRect.offsetMax = Vector2.zero;
                Image bossHealthFillImage = bossHealthFillObj.AddComponent<Image>();
                bossHealthFillImage.sprite = sliderFill;
                bossHealthFillImage.type = Image.Type.Filled;
                bossHealthFillImage.fillMethod = Image.FillMethod.Horizontal;
                bossHealthFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                bossHealthFillImage.fillAmount = 1f;
                bossHealthFillImage.color = new Color(0.9f, 0.14f, 0.16f, 1f);
                bossHealthFillImage.raycastTarget = false;

                // Front Frame
                GameObject bossFrontObj = new GameObject("Front Frame");
                bossFrontObj.transform.SetParent(bossBarContainer.transform, false);
                RectTransform bossFrontRect = bossFrontObj.AddComponent<RectTransform>();
                bossFrontRect.anchorMin = Vector2.zero;
                bossFrontRect.anchorMax = Vector2.one;
                bossFrontRect.offsetMin = Vector2.zero;
                bossFrontRect.offsetMax = Vector2.zero;
                Image bossFrontImage = bossFrontObj.AddComponent<Image>();
                bossFrontImage.sprite = sliderFront;
                bossFrontImage.type = Image.Type.Sliced;
                bossFrontImage.color = new Color(1f, 1f, 1f, 0.45f);
                bossFrontImage.raycastTarget = false;

                // Boss Health Text
                GameObject bossTextObj = new GameObject("Boss Health Text");
                bossTextObj.transform.SetParent(bossBarContainer.transform, false);
                RectTransform bossTextRect = bossTextObj.AddComponent<RectTransform>();
                bossTextRect.anchorMin = Vector2.zero;
                bossTextRect.anchorMax = Vector2.one;
                bossTextRect.offsetMin = new Vector2(25f, 0f);
                bossTextRect.offsetMax = new Vector2(-25f, 0f);
                TextMeshProUGUI bossHpText = bossTextObj.AddComponent<TextMeshProUGUI>();
                bossHpText.font = fallbackFont;
                bossHpText.text = "5000 / 5000";
                bossHpText.fontSize = 16;
                bossHpText.fontStyle = FontStyles.Bold;
                bossHpText.alignment = TextAlignmentOptions.Center;
                bossHpText.color = Color.white;
                bossHpText.raycastTarget = false;

                // Serialize UIBossHealthBar
                SerializedObject bossSO = new SerializedObject(bossHealthBarComp);
                bossSO.FindProperty("bossFillImage").objectReferenceValue = bossHealthFillImage;
                bossSO.FindProperty("bossMaskFillImage").objectReferenceValue = bossMaskImage;
                bossSO.FindProperty("bossNameText").objectReferenceValue = bossNameText;
                bossSO.FindProperty("bossHealthText").objectReferenceValue = bossHpText;
                bossSO.FindProperty("canvasGroup").objectReferenceValue = bossCanvasGroup;
                bossSO.FindProperty("bossIcon").objectReferenceValue = skullImage;
                bossSO.ApplyModifiedPropertiesWithoutUndo();


                // ==========================================
                // 5. LIÊN KẾT VÀO UIGAME
                // ==========================================
                SerializedObject uiGameSO = new SerializedObject(uiGame);
                SerializedProperty playerBarProp = uiGameSO.FindProperty("playerHealthBar");
                if (playerBarProp != null) playerBarProp.objectReferenceValue = playerHealthBarComp;

                SerializedProperty bossBarProp = uiGameSO.FindProperty("bossHealthBar");
                if (bossBarProp != null) bossBarProp.objectReferenceValue = bossHealthBarComp;

                uiGameSO.ApplyModifiedPropertiesWithoutUndo();

                // Bảo vệ vị trí Floating Joystick không bị xê dịch
                Transform joystickTransform = prefabRoot.transform.Find("Floating Joystick");
                if (joystickTransform != null)
                {
                    RectTransform joyRect = joystickTransform.GetComponent<RectTransform>();
                    if (joyRect != null)
                    {
                        joyRect.anchoredPosition = new Vector2(-785.51166f, 0f);
                    }
                }

                // 6. Lưu Prefab
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PREFAB_PATH);
                Debug.Log($"[UIHealthBarsSetup] Đã lưu thành công UI Game Prefab tại: {PREFAB_PATH}");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 7. Đồng bộ Scene Game.unity nếu đang mở
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path == SCENE_PATH)
            {
                var uiGameInScene = Object.FindAnyObjectByType<UIGame>();
                if (uiGameInScene != null)
                {
                    EditorUtility.SetDirty(uiGameInScene);
                    EditorSceneManager.MarkSceneDirty(activeScene);
                    EditorSceneManager.SaveScene(activeScene);
                    Debug.Log("[UIHealthBarsSetup] Đã đồng bộ Scene Game.unity thành công!");
                }
            }

            Debug.Log("[UIHealthBarsSetup] Hoàn tất thiết lập giao diện Thanh máu Player và Boss!");
        }
    }
}
#endif

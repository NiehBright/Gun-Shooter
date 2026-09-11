#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.IO;

namespace Watermelon.SquadShooter
{
    [InitializeOnLoad]
    public class UILoadingScreenSetup
    {
        private const string PREFAB_PATH = "Assets/Project Data/Game/Prefabs/UI/Pages/UI Loading Screen.prefab";
        private const string SCENE_PATH = "Assets/Project Data/Game/Scenes/Game.unity";
        private const string VERSION_KEY = "GunShooter_LoadingScreenPrefab_v3";

        // Asset paths
        private const string BG_SPRITE_PATH = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Demo/Demo_Backgound/Background_00.png";
        private const string GLOW_SPRITE_PATH = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Demo/Demo_Backgound/Background_ScreenGlow.png";
        private const string SLIDER_FRAME_PATH = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Sliders_Custom/Slider07_White1_Frame.png";
        private const string SLIDER_FILL_PATH = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Sliders_Custom/Slider07_White2_Fill.png";
        private const string VIETNAMESE_FONT_PATH = "Assets/Project Data/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset";
        private const string TITLE_FONT_PATH = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Fonts/Oxanium-ExtraBold_Extended ASCII SDF.asset";

        static UILoadingScreenSetup()
        {
            EditorApplication.delayCall += CheckAndBuild;
        }

        private static void CheckAndBuild()
        {
            if (!EditorPrefs.GetBool(VERSION_KEY, false))
            {
                BuildPrefabAndLinkScene(false);
                EditorPrefs.SetBool(VERSION_KEY, true);
            }
        }

        [MenuItem("GunShooter/Rebuild Loading Screen Prefab & Link Scene")]
        public static void ForceBuild()
        {
            BuildPrefabAndLinkScene(true);
        }

        [MenuItem("GunShooter/Select Loading Screen Prefab")]
        public static void SelectPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            if (prefab != null)
            {
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
                Debug.Log($"[LoadingScreen] Prefab tìm thấy tại: {PREFAB_PATH}. Anh có thể double click vào để chỉnh sửa tùy ý trong Prefab Mode!");
            }
            else
            {
                Debug.LogWarning($"[LoadingScreen] Chưa tìm thấy Prefab tại: {PREFAB_PATH}. Đang tiến hành tạo mới...");
                BuildPrefabAndLinkScene(true);
            }
        }

        public static void BuildPrefabAndLinkScene(bool isManual)
        {
            Debug.Log("[LoadingScreen] Bắt đầu tạo UI Loading Screen Prefab chất lượng cao...");

            // 1. Tải resources
            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BG_SPRITE_PATH);
            Sprite glowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GLOW_SPRITE_PATH);
            Sprite sliderFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SLIDER_FRAME_PATH);
            Sprite sliderFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SLIDER_FILL_PATH);
            TMP_FontAsset vietnameseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(VIETNAMESE_FONT_PATH);
            TMP_FontAsset titleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TITLE_FONT_PATH);
            if (titleFont == null) titleFont = vietnameseFont;

            // 2. Tạo Root GameObject
            GameObject rootObj = new GameObject("UI Loading Screen");
            RectTransform rootRect = rootObj.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            rootRect.pivot = new Vector2(0.5f, 0.5f);

            Canvas canvas = rootObj.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1000;

            rootObj.AddComponent<GraphicRaycaster>();

            CanvasGroup canvasGroup = rootObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f; // Hiển thị rõ ràng trong Prefab Mode để user dễ quan sát & chỉnh sửa
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            UILoadingScreen loadingScript = rootObj.AddComponent<UILoadingScreen>();

            // 3. Tạo Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(rootObj.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = bgObj.AddComponent<Image>();
            if (bgSprite != null) bgImage.sprite = bgSprite;
            bgImage.color = new Color(0.08f, 0.09f, 0.12f, 1f); // Dark tactical charcoal background
            bgImage.raycastTarget = true;

            // 4. Tạo Screen Glow / Vignette
            if (glowSprite != null)
            {
                GameObject glowObj = new GameObject("VignetteGlow");
                glowObj.transform.SetParent(rootObj.transform, false);
                RectTransform glowRect = glowObj.AddComponent<RectTransform>();
                glowRect.anchorMin = Vector2.zero;
                glowRect.anchorMax = Vector2.one;
                glowRect.offsetMin = Vector2.zero;
                glowRect.offsetMax = Vector2.zero;
                Image glowImage = glowObj.AddComponent<Image>();
                glowImage.sprite = glowSprite;
                glowImage.color = new Color(0.12f, 0.35f, 0.65f, 0.35f); // Subtle futuristic cyan glow
                glowImage.raycastTarget = false;
            }

            // 5. Tạo Title / Logo ở giữa
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(rootObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = new Vector2(0, 80);
            titleRect.sizeDelta = new Vector2(700, 80);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            if (titleFont != null) titleText.font = titleFont;
            titleText.text = "GUN SHOOTER";
            titleText.fontSize = 58;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;

            GameObject subTitleObj = new GameObject("SubtitleText");
            subTitleObj.transform.SetParent(rootObj.transform, false);
            RectTransform subTitleRect = subTitleObj.AddComponent<RectTransform>();
            subTitleRect.anchorMin = new Vector2(0.5f, 0.5f);
            subTitleRect.anchorMax = new Vector2(0.5f, 0.5f);
            subTitleRect.pivot = new Vector2(0.5f, 0.5f);
            subTitleRect.anchoredPosition = new Vector2(0, 35);
            subTitleRect.sizeDelta = new Vector2(500, 30);

            TextMeshProUGUI subTitleText = subTitleObj.AddComponent<TextMeshProUGUI>();
            if (vietnameseFont != null) subTitleText.font = vietnameseFont;
            subTitleText.text = "TACTICAL SQUAD OPERATIONS";
            subTitleText.fontSize = 15;
            subTitleText.fontStyle = FontStyles.Bold;
            subTitleText.characterSpacing = 8;
            subTitleText.alignment = TextAlignmentOptions.Center;
            subTitleText.color = new Color(0.45f, 0.7f, 0.95f, 0.75f);

            // 6. Tạo Bottom Section (Slider, Percentage, Hint)
            GameObject bottomContainer = new GameObject("BottomSection");
            bottomContainer.transform.SetParent(rootObj.transform, false);
            RectTransform bottomRect = bottomContainer.AddComponent<RectTransform>();
            bottomRect.anchorMin = new Vector2(0.5f, 0f);
            bottomRect.anchorMax = new Vector2(0.5f, 0f);
            bottomRect.pivot = new Vector2(0.5f, 0f);
            bottomRect.anchoredPosition = new Vector2(0, 140);
            bottomRect.sizeDelta = new Vector2(850, 220);

            // Percentage Text (nằm ngay phía trên slider)
            GameObject percentObj = new GameObject("PercentageText");
            percentObj.transform.SetParent(bottomContainer.transform, false);
            RectTransform percentRect = percentObj.AddComponent<RectTransform>();
            percentRect.anchorMin = new Vector2(0.5f, 0.5f);
            percentRect.anchorMax = new Vector2(0.5f, 0.5f);
            percentRect.pivot = new Vector2(0.5f, 0.5f);
            percentRect.anchoredPosition = new Vector2(0, 52);
            percentRect.sizeDelta = new Vector2(250, 40);

            TextMeshProUGUI percentText = percentObj.AddComponent<TextMeshProUGUI>();
            if (titleFont != null) percentText.font = titleFont;
            percentText.text = "0%";
            percentText.fontSize = 26;
            percentText.fontStyle = FontStyles.Bold;
            percentText.alignment = TextAlignmentOptions.Center;
            percentText.color = new Color(0.92f, 0.97f, 1f, 1f);

            // Slider
            GameObject sliderObj = new GameObject("Slider_Loading");
            sliderObj.transform.SetParent(bottomContainer.transform, false);
            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
            sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
            sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.anchoredPosition = new Vector2(0, 15);
            sliderRect.sizeDelta = new Vector2(720, 28);

            Image sliderBg = sliderObj.AddComponent<Image>();
            if (sliderFrameSprite != null)
            {
                sliderBg.sprite = sliderFrameSprite;
                sliderBg.type = Image.Type.Sliced;
            }
            sliderBg.color = new Color(0.12f, 0.14f, 0.18f, 1f);

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.transition = Selectable.Transition.None;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;

            // Fill Area
            GameObject fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(4, 4);
            fillAreaRect.offsetMax = new Vector2(-4, -4);

            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image fillImage = fillObj.AddComponent<Image>();
            if (sliderFillSprite != null)
            {
                fillImage.sprite = sliderFillSprite;
                fillImage.type = Image.Type.Sliced;
            }
            fillImage.color = new Color(0.2f, 0.82f, 0.95f, 1f); // Vibrant cyan/teal energy fill

            slider.fillRect = fillRect;
            slider.targetGraphic = fillImage;

            // Hint Text (nằm phía dưới slider)
            GameObject hintObj = new GameObject("HintText");
            hintObj.transform.SetParent(bottomContainer.transform, false);
            RectTransform hintRect = hintObj.AddComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0.5f, 0.5f);
            hintRect.anchorMax = new Vector2(0.5f, 0.5f);
            hintRect.pivot = new Vector2(0.5f, 0.5f);
            hintRect.anchoredPosition = new Vector2(0, -35);
            hintRect.sizeDelta = new Vector2(820, 50);

            TextMeshProUGUI hintText = hintObj.AddComponent<TextMeshProUGUI>();
            if (vietnameseFont != null) hintText.font = vietnameseFont;
            hintText.text = "Đang tải dữ liệu...";
            hintText.fontSize = 22;
            hintText.alignment = TextAlignmentOptions.Center;
            hintText.color = new Color(0.8f, 0.88f, 0.96f, 0.9f);

            // 7. Gán Serialized Fields vào UILoadingScreen
            SerializedObject so = new SerializedObject(loadingScript);
            so.FindProperty("loadingSlider").objectReferenceValue = slider;
            so.FindProperty("progressFillImage").objectReferenceValue = fillImage;
            so.FindProperty("progressText").objectReferenceValue = percentText;
            so.FindProperty("hintText").objectReferenceValue = hintText;
            so.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
            so.ApplyModifiedProperties();

            // 8. Lưu thành Prefab
            string directory = Path.GetDirectoryName(PREFAB_PATH);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(rootObj, PREFAB_PATH);
            Object.DestroyImmediate(rootObj);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[LoadingScreen] Đã lưu Prefab thành công tại: {PREFAB_PATH}");

            // 9. Cập nhật Scene Game.unity
            UpdateSceneWithPrefab(savedPrefab);

            if (isManual)
            {
                EditorUtility.DisplayDialog("Thành công!", 
                    $"Đã tạo UI Loading Screen Prefab tại:\n{PREFAB_PATH}\n\nvà đã liên kết vào Scene Game.unity!\n\nBây giờ anh có thể mở Prefab này trong thư mục Prefabs để chỉnh sửa giao diện theo ý thích nhé!", "Tuyệt vời!");
            }
        }

        private static void UpdateSceneWithPrefab(GameObject prefabAsset)
        {
            if (prefabAsset == null) return;

            var currentScene = EditorSceneManager.GetActiveScene();
            bool needReopen = currentScene.path != SCENE_PATH;

            if (needReopen)
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    currentScene = EditorSceneManager.OpenScene(SCENE_PATH);
                }
                else
                {
                    Debug.LogWarning("[LoadingScreen] Không thể mở scene Game.unity vì scene hiện tại chưa được lưu.");
                    return;
                }
            }

            // Tìm UIController (Canvas cha)
            UIController uiController = Object.FindAnyObjectByType<UIController>(FindObjectsInactive.Include);
            if (uiController == null)
            {
                Debug.LogError("[LoadingScreen] Không tìm thấy UIController trong scene Game.unity!");
                return;
            }

            // Tìm và xóa các đối tượng loading cũ (cả object rời và instance dummy cũ)
            var allScreens = Object.FindObjectsByType<UILoadingScreen>(FindObjectsInactive.Include);
            foreach (var s in allScreens)
            {
                Undo.DestroyObjectImmediate(s.gameObject);
            }

            for (int i = uiController.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = uiController.transform.GetChild(i);
                if (child.name == "UI Loading Screen" || child.name == "UI Page - Loading Screen")
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                }
            }

            // Instantiate Prefab thật vào làm con của UIController Canvas
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset, uiController.transform);
            instance.name = "UI Loading Screen";

            // Trong Scene thì ẩn Canvas đi để không che mắt người làm việc trong Editor Scene view
            Canvas sceneCanvas = instance.GetComponent<Canvas>();
            if (sceneCanvas != null) sceneCanvas.enabled = false;

            CanvasGroup sceneCg = instance.GetComponent<CanvasGroup>();
            if (sceneCg != null) sceneCg.alpha = 0f;

            EditorSceneManager.MarkSceneDirty(currentScene);
            EditorSceneManager.SaveScene(currentScene);

            Debug.Log("[LoadingScreen] Đã gắn Prefab Instance 'UI Loading Screen' vào Canvas UIController trong scene Game.unity!");
        }
    }
}
#endif

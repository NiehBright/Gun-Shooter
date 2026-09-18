#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Watermelon.SquadShooter
{
    [InitializeOnLoad]
    public static class UIWeaponPageEditorTool
    {
        static UIWeaponPageEditorTool()
        {
            EditorApplication.delayCall += AutoRunOnce;
        }

        private static void AutoRunOnce()
        {
            if (SessionState.GetBool("UIWeaponPanel_Bake_Redesign_Done", false)) return;
            SessionState.SetBool("UIWeaponPanel_Bake_Redesign_Done", true);
            SetupUIWeaponPanel();
        }

        private const string PREFAB_PATH = "Assets/Project Data/Game/Prefabs/UI/Pages/UI Weapon Panel.prefab";
        private const string CARD_PREFAB_PATH = "Assets/Project Data/Game/Prefabs/UI/Weapon Panel UI.prefab";

        private const string SP_CARD_BG = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Frame_Custom/Frame_ListFrame03_White1.png";
        private const string SP_GLOW = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Frame_Custom/Frame_LineFrame03_White4_Glow.png";
        private const string SP_BADGE = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Label/Label_Label01_White1.png";
        private const string SP_SLOT_FRAME = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Frame_Custom/Frame_ItemFrame01_n_White1.png";
        private const string SP_SLOT_BG = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Frame_Custom/Frame_ItemFrame01_00_White.png";
        private const string SP_STRIPE = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Frame_Custom/Frame_LineFrame05_White1.png";
        private const string SP_CHECK = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Icon_PictoIcons(x2)/128/Icon_Check.Png";
        private const string SP_LOCK = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Icon_PictoIcons(x2)/128/Icon_Lock.Png";
        private const string SP_BAR_FRAME = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Sliders_Custom/Slider06_White1_Frame.png";
        private const string SP_BAR_FILL = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Sliders_Custom/Slider06_White3_Fill1.png";
        private const string FONT_OXANIUM = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Fonts/Oxanium-ExtraBold_Extended ASCII SDF.asset";
        private const string FONT_NEXON_B = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Fonts/NEXON Football Gothic B SDF.asset";

        [MenuItem("Tools/GunShooter/Setup UI Weapon Panel", false, 101)]
        public static void SetupUIWeaponPanel()
        {
            // 1. Setup weapon card prefab first
            SetupWeaponCardPrefab();

            // 2. Setup main weapon page prefab
            SetupWeaponPagePrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=green>[UIWeaponPageEditorTool] 🎉 ALL UI Weapon Panel components successfully configured with GUI Pro-SurvivalClean!</color>");
        }

        public static void SetupWeaponCardPrefab()
        {
            GameObject cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CARD_PREFAB_PATH);
            if (cardPrefab == null)
            {
                Debug.LogWarning($"[UIWeaponPageEditorTool] Card prefab not found at {CARD_PREFAB_PATH}");
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(cardPrefab) as GameObject;
            if (instance == null) return;

            Sprite cardBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SP_CARD_BG);
            Sprite glowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SP_GLOW);
            Sprite badgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SP_BADGE);
            Sprite slotFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SP_SLOT_FRAME);
            Sprite slotBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SP_SLOT_BG);
            Sprite stripeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SP_STRIPE);
            Sprite checkSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SP_CHECK);
            Sprite lockSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SP_LOCK);
            Sprite barFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SP_BAR_FRAME);
            Sprite barFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SP_BAR_FILL);
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_OXANIUM);
            if (font == null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_NEXON_B);

            instance.layer = 5;
            instance.transform.localScale = Vector3.one;

            // Card RectTransform — 165px height for x3 font sizes
            RectTransform cardRt = instance.GetComponent<RectTransform>();
            if (cardRt != null)
            {
                cardRt.sizeDelta = new Vector2(cardRt.sizeDelta.x, 165f);
            }

            LayoutElement le = instance.GetComponent<LayoutElement>();
            if (le == null) le = instance.AddComponent<LayoutElement>();
            le.minHeight = 165f;
            le.preferredHeight = 165f;
            le.flexibleWidth = 1f;

            // Background
            Transform bgTr = instance.transform.Find("Background");
            if (bgTr != null)
            {
                Image bgImg = bgTr.GetComponent<Image>();
                if (bgImg != null)
                {
                    if (cardBgSprite != null)
                    {
                        bgImg.sprite = cardBgSprite;
                        bgImg.type = Image.Type.Sliced;
                    }
                    bgImg.color = new Color(0.06f, 0.09f, 0.16f, 0.98f);
                }

                // Left Rarity Stripe
                Transform stripeTr = bgTr.Find("Rarity Stripe");
                GameObject stripeObj = (stripeTr != null) ? stripeTr.gameObject : UIWeaponPageBuilder.CreateUIObject("Rarity Stripe", bgTr, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                RectTransform strRt = stripeObj.GetComponent<RectTransform>();
                strRt.anchorMin = new Vector2(0f, 0.5f);
                strRt.anchorMax = new Vector2(0f, 0.5f);
                strRt.pivot = new Vector2(0f, 0.5f);
                strRt.anchoredPosition = new Vector2(5f, 0f);
                strRt.sizeDelta = new Vector2(5f, 153f);
                Image strImg = stripeObj.GetComponent<Image>();
                if (strImg != null)
                {
                    if (stripeSprite != null) { strImg.sprite = stripeSprite; strImg.type = Image.Type.Sliced; }
                    strImg.color = new Color(1f, 0.75f, 0.15f);
                    strImg.raycastTarget = false;
                }

                // Slot Container (Icon Background)
                Transform iconBgTr = bgTr.Find("Icon Background");
                if (iconBgTr != null)
                {
                    RectTransform slotRt = (RectTransform)iconBgTr;
                    slotRt.anchorMin = new Vector2(0f, 0.5f);
                    slotRt.anchorMax = new Vector2(0f, 0.5f);
                    slotRt.pivot = new Vector2(0f, 0.5f);
                    slotRt.anchoredPosition = new Vector2(20f, 0f);
                    slotRt.sizeDelta = new Vector2(128f, 128f);

                    // Slot Inner BG
                    Transform sBgTr = iconBgTr.Find("Slot BG");
                    GameObject sBgObj = (sBgTr != null) ? sBgTr.gameObject : UIWeaponPageBuilder.CreateUIObject("Slot BG", iconBgTr, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    sBgObj.transform.SetAsFirstSibling();
                    RectTransform sBgRt = sBgObj.GetComponent<RectTransform>();
                    sBgRt.anchorMin = Vector2.zero;
                    sBgRt.anchorMax = Vector2.one;
                    sBgRt.sizeDelta = Vector2.zero;
                    sBgRt.anchoredPosition = Vector2.zero;
                    Image sBgImg = sBgObj.GetComponent<Image>();
                    if (sBgImg != null)
                    {
                        if (slotBgSprite != null) { sBgImg.sprite = slotBgSprite; sBgImg.type = Image.Type.Sliced; }
                        sBgImg.color = new Color(0.04f, 0.06f, 0.11f, 0.95f);
                        sBgImg.raycastTarget = false;
                    }

                    // Weapon Icon
                    Transform iconTr = iconBgTr.Find("Weapon Icon");
                    if (iconTr != null)
                    {
                        RectTransform iconRt = (RectTransform)iconTr;
                        iconRt.anchorMin = new Vector2(0.5f, 0.5f);
                        iconRt.anchorMax = new Vector2(0.5f, 0.5f);
                        iconRt.pivot = new Vector2(0.5f, 0.5f);
                        iconRt.anchoredPosition = Vector2.zero;
                        iconRt.sizeDelta = new Vector2(104f, 104f);
                        Image iconImg = iconTr.GetComponent<Image>();
                        if (iconImg != null) iconImg.preserveAspect = true;
                    }

                    // Slot Outer Frame
                    Transform sFrameTr = iconBgTr.Find("Slot Frame");
                    GameObject sFrameObj = (sFrameTr != null) ? sFrameTr.gameObject : UIWeaponPageBuilder.CreateUIObject("Slot Frame", iconBgTr, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    sFrameObj.transform.SetAsLastSibling();
                    RectTransform sfRt = sFrameObj.GetComponent<RectTransform>();
                    sfRt.anchorMin = Vector2.zero;
                    sfRt.anchorMax = Vector2.one;
                    sfRt.sizeDelta = Vector2.zero;
                    sfRt.anchoredPosition = Vector2.zero;
                    Image sfImg = sFrameObj.GetComponent<Image>();
                    if (sfImg != null)
                    {
                        if (slotFrameSprite != null) { sfImg.sprite = slotFrameSprite; sfImg.type = Image.Type.Sliced; }
                        sfImg.color = new Color(1f, 0.9f, 0.6f);
                        sfImg.raycastTarget = false;
                    }
                }

                // Name
                Transform nameTr = bgTr.Find("Name");
                if (nameTr != null)
                {
                    RectTransform nameRt = (RectTransform)nameTr;
                    nameRt.anchorMin = new Vector2(0f, 1f);
                    nameRt.anchorMax = new Vector2(0f, 1f);
                    nameRt.pivot = new Vector2(0f, 1f);
                    nameRt.anchoredPosition = new Vector2(164f, -14f);
                    nameRt.sizeDelta = new Vector2(290f, 58f);
                    var tmp = nameTr.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        if (font != null) tmp.font = font;
                        tmp.fontSize = 54f;
                        tmp.fontStyle = FontStyles.Bold;
                        tmp.enableAutoSizing = true;
                        tmp.fontSizeMin = 34f;
                        tmp.fontSizeMax = 54f;
                        tmp.alignment = TextAlignmentOptions.Left;
                        tmp.color = Color.white;
                    }
                }

                // Rarity Badge Pill Tag
                Transform badgeTr = bgTr.Find("Rarity Badge");
                GameObject badgeObj = (badgeTr != null) ? badgeTr.gameObject : UIWeaponPageBuilder.CreateUIObject("Rarity Badge", bgTr, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                RectTransform badgeRt = badgeObj.GetComponent<RectTransform>();
                badgeRt.anchorMin = new Vector2(0f, 1f);
                badgeRt.anchorMax = new Vector2(0f, 1f);
                badgeRt.pivot = new Vector2(0f, 1f);
                badgeRt.anchoredPosition = new Vector2(164f, -74f);
                badgeRt.sizeDelta = new Vector2(140f, 34f);
                Image badgeImg = badgeObj.GetComponent<Image>();
                if (badgeImg != null)
                {
                    if (badgeSprite != null) { badgeImg.sprite = badgeSprite; badgeImg.type = Image.Type.Sliced; }
                    badgeImg.color = new Color(0.35f, 0.25f, 0.05f, 0.95f);
                    badgeImg.raycastTarget = false;
                }

                // Rarity Text inside badge
                Transform rarTr = bgTr.Find("Rarity");
                if (rarTr == null) rarTr = badgeObj.transform.Find("Rarity");
                if (rarTr != null)
                {
                    rarTr.SetParent(badgeObj.transform, false);
                    RectTransform rarRt = (RectTransform)rarTr;
                    rarRt.anchorMin = Vector2.zero;
                    rarRt.anchorMax = Vector2.one;
                    rarRt.offsetMin = Vector2.zero;
                    rarRt.offsetMax = Vector2.zero;
                    rarRt.sizeDelta = Vector2.zero;
                    rarRt.anchoredPosition = Vector2.zero;
                    var tmp = rarTr.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        if (font != null) tmp.font = font;
                        tmp.fontSize = 24f;
                        tmp.fontStyle = FontStyles.Bold;
                        tmp.alignment = TextAlignmentOptions.Center;
                        tmp.color = new Color(1f, 0.85f, 0.4f);
                    }
                }

                // Level Text
                Transform lvlTr = bgTr.Find("Level Text");
                if (lvlTr != null)
                {
                    RectTransform lvlRt = (RectTransform)lvlTr;
                    lvlRt.anchorMin = new Vector2(0f, 0f);
                    lvlRt.anchorMax = new Vector2(0f, 0f);
                    lvlRt.pivot = new Vector2(0f, 0f);
                    lvlRt.anchoredPosition = new Vector2(164f, 16f);
                    lvlRt.sizeDelta = new Vector2(200f, 42f);
                    var tmp = lvlTr.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        if (font != null) tmp.font = font;
                        tmp.fontSize = 40f;
                        tmp.fontStyle = FontStyles.Bold;
                        tmp.alignment = TextAlignmentOptions.Left;
                        tmp.color = new Color(0f, 0.85f, 1f);
                    }
                }
            }

            // Selection Back
            Transform selTr = instance.transform.Find("Selection Back");
            if (selTr != null)
            {
                Image selImg = selTr.GetComponent<Image>();
                if (selImg != null)
                {
                    if (glowSprite != null)
                    {
                        selImg.sprite = glowSprite;
                        selImg.type = Image.Type.Sliced;
                    }
                    selImg.color = new Color(0f, 0.85f, 1f, 0.95f);
                }
            }

            // Fonts on all text components
            if (font != null)
            {
                foreach (var tmp in instance.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    tmp.font = font;
                }
            }

            UIWeaponPageBuilder.SetLayerRecursively(instance, 5);

            EditorUtility.SetDirty(instance);
            bool success = false;
            PrefabUtility.SaveAsPrefabAsset(instance, CARD_PREFAB_PATH, out success);
            Object.DestroyImmediate(instance);

            if (success)
                Debug.Log("<color=cyan>[UIWeaponPageEditorTool] ✅ Successfully updated Weapon Panel UI card prefab with SurvivalClean!</color>");
            else
                Debug.LogError("[UIWeaponPageEditorTool] ❌ Failed to save Weapon Panel UI card prefab!");
        }

        public static void SetupWeaponPagePrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            if (prefab == null)
            {
                Debug.LogError($"[UIWeaponPageEditorTool] Cannot find prefab at {PREFAB_PATH}");
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                Debug.LogError("[UIWeaponPageEditorTool] Failed to instantiate prefab.");
                return;
            }

            // CRITICAL: Ensure root scale is (1, 1, 1) and layer is 5 (UI) so it's fully visible in Scene View and Prefab Mode!
            instance.transform.localScale = Vector3.one;
            instance.layer = 5;

            UIWeaponPage page = instance.GetComponent<UIWeaponPage>();
            if (page != null)
            {
                UIWeaponPageBuilder.BuildLayout(page);

                // Populate preview weapon cards in the right list for Editor Prefab viewing
                CreatePreviewCards(page);

                // Ensure all descendants are on Layer 5 (UI) and scale is Vector3.one
                UIWeaponPageBuilder.SetLayerRecursively(instance, 5);
                instance.transform.localScale = Vector3.one;

                EditorUtility.SetDirty(instance);
                bool success = false;
                PrefabUtility.SaveAsPrefabAsset(instance, PREFAB_PATH, out success);

                if (success)
                {
                    Debug.Log("<color=green>[UIWeaponPageEditorTool] ✅ Successfully baked UI Weapon Panel page prefab with full SurvivalClean layout!</color>");
                    var detailsPanel = instance.GetComponentInChildren<UIWeaponDetailsPanel>(true);
                    if (detailsPanel != null)
                        Debug.Log("<color=cyan>[UIWeaponPageEditorTool] ✅ UIWeaponDetailsPanel found and serialized.</color>");
                }
                else
                {
                    Debug.LogError("[UIWeaponPageEditorTool] ❌ Failed to save page prefab!");
                }
            }
            else
            {
                Debug.LogError("[UIWeaponPageEditorTool] UIWeaponPage component not found on prefab.");
            }

            Object.DestroyImmediate(instance);
        }

        private static void CreatePreviewCards(UIWeaponPage page)
        {
            GameObject cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CARD_PREFAB_PATH);
            if (cardPrefab == null || page == null) return;

            Transform containerTr = page.PanelsContainer;
            if (containerTr == null && page.ScrollView != null)
                containerTr = page.ScrollView.content;
            if (containerTr == null) return;

            // Clear old children first
            for (int i = containerTr.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(containerTr.GetChild(i).gameObject);
            }

            Sprite cardBg = AssetDatabase.LoadAssetAtPath<Sprite>(SP_CARD_BG);
            Sprite glowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SP_GLOW);
            Sprite badgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SP_BADGE);
            Sprite slotFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SP_SLOT_FRAME);
            Sprite slotBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SP_SLOT_BG);
            Sprite stripeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SP_STRIPE);
            Sprite checkSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SP_CHECK);
            Sprite lockSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SP_LOCK);
            Sprite barFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SP_BAR_FRAME);
            Sprite barFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SP_BAR_FILL);
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_OXANIUM);
            if (font == null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_NEXON_B);

            Sprite iconMinigun = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Project Data/Game/Images/Weapons/weapon_Minigun_Fire.png");
            Sprite iconShotgun = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Project Data/Game/Images/Weapons/weapon_Shotgun.png");
            Sprite iconTesla = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Project Data/Game/Images/Weapons/weapon_Teslagun_Lightning.png");
            Sprite iconLava = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Project Data/Game/Images/Weapons/weapon_Lava_gun.png");

            var previewData = new (string name, string rarity, Color rarityColor, string level, Sprite icon, bool isEquipped, bool isSelected, bool isLocked, int cards, int targetCards)[]
            {
                ("MINIGUN", "LEGENDARY", new Color(1f, 0.75f, 0.15f), "CẤP 1", iconMinigun, true, true, false, 0, 0),
                ("SHOTGUN", "EPIC", new Color(0.75f, 0.35f, 1f), "CẤP 3", iconShotgun, false, false, false, 0, 0),
                ("TESLA GUN", "MYTHIC", new Color(0.95f, 0.25f, 0.35f), "CẤP 2", iconTesla, false, false, true, 15, 50),
                ("LAVA GUN", "RARE", new Color(0.2f, 0.65f, 1f), "CẤP 1", iconLava, false, false, false, 0, 0),
            };

            for (int i = 0; i < previewData.Length; i++)
            {
                var p = previewData[i];
                GameObject cardInstance = PrefabUtility.InstantiatePrefab(cardPrefab, containerTr) as GameObject;
                if (cardInstance == null) continue;

                cardInstance.name = $"WeaponCard_{p.name}";
                cardInstance.transform.localScale = Vector3.one;
                cardInstance.layer = 5;

                RectTransform cardRt = cardInstance.GetComponent<RectTransform>();
                if (cardRt != null)
                {
                    cardRt.sizeDelta = new Vector2(cardRt.sizeDelta.x, 165f);
                }

                LayoutElement le = cardInstance.GetComponent<LayoutElement>();
                if (le == null) le = cardInstance.AddComponent<LayoutElement>();
                le.minHeight = 165f;
                le.preferredHeight = 165f;
                le.flexibleWidth = 1f;

                Transform bgTr = cardInstance.transform.Find("Background");
                if (bgTr != null)
                {
                    // Background
                    Image bgImg = bgTr.GetComponent<Image>();
                    if (bgImg != null)
                    {
                        if (cardBg != null) { bgImg.sprite = cardBg; bgImg.type = Image.Type.Sliced; }
                        bgImg.color = new Color(0.06f, 0.09f, 0.16f, 0.98f);
                    }

                    // Left Rarity Accent Stripe
                    Transform stripeTr = bgTr.Find("Rarity Stripe");
                    GameObject stripeObj = (stripeTr != null) ? stripeTr.gameObject : UIWeaponPageBuilder.CreateUIObject("Rarity Stripe", bgTr, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    RectTransform strRt = stripeObj.GetComponent<RectTransform>();
                    strRt.anchorMin = new Vector2(0f, 0.5f);
                    strRt.anchorMax = new Vector2(0f, 0.5f);
                    strRt.pivot = new Vector2(0f, 0.5f);
                    strRt.anchoredPosition = new Vector2(5f, 0f);
                    strRt.sizeDelta = new Vector2(5f, 153f);
                    Image strImg = stripeObj.GetComponent<Image>();
                    if (strImg != null)
                    {
                        if (stripeSprite != null) { strImg.sprite = stripeSprite; strImg.type = Image.Type.Sliced; }
                        strImg.color = p.rarityColor;
                        strImg.raycastTarget = false;
                    }

                    // Slot Container
                    Transform iconBgTr = bgTr.Find("Icon Background");
                    if (iconBgTr != null)
                    {
                        RectTransform slotRt = (RectTransform)iconBgTr;
                        slotRt.anchorMin = new Vector2(0f, 0.5f);
                        slotRt.anchorMax = new Vector2(0f, 0.5f);
                        slotRt.pivot = new Vector2(0f, 0.5f);
                        slotRt.anchoredPosition = new Vector2(20f, 0f);
                        slotRt.sizeDelta = new Vector2(128f, 128f);

                        // Slot Inner BG
                        Transform sBgTr = iconBgTr.Find("Slot BG");
                        GameObject sBgObj = (sBgTr != null) ? sBgTr.gameObject : UIWeaponPageBuilder.CreateUIObject("Slot BG", iconBgTr, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                        sBgObj.transform.SetAsFirstSibling();
                        RectTransform sBgRt = sBgObj.GetComponent<RectTransform>();
                        sBgRt.anchorMin = Vector2.zero;
                        sBgRt.anchorMax = Vector2.one;
                        sBgRt.sizeDelta = Vector2.zero;
                        sBgRt.anchoredPosition = Vector2.zero;
                        Image sBgImg = sBgObj.GetComponent<Image>();
                        if (sBgImg != null)
                        {
                            if (slotBgSprite != null) { sBgImg.sprite = slotBgSprite; sBgImg.type = Image.Type.Sliced; }
                            sBgImg.color = new Color(0.04f, 0.06f, 0.11f, 0.95f);
                            sBgImg.raycastTarget = false;
                        }

                        // Weapon Icon Glow
                        Image glowImg = iconBgTr.GetComponent<Image>();
                        if (glowImg != null)
                        {
                            glowImg.color = new Color(p.rarityColor.r, p.rarityColor.g, p.rarityColor.b, 0.35f);
                        }

                        // Weapon Icon
                        Transform iconTr = iconBgTr.Find("Weapon Icon");
                        if (iconTr != null)
                        {
                            RectTransform iconRt = (RectTransform)iconTr;
                            iconRt.anchorMin = new Vector2(0.5f, 0.5f);
                            iconRt.anchorMax = new Vector2(0.5f, 0.5f);
                            iconRt.pivot = new Vector2(0.5f, 0.5f);
                            iconRt.anchoredPosition = Vector2.zero;
                            iconRt.sizeDelta = new Vector2(104f, 104f);
                            Image iconImg = iconTr.GetComponent<Image>();
                            if (iconImg != null)
                            {
                                if (p.icon != null) iconImg.sprite = p.icon;
                                iconImg.preserveAspect = true;
                                iconImg.color = Color.white;
                            }
                        }

                        // Slot Outer Frame
                        Transform sFrameTr = iconBgTr.Find("Slot Frame");
                        GameObject sFrameObj = (sFrameTr != null) ? sFrameTr.gameObject : UIWeaponPageBuilder.CreateUIObject("Slot Frame", iconBgTr, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                        sFrameObj.transform.SetAsLastSibling();
                        RectTransform sfRt = sFrameObj.GetComponent<RectTransform>();
                        sfRt.anchorMin = Vector2.zero;
                        sfRt.anchorMax = Vector2.one;
                        sfRt.sizeDelta = Vector2.zero;
                        sfRt.anchoredPosition = Vector2.zero;
                        Image sfImg = sFrameObj.GetComponent<Image>();
                        if (sfImg != null)
                        {
                            if (slotFrameSprite != null) { sfImg.sprite = slotFrameSprite; sfImg.type = Image.Type.Sliced; }
                            sfImg.color = Color.Lerp(p.rarityColor, Color.white, 0.35f);
                            sfImg.raycastTarget = false;
                        }
                    }

                    // Name
                    Transform nameTr = bgTr.Find("Name");
                    if (nameTr != null)
                    {
                        RectTransform nameRt = (RectTransform)nameTr;
                        nameRt.anchorMin = new Vector2(0f, 1f);
                        nameRt.anchorMax = new Vector2(0f, 1f);
                        nameRt.pivot = new Vector2(0f, 1f);
                        nameRt.anchoredPosition = new Vector2(164f, -14f);
                        nameRt.sizeDelta = new Vector2(290f, 58f);
                        var tmp = nameTr.GetComponent<TextMeshProUGUI>();
                        if (tmp != null)
                        {
                            if (font != null) tmp.font = font;
                            tmp.text = p.name;
                            tmp.fontSize = 54f;
                            tmp.fontStyle = FontStyles.Bold;
                            tmp.enableAutoSizing = true;
                            tmp.fontSizeMin = 34f;
                            tmp.fontSizeMax = 54f;
                            tmp.alignment = TextAlignmentOptions.Left;
                            tmp.color = Color.white;
                        }
                    }

                    // Rarity Badge Pill Tag
                    Transform badgeTr = bgTr.Find("Rarity Badge");
                    GameObject badgeObj = (badgeTr != null) ? badgeTr.gameObject : UIWeaponPageBuilder.CreateUIObject("Rarity Badge", bgTr, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    RectTransform badgeRt = badgeObj.GetComponent<RectTransform>();
                    badgeRt.anchorMin = new Vector2(0f, 1f);
                    badgeRt.anchorMax = new Vector2(0f, 1f);
                    badgeRt.pivot = new Vector2(0f, 1f);
                    badgeRt.anchoredPosition = new Vector2(164f, -74f);
                    badgeRt.sizeDelta = new Vector2(140f, 34f);
                    Image badgeImg = badgeObj.GetComponent<Image>();
                    if (badgeImg != null)
                    {
                        if (badgeSprite != null) { badgeImg.sprite = badgeSprite; badgeImg.type = Image.Type.Sliced; }
                        badgeImg.color = new Color(p.rarityColor.r * 0.35f, p.rarityColor.g * 0.35f, p.rarityColor.b * 0.35f, 0.95f);
                        badgeImg.raycastTarget = false;
                    }

                    // Rarity Text inside badge
                    Transform rarTr = bgTr.Find("Rarity");
                    if (rarTr == null) rarTr = badgeObj.transform.Find("Rarity");
                    if (rarTr != null)
                    {
                        rarTr.SetParent(badgeObj.transform, false);
                        RectTransform rarRt = (RectTransform)rarTr;
                        rarRt.anchorMin = Vector2.zero;
                        rarRt.anchorMax = Vector2.one;
                        rarRt.offsetMin = Vector2.zero;
                        rarRt.offsetMax = Vector2.zero;
                        rarRt.sizeDelta = Vector2.zero;
                        rarRt.anchoredPosition = Vector2.zero;
                        var tmp = rarTr.GetComponent<TextMeshProUGUI>();
                        if (tmp != null)
                        {
                            if (font != null) tmp.font = font;
                            tmp.text = p.rarity;
                            tmp.fontSize = 24f;
                            tmp.fontStyle = FontStyles.Bold;
                            tmp.alignment = TextAlignmentOptions.Center;
                            tmp.color = Color.Lerp(p.rarityColor, Color.white, 0.6f);
                        }
                    }

                    // Level Text
                    Transform lvlTr = bgTr.Find("Level Text");
                    if (lvlTr != null)
                    {
                        lvlTr.gameObject.SetActive(!p.isLocked);
                        RectTransform lvlRt = (RectTransform)lvlTr;
                        lvlRt.anchorMin = new Vector2(0f, 0f);
                        lvlRt.anchorMax = new Vector2(0f, 0f);
                        lvlRt.pivot = new Vector2(0f, 0f);
                        lvlRt.anchoredPosition = new Vector2(164f, 16f);
                        lvlRt.sizeDelta = new Vector2(200f, 42f);
                        var tmp = lvlTr.GetComponent<TextMeshProUGUI>();
                        if (tmp != null)
                        {
                            if (font != null) tmp.font = font;
                            tmp.text = p.level;
                            tmp.fontSize = 40f;
                            tmp.fontStyle = FontStyles.Bold;
                            tmp.alignment = TextAlignmentOptions.Left;
                            tmp.color = new Color(0f, 0.85f, 1f);
                        }
                    }
                }

                // Selection Back (glow)
                Transform selTr = cardInstance.transform.Find("Selection Back");
                if (selTr != null)
                {
                    selTr.gameObject.SetActive(p.isSelected);
                    Image selImg = selTr.GetComponent<Image>();
                    if (selImg != null)
                    {
                        if (glowSprite != null) { selImg.sprite = glowSprite; selImg.type = Image.Type.Sliced; }
                        selImg.color = new Color(0f, 0.85f, 1f, 0.95f);
                    }
                }

                // Equipped Badge
                Transform eqBadgeTr = cardInstance.transform.Find("Equipped Badge");
                if (p.isEquipped)
                {
                    GameObject eqBadgeObj = (eqBadgeTr != null) ? eqBadgeTr.gameObject : UIWeaponPageBuilder.CreateUIObject("Equipped Badge", cardInstance.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    eqBadgeObj.SetActive(true);
                    RectTransform badgeRt = eqBadgeObj.GetComponent<RectTransform>();
                    badgeRt.anchorMin = new Vector2(1f, 0.5f);
                    badgeRt.anchorMax = new Vector2(1f, 0.5f);
                    badgeRt.pivot = new Vector2(1f, 0.5f);
                    badgeRt.anchoredPosition = new Vector2(-18f, 0f);
                    badgeRt.sizeDelta = new Vector2(195f, 54f);

                    Image badgeImg = eqBadgeObj.GetComponent<Image>();
                    if (badgeImg != null)
                    {
                        if (badgeSprite != null) { badgeImg.sprite = badgeSprite; badgeImg.type = Image.Type.Sliced; }
                        badgeImg.color = new Color(0.12f, 0.65f, 0.33f, 0.95f);
                    }

                    // Check Icon
                    Transform checkTr = eqBadgeObj.transform.Find("Check Icon");
                    GameObject checkObj = (checkTr != null) ? checkTr.gameObject : UIWeaponPageBuilder.CreateUIObject("Check Icon", eqBadgeObj.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    RectTransform checkRt = checkObj.GetComponent<RectTransform>();
                    checkRt.anchorMin = new Vector2(0f, 0.5f);
                    checkRt.anchorMax = new Vector2(0f, 0.5f);
                    checkRt.pivot = new Vector2(0.5f, 0.5f);
                    checkRt.anchoredPosition = new Vector2(26f, 0f);
                    checkRt.sizeDelta = new Vector2(26f, 26f);
                    Image checkImg = checkObj.GetComponent<Image>();
                    if (checkImg != null)
                    {
                        if (checkSprite != null) checkImg.sprite = checkSprite;
                        checkImg.color = Color.white;
                        checkImg.raycastTarget = false;
                    }

                    // Text
                    Transform textTr = eqBadgeObj.transform.Find("Text");
                    GameObject textObj = (textTr != null) ? textTr.gameObject : UIWeaponPageBuilder.CreateUIObject("Text", eqBadgeObj.transform, typeof(RectTransform), typeof(TextMeshProUGUI));
                    RectTransform textRt = textObj.GetComponent<RectTransform>();
                    textRt.anchorMin = Vector2.zero;
                    textRt.anchorMax = Vector2.one;
                    textRt.offsetMin = new Vector2(44f, 0f);
                    textRt.offsetMax = new Vector2(-10f, 0f);
                    var tmp = textObj.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        if (font != null) tmp.font = font;
                        tmp.text = "ĐANG DÙNG";
                        tmp.fontSize = 28f;
                        tmp.fontStyle = FontStyles.Bold;
                        tmp.color = Color.white;
                        tmp.alignment = TextAlignmentOptions.Center;
                    }
                }
                else if (eqBadgeTr != null)
                {
                    eqBadgeTr.gameObject.SetActive(false);
                }

                // Locked State
                Transform lockTr = (bgTr != null) ? bgTr.Find("Lock State") : cardInstance.transform.Find("Lock State");
                if (lockTr != null)
                {
                    lockTr.gameObject.SetActive(p.isLocked);
                    if (p.isLocked)
                    {
                        RectTransform lockRt = (RectTransform)lockTr;
                        lockRt.anchorMin = new Vector2(1f, 0.5f);
                        lockRt.anchorMax = new Vector2(1f, 0.5f);
                        lockRt.pivot = new Vector2(1f, 0.5f);
                        lockRt.anchoredPosition = new Vector2(-18f, 0f);
                        lockRt.sizeDelta = new Vector2(195f, 65f);
                        lockRt.localScale = Vector3.one;

                        // Lock Icon
                        Transform lockIconTr = lockTr.Find("Lock Icon");
                        GameObject lockIconObj = (lockIconTr != null) ? lockIconTr.gameObject : UIWeaponPageBuilder.CreateUIObject("Lock Icon", lockTr, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                        RectTransform liRt = lockIconObj.GetComponent<RectTransform>();
                        liRt.anchorMin = new Vector2(0f, 1f);
                        liRt.anchorMax = new Vector2(0f, 1f);
                        liRt.pivot = new Vector2(0f, 1f);
                        liRt.anchoredPosition = new Vector2(6f, -4f);
                        liRt.sizeDelta = new Vector2(26f, 26f);
                        Image liImg = lockIconObj.GetComponent<Image>();
                        if (liImg != null)
                        {
                            if (lockSprite != null) liImg.sprite = lockSprite;
                            liImg.color = new Color(1f, 0.78f, 0.25f, 1f);
                            liImg.raycastTarget = false;
                        }

                        // Cards Amount Text
                        Transform cardAmtTr = lockTr.Find("Cards Amount Text");
                        if (cardAmtTr != null)
                        {
                            RectTransform cardAmtRt = (RectTransform)cardAmtTr;
                            cardAmtRt.anchorMin = new Vector2(0f, 1f);
                            cardAmtRt.anchorMax = new Vector2(1f, 1f);
                            cardAmtRt.pivot = new Vector2(1f, 1f);
                            cardAmtRt.anchoredPosition = new Vector2(0f, 0f);
                            cardAmtRt.sizeDelta = new Vector2(0f, 32f);
                            var tmp = cardAmtTr.GetComponent<TextMeshProUGUI>();
                            if (tmp != null)
                            {
                                if (font != null) tmp.font = font;
                                tmp.text = $"{p.cards}/{p.targetCards}";
                                tmp.fontSize = 32f;
                                tmp.fontStyle = FontStyles.Bold;
                                tmp.alignment = TextAlignmentOptions.Right;
                                tmp.color = new Color(1f, 0.78f, 0.25f, 1f);
                            }
                        }

                        // Progress Bar Frame + Fill
                        Transform barFrameTr = lockTr.Find("Progress Bar");
                        GameObject barFrameObj = (barFrameTr != null) ? barFrameTr.gameObject : UIWeaponPageBuilder.CreateUIObject("Progress Bar", lockTr, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                        RectTransform bfRt = barFrameObj.GetComponent<RectTransform>();
                        bfRt.anchorMin = new Vector2(0f, 0f);
                        bfRt.anchorMax = new Vector2(1f, 0f);
                        bfRt.pivot = new Vector2(0.5f, 0f);
                        bfRt.anchoredPosition = new Vector2(0f, 6f);
                        bfRt.sizeDelta = new Vector2(0f, 14f);
                        Image bfImg = barFrameObj.GetComponent<Image>();
                        if (bfImg != null)
                        {
                            if (barFrameSprite != null) { bfImg.sprite = barFrameSprite; bfImg.type = Image.Type.Sliced; }
                            bfImg.color = new Color(0.20f, 0.25f, 0.35f, 0.95f);
                            bfImg.raycastTarget = false;
                        }

                        Transform barFillTr = barFrameObj.transform.Find("Fill");
                        GameObject barFillObj = (barFillTr != null) ? barFillTr.gameObject : UIWeaponPageBuilder.CreateUIObject("Fill", barFrameObj.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                        RectTransform fillRt = barFillObj.GetComponent<RectTransform>();
                        fillRt.anchorMin = Vector2.zero;
                        fillRt.anchorMax = Vector2.one;
                        fillRt.offsetMin = new Vector2(2f, 2f);
                        fillRt.offsetMax = new Vector2(-2f, -2f);
                        Image fillImg = barFillObj.GetComponent<Image>();
                        if (fillImg != null)
                        {
                            if (barFillSprite != null)
                            {
                                fillImg.sprite = barFillSprite;
                                fillImg.type = Image.Type.Filled;
                                fillImg.fillMethod = Image.FillMethod.Horizontal;
                                fillImg.fillOrigin = 0;
                            }
                            fillImg.color = new Color(0f, 0.85f, 1f, 1f);
                            fillImg.fillAmount = p.targetCards > 0 ? Mathf.Clamp01((float)p.cards / p.targetCards) : 0f;
                            fillImg.raycastTarget = false;
                        }
                    }
                }

                // Hide old elements
                Transform pwrTr = (bgTr != null) ? bgTr.Find("Power Panel") : cardInstance.transform.Find("Power Panel");
                if (pwrTr != null) pwrTr.gameObject.SetActive(false);
                Transform upgTr = (bgTr != null) ? bgTr.Find("Upgrade Button") : cardInstance.transform.Find("Upgrade Button");
                if (upgTr != null) upgTr.gameObject.SetActive(false);
                Transform maxTr = (bgTr != null) ? bgTr.Find("Max Panel") : cardInstance.transform.Find("Max Panel");
                if (maxTr != null) maxTr.gameObject.SetActive(false);

                UIWeaponPageBuilder.SetLayerRecursively(cardInstance, 5);
            }
        }
    }
}
#endif

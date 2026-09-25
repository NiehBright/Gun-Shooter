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
            if (SessionState.GetBool("UIWeaponPanel_Bake_CleanEquip_Done", false)) return;
            SessionState.SetBool("UIWeaponPanel_Bake_CleanEquip_Done", true);
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
        private const string FONT_BEVIETNAM = "Assets/Project Data/Game/Fonts/BeVietnamPro-Bold SDF.asset";
        private const string FONT_OXANIUM = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Fonts/Oxanium-ExtraBold_Extended ASCII SDF.asset";
        private const string FONT_NEXON_B = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Fonts/NEXON Football Gothic B SDF.asset";

        [MenuItem("Tools/GunShooter/Setup UI Weapon Panel", false, 101)]
        public static void SetupUIWeaponPanel()
        {
            // 1. Setup weapon card prefab first
            SetupWeaponCardPrefab();

            // 2. Setup main weapon page prefab (clean container with 0 dummy cards)
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
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_BEVIETNAM);
            if (font == null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_OXANIUM);
            if (font == null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_NEXON_B);

            instance.layer = 5;
            instance.transform.localScale = Vector3.one;

            // Card RectTransform — 155px height
            RectTransform cardRt = instance.GetComponent<RectTransform>();
            if (cardRt != null)
            {
                cardRt.sizeDelta = new Vector2(cardRt.sizeDelta.x, 155f);
            }

            LayoutElement le = instance.GetComponent<LayoutElement>();
            if (le == null) le = instance.AddComponent<LayoutElement>();
            le.minHeight = 155f;
            le.preferredHeight = 155f;
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
                strRt.sizeDelta = new Vector2(5f, 145f);
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
                    slotRt.sizeDelta = new Vector2(120f, 120f);

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
                        iconRt.sizeDelta = new Vector2(96f, 96f);
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
                    nameRt.anchorMin = new Vector2(0f, 0.5f);
                    nameRt.anchorMax = new Vector2(0f, 0.5f);
                    nameRt.pivot = new Vector2(0f, 0.5f);
                    nameRt.anchoredPosition = new Vector2(155f, 18f);
                    nameRt.sizeDelta = new Vector2(280f, 48f);
                    var tmp = nameTr.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        if (font != null) tmp.font = font;
                        tmp.fontSize = 46f;
                        tmp.fontStyle = FontStyles.Bold;
                        tmp.enableAutoSizing = true;
                        tmp.fontSizeMin = 30f;
                        tmp.fontSizeMax = 46f;
                        tmp.alignment = TextAlignmentOptions.Left;
                        tmp.color = Color.white;
                    }
                }

                // Rarity Badge Pill Tag
                Transform badgeTr = bgTr.Find("Rarity Badge");
                GameObject badgeObj = (badgeTr != null) ? badgeTr.gameObject : UIWeaponPageBuilder.CreateUIObject("Rarity Badge", bgTr, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                RectTransform badgeRt = badgeObj.GetComponent<RectTransform>();
                badgeRt.anchorMin = new Vector2(0f, 0.5f);
                badgeRt.anchorMax = new Vector2(0f, 0.5f);
                badgeRt.pivot = new Vector2(0f, 0.5f);
                badgeRt.anchoredPosition = new Vector2(155f, -22f);
                badgeRt.sizeDelta = new Vector2(130f, 28f);
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
                        tmp.fontSize = 20f;
                        tmp.fontStyle = FontStyles.Bold;
                        tmp.alignment = TextAlignmentOptions.Center;
                        tmp.color = new Color(1f, 0.85f, 0.4f);
                    }
                }

                // Tắt hoàn toàn level text trên thẻ
                Transform lvlTr = bgTr.Find("Level Text");
                if (lvlTr != null) lvlTr.gameObject.SetActive(false);
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

            // Equip Action Container trên thẻ
            Transform eqTr = instance.transform.Find("Equip Action");
            if (eqTr == null) eqTr = instance.transform.Find("Equipped Badge");
            GameObject eqObj = (eqTr != null) ? eqTr.gameObject : UIWeaponPageBuilder.CreateUIObject("Equip Action", instance.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            eqObj.name = "Equip Action";
            eqObj.SetActive(true);

            RectTransform eqRt = eqObj.GetComponent<RectTransform>();
            eqRt.anchorMin = new Vector2(1f, 0.5f);
            eqRt.anchorMax = new Vector2(1f, 0.5f);
            eqRt.pivot = new Vector2(1f, 0.5f);
            eqRt.anchoredPosition = new Vector2(-18f, 0f);
            eqRt.sizeDelta = new Vector2(175f, 52f);

            Image eqImg = eqObj.GetComponent<Image>();
            if (eqImg != null)
            {
                if (badgeSprite != null) { eqImg.sprite = badgeSprite; eqImg.type = Image.Type.Sliced; }
                eqImg.color = new Color(0.12f, 0.65f, 0.33f, 0.95f);
            }

            // Check Icon
            Transform checkTr = eqObj.transform.Find("Action Icon");
            if (checkTr == null) checkTr = eqObj.transform.Find("Check Icon");
            GameObject checkObj = (checkTr != null) ? checkTr.gameObject : UIWeaponPageBuilder.CreateUIObject("Action Icon", eqObj.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            checkObj.name = "Action Icon";
            checkObj.SetActive(true);
            RectTransform checkRt = checkObj.GetComponent<RectTransform>();
            checkRt.anchorMin = new Vector2(0f, 0.5f);
            checkRt.anchorMax = new Vector2(0f, 0.5f);
            checkRt.pivot = new Vector2(0.5f, 0.5f);
            checkRt.anchoredPosition = new Vector2(24f, 0f);
            checkRt.sizeDelta = new Vector2(24f, 24f);
            Image checkImg = checkObj.GetComponent<Image>();
            if (checkImg != null)
            {
                if (checkSprite != null) checkImg.sprite = checkSprite;
                checkImg.color = Color.white;
                checkImg.raycastTarget = false;
            }

            // Text
            Transform eqTextTr = eqObj.transform.Find("Text");
            GameObject eqTextObj = (eqTextTr != null) ? eqTextTr.gameObject : UIWeaponPageBuilder.CreateUIObject("Text", eqObj.transform, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform eqTextRt = eqTextObj.GetComponent<RectTransform>();
            eqTextRt.anchorMin = Vector2.zero;
            eqTextRt.anchorMax = Vector2.one;
            eqTextRt.offsetMin = new Vector2(36f, 0f);
            eqTextRt.offsetMax = new Vector2(-8f, 0f);
            var eqTmp = eqTextObj.GetComponent<TextMeshProUGUI>();
            if (eqTmp != null)
            {
                if (font != null) eqTmp.font = font;
                eqTmp.text = "ĐANG DÙNG";
                eqTmp.fontSize = 24f;
                eqTmp.fontStyle = FontStyles.Bold;
                eqTmp.color = Color.white;
                eqTmp.alignment = TextAlignmentOptions.Center;
            }

            // TẮT HOÀN TOÀN TẤT CẢ CÁC THÀNH PHẦN UPGRADE / POWER CŨ
            string[] hideList = { "Upgrade State", "Power Panel", "Max Panel", "Lock State", "Level Text" };
            foreach (var h in hideList)
            {
                Transform t = instance.transform.Find(h);
                if (t != null) t.gameObject.SetActive(false);
                if (bgTr != null)
                {
                    Transform tBg = bgTr.Find(h);
                    if (tBg != null) tBg.gameObject.SetActive(false);
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

            // Root scale (1, 1, 1) and layer 5
            instance.transform.localScale = Vector3.one;
            instance.layer = 5;

            UIWeaponPage page = instance.GetComponent<UIWeaponPage>();
            if (page != null)
            {
                UIWeaponPageBuilder.BuildLayout(page);

                // XÓA SẠCH MỌI THẺ DUMMY TRONG PANELS CONTAINER!
                // Container phải để trống hoàn toàn để khi Runtime chạy, Initialise() sẽ spawn đúng các vũ khí thực tế.
                Transform containerTr = page.PanelsContainer;
                if (containerTr == null && page.ScrollView != null)
                    containerTr = page.ScrollView.content;
                if (containerTr != null)
                {
                    for (int i = containerTr.childCount - 1; i >= 0; i--)
                    {
                        Object.DestroyImmediate(containerTr.GetChild(i).gameObject);
                    }
                }

                // Ensure all descendants are on Layer 5 (UI) and scale is Vector3.one
                UIWeaponPageBuilder.SetLayerRecursively(instance, 5);
                instance.transform.localScale = Vector3.one;

                EditorUtility.SetDirty(instance);
                bool success = false;
                PrefabUtility.SaveAsPrefabAsset(instance, PREFAB_PATH, out success);

                if (success)
                {
                    Debug.Log("<color=green>[UIWeaponPageEditorTool] ✅ Successfully baked UI Weapon Panel page prefab (Clean Container, No Dummy Cards)!</color>");
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
    }
}
#endif

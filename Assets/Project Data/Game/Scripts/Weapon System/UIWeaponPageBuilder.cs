using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public static class UIWeaponPageBuilder
    {
        // ───────────────────────────────────────────────
        // ASSET PATHS — GUI Pro-SurvivalClean
        // ───────────────────────────────────────────────
        private const string SC_BASE = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/";
        private const string SC_FONT = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Fonts/";

        // Frame sprites (9-sliced panels)
        private const string SP_PANEL_BG        = SC_BASE + "Frame_Custom/Frame_Frame01_White1.png";
        private const string SP_INNER_PANEL      = SC_BASE + "Frame_Custom/Frame_Frame02_White1.png";
        private const string SP_STAT_ROW_BG      = SC_BASE + "Frame_Custom/Frame_ListFrame01_White1.png";
        private const string SP_WEAPON_CARD_BG   = SC_BASE + "Frame_Custom/Frame_ListFrame03_White1.png";
        private const string SP_CARD_FRAME_BG    = SC_BASE + "Frame_Custom/Frame_CardFrame02_White1.png";

        // Slider / Progress bar
        private const string SP_BAR_FRAME        = SC_BASE + "Sliders_Custom/Slider06_White1_Frame.png";
        private const string SP_BAR_FILL_AREA    = SC_BASE + "Sliders_Custom/Slider06_White2_FillArea.png";
        private const string SP_BAR_FILL         = SC_BASE + "Sliders_Custom/Slider06_White3_Fill1.png";

        // Labels / Badges
        private const string SP_RARITY_BADGE     = SC_BASE + "Label/Label_Label01_White1.png";
        private const string SP_MAX_LEVEL        = SC_BASE + "Label/Label_Label04_White1.png";

        // Buttons
        private const string SP_BTN_PRIMARY      = SC_BASE + "Button_Custom/Btn_TextButton_Square01_White1.png";
        private const string SP_BTN_SECONDARY    = SC_BASE + "Button_Custom/Btn_TextButton_Square01_White2.png";
        private const string SP_BTN_TERTIARY     = SC_BASE + "Button_Custom/Btn_TextButton_Square01_White3.png";
        private const string SP_BTN_CLOSE        = SC_BASE + "Button_Custom/Btn_IconButton_Square01_White1.png";

        // Decorations
        private const string SP_SELECTION_GLOW   = SC_BASE + "Frame_Custom/Frame_LineFrame03_White4_Glow.png";
        private const string SP_DIVIDER_LINE     = SC_BASE + "Frame_Custom/Frame_LineFrame05_White1.png";
        private const string SP_TITLE_LINE       = SC_BASE + "Frame_Custom/Frame_LIneFrame_TitleLIne01_White1.png";
        private const string SP_SLOT_FRAME       = SC_BASE + "Frame_Custom/Frame_ItemFrame01_n_White1.png";
        private const string SP_SLOT_BG          = SC_BASE + "Frame_Custom/Frame_ItemFrame01_00_White.png";
        private const string SP_CHECK            = SC_BASE + "Icon_PictoIcons(x2)/128/Icon_Check.Png";
        private const string SP_LOCK             = SC_BASE + "Icon_PictoIcons(x2)/128/Icon_Lock.Png";

        // Font
        private const string FONT_OXANIUM        = SC_FONT + "Oxanium-ExtraBold_Extended ASCII SDF.asset";
        private const string FONT_NEXON_B        = SC_FONT + "NEXON Football Gothic B SDF.asset";

        // ───────────────────────────────────────────────
        // COLOR PALETTE — SurvivalClean Style
        // ───────────────────────────────────────────────
        private static readonly Color COL_PANEL_BG       = new Color(0.05f, 0.08f, 0.14f, 0.97f);   // #0D1424
        private static readonly Color COL_INNER_BG       = new Color(0.08f, 0.11f, 0.19f, 0.96f);   // #141D30
        private static readonly Color COL_STAT_ROW_BG    = new Color(0.07f, 0.10f, 0.17f, 0.85f);   // stat row
        private static readonly Color COL_ACCENT_CYAN    = new Color(0f, 0.85f, 1f, 1f);             // #00D9FF
        private static readonly Color COL_ACCENT_GREEN   = new Color(0.18f, 0.80f, 0.44f, 1f);       // #2ECC71
        private static readonly Color COL_ACCENT_GOLD    = new Color(0.96f, 0.74f, 0.18f, 1f);       // #F5BD2F
        private static readonly Color COL_ACCENT_ORANGE  = new Color(0.90f, 0.49f, 0.13f, 1f);       // #E67E22
        private static readonly Color COL_BTN_BLUE       = new Color(0.10f, 0.56f, 0.89f, 1f);       // #1A8FE3
        private static readonly Color COL_TEXT_PRIMARY    = Color.white;
        private static readonly Color COL_TEXT_SECONDARY  = new Color(0.72f, 0.77f, 0.84f, 1f);       // #B8C4D6
        private static readonly Color COL_STAT_BONUS     = new Color(0.18f, 0.94f, 0.45f, 1f);       // #2EF073
        private static readonly Color COL_DIVIDER_CYAN   = new Color(0f, 0.85f, 1f, 0.45f);
        private static readonly Color COL_PURPLE_BAR     = new Color(0.55f, 0.25f, 0.95f, 1f);
        private static readonly Color COL_WARNING_GOLD   = new Color(1f, 0.80f, 0.30f, 1f);

        // ───────────────────────────────────────────────
        // UI HELPERS (Layer 5 enforcement)
        // ───────────────────────────────────────────────
        /// <summary>Create a GameObject on the UI layer (Layer 5) with optional parent and components.</summary>
        public static GameObject CreateUIObject(string name, Transform parent, params System.Type[] components)
        {
            GameObject obj = new GameObject(name, components);
            obj.layer = 5; // Layer 5 is standard Unity UI layer
            if (parent != null) obj.transform.SetParent(parent, false);
            return obj;
        }

        /// <summary>Recursively assign a layer to a GameObject and all its descendants.</summary>
        public static void SetLayerRecursively(GameObject obj, int layer)
        {
            if (obj == null) return;
            obj.layer = layer;
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                SetLayerRecursively(obj.transform.GetChild(i).gameObject, layer);
            }
        }

        // ───────────────────────────────────────────────
        // SPRITE CACHE
        // ───────────────────────────────────────────────
        private static Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();
        private static TMP_FontAsset _cachedFont;

        /// <summary>
        /// Collect all sprites currently used in the hierarchy (called BEFORE destroying any objects)
        /// so they can be reused when rebuilding at runtime (when AssetDatabase is unavailable).
        /// </summary>
        private static void CollectSpritesFromHierarchy(UIWeaponPage page)
        {
            _spriteCache.Clear();
            foreach (var img in page.GetComponentsInChildren<Image>(true))
            {
                if (img.sprite != null && !_spriteCache.ContainsKey(img.sprite.name))
                    _spriteCache[img.sprite.name] = img.sprite;
            }
        }

        /// <summary>Get a sprite by its asset name from the cache.</summary>
        public static Sprite GetCachedSprite(string spriteName)
        {
            if (_spriteCache.TryGetValue(spriteName, out Sprite s)) return s;
            return null;
        }

        /// <summary>Get a sprite — tries cache first, then fallback sprite.</summary>
        public static Sprite GetSprite(string spriteName, Sprite fallback = null)
        {
            Sprite s = GetCachedSprite(spriteName);
            return s != null ? s : fallback;
        }

        /// <summary>Get SurvivalClean sprite by name (with Editor fallback search if needed).</summary>
        public static Sprite GetSurvivalCleanSprite(string spriteName)
        {
            Sprite s = GetCachedSprite(spriteName);
            if (s != null) return s;

#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"{spriteName} t:Sprite");
            if (guids != null && guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                Sprite loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (loaded != null)
                {
                    _spriteCache[spriteName] = loaded;
                    return loaded;
                }
            }
#endif
            return null;
        }

        /// <summary>Get the cached Oxanium/SurvivalClean font.</summary>
        public static TMP_FontAsset GetFont()
        {
            if (_cachedFont != null) return _cachedFont;

#if UNITY_EDITOR
            _cachedFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_OXANIUM);
            if (_cachedFont == null)
                _cachedFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_NEXON_B);
#endif
            return _cachedFont;
        }

#if UNITY_EDITOR
        /// <summary>Load SurvivalClean sprites via AssetDatabase (Editor only) and populate the cache.</summary>
        public static void LoadSurvivalCleanSprites()
        {
            LoadEditorSprite(SP_PANEL_BG);
            LoadEditorSprite(SP_INNER_PANEL);
            LoadEditorSprite(SP_STAT_ROW_BG);
            LoadEditorSprite(SP_WEAPON_CARD_BG);
            LoadEditorSprite(SP_CARD_FRAME_BG);
            LoadEditorSprite(SP_BAR_FRAME);
            LoadEditorSprite(SP_BAR_FILL_AREA);
            LoadEditorSprite(SP_BAR_FILL);
            LoadEditorSprite(SP_RARITY_BADGE);
            LoadEditorSprite(SP_MAX_LEVEL);
            LoadEditorSprite(SP_BTN_PRIMARY);
            LoadEditorSprite(SP_BTN_SECONDARY);
            LoadEditorSprite(SP_BTN_TERTIARY);
            LoadEditorSprite(SP_BTN_CLOSE);
            LoadEditorSprite(SP_SELECTION_GLOW);
            LoadEditorSprite(SP_DIVIDER_LINE);
            LoadEditorSprite(SP_TITLE_LINE);
            LoadEditorSprite(SP_SLOT_FRAME);
            LoadEditorSprite(SP_SLOT_BG);
            LoadEditorSprite(SP_CHECK);
            LoadEditorSprite(SP_LOCK);

            // Font
            _cachedFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_OXANIUM);
            if (_cachedFont == null)
                _cachedFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_NEXON_B);
        }

        private static void LoadEditorSprite(string path)
        {
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null && !_spriteCache.ContainsKey(s.name))
                _spriteCache[s.name] = s;
        }
#endif

        /// <summary>Find the best available font — Oxanium > existing hierarchy font > null.</summary>
        private static TMP_FontAsset FindFont(UIWeaponPage page)
        {
            if (_cachedFont != null) return _cachedFont;

            foreach (var t in page.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (t.font != null && t.font.name.Contains("Oxanium"))
                {
                    _cachedFont = t.font;
                    return _cachedFont;
                }
            }

            foreach (var t in page.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (t.font != null)
                {
                    _cachedFont = t.font;
                    return _cachedFont;
                }
            }

            return null;
        }

        // ───────────────────────────────────────────────
        // MAIN BUILD ENTRY POINT
        // ───────────────────────────────────────────────
        public static void BuildLayout(UIWeaponPage page)
        {
            if (page == null) return;

            // Ensure root scale is 1, 1, 1 and layer is 5 (UI)
            page.transform.localScale = Vector3.one;
            page.gameObject.layer = 5;

            // 0. COLLECT EXISTING SPRITES (before destroying anything)
            CollectSpritesFromHierarchy(page);

#if UNITY_EDITOR
            LoadSurvivalCleanSprites();
#endif

            // Resolve sprites
            Sprite panelBgSprite   = GetCachedSprite("Frame_Frame01_White1");
            Sprite innerBgSprite   = GetCachedSprite("Frame_Frame02_White1");
            Sprite statRowSprite   = GetCachedSprite("Frame_ListFrame01_White1");
            Sprite barFrameSprite  = GetCachedSprite("Slider06_White1_Frame");
            Sprite barFillSprite   = GetCachedSprite("Slider06_White3_Fill1");
            Sprite raritySprite    = GetCachedSprite("Label_Label01_White1");
            Sprite btnPrimary      = GetCachedSprite("Btn_TextButton_Square01_White1");
            Sprite btnSecondary    = GetCachedSprite("Btn_TextButton_Square01_White2");
            Sprite btnTertiary     = GetCachedSprite("Btn_TextButton_Square01_White3");
            Sprite maxLevelSprite  = GetCachedSprite("Label_Label04_White1");
            Sprite dividerSprite   = GetCachedSprite("Frame_LineFrame05_White1");
            Sprite cardFrameSprite = GetCachedSprite("Frame_CardFrame02_White1");

            // Fallback: grab any sliced sprite from hierarchy if SC sprites not found
            Sprite fallbackSliced = null;
            foreach (var img in page.GetComponentsInChildren<Image>(true))
            {
                if (img.sprite != null && img.type == Image.Type.Sliced)
                {
                    fallbackSliced = img.sprite;
                    break;
                }
            }

            if (panelBgSprite == null) panelBgSprite = fallbackSliced;
            if (innerBgSprite == null) innerBgSprite = fallbackSliced;
            if (statRowSprite == null) statRowSprite = fallbackSliced;
            if (barFrameSprite == null) barFrameSprite = fallbackSliced;
            if (barFillSprite == null) barFillSprite = fallbackSliced;
            if (raritySprite == null) raritySprite = fallbackSliced;
            if (btnPrimary == null) btnPrimary = fallbackSliced;
            if (btnSecondary == null) btnSecondary = fallbackSliced;
            if (btnTertiary == null) btnTertiary = fallbackSliced;
            if (maxLevelSprite == null) maxLevelSprite = fallbackSliced;
            if (dividerSprite == null) dividerSprite = fallbackSliced;
            if (cardFrameSprite == null) cardFrameSprite = fallbackSliced;

            TMP_FontAsset font = FindFont(page);

            // 1. RECONFIGURE RIGHT PANEL (Back Panel / Weapon Selector List) - 560px WIDTH FULL HEIGHT FOR x3 BIG TEXT
            RectTransform backPanelRect = page.BackgroundPanelRectTransform;
            if (backPanelRect == null)
            {
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var bgPanelField = typeof(UIWeaponPage).BaseType.GetField("backgroundPanelRectTransform", flags);
                if (bgPanelField != null)
                    backPanelRect = bgPanelField.GetValue(page) as RectTransform;
            }

            if (backPanelRect != null)
            {
                backPanelRect.gameObject.layer = 5;
                backPanelRect.anchorMin = new Vector2(1f, 0f);
                backPanelRect.anchorMax = new Vector2(1f, 1f);
                backPanelRect.pivot = new Vector2(1f, 0.5f);
                backPanelRect.sizeDelta = new Vector2(560f, 0f);
                backPanelRect.anchoredPosition = new Vector2(-50f, 0f);
                backPanelRect.localScale = Vector3.one;

                Image bgImg = backPanelRect.GetComponent<Image>();
                if (bgImg == null) bgImg = backPanelRect.gameObject.AddComponent<Image>();
                SetSprite(bgImg, panelBgSprite);
                bgImg.color = COL_PANEL_BG;

                // Neon Divider Line (left edge)
                Transform oldDivider = backPanelRect.Find("Neon Divider");
                if (oldDivider != null) Object.DestroyImmediate(oldDivider.gameObject);
                GameObject dividerObj = CreateUIObject("Neon Divider", backPanelRect, typeof(RectTransform), typeof(Image));
                RectTransform divRt = dividerObj.GetComponent<RectTransform>();
                divRt.anchorMin = new Vector2(0f, 0f);
                divRt.anchorMax = new Vector2(0f, 1f);
                divRt.pivot = new Vector2(0f, 0.5f);
                divRt.sizeDelta = new Vector2(4f, 0f);
                divRt.anchoredPosition = Vector2.zero;
                Image divImg = dividerObj.GetComponent<Image>();
                SetSprite(divImg, dividerSprite);
                divImg.color = COL_DIVIDER_CYAN;

                // Title ("KHO VŨ KHÍ" — x3 BIGGER 72px)
                Transform titleTr = backPanelRect.Find("Title");
                if (titleTr != null)
                {
                    titleTr.gameObject.layer = 5;
                    RectTransform titleRt = titleTr.GetComponent<RectTransform>();
                    titleRt.anchorMin = new Vector2(0f, 1f);
                    titleRt.anchorMax = new Vector2(1f, 1f);
                    titleRt.pivot = new Vector2(0.5f, 1f);
                    titleRt.anchoredPosition = new Vector2(-36f, -16f);
                    titleRt.sizeDelta = new Vector2(-140f, 85f);

                    TextMeshProUGUI titleTMP = titleTr.GetComponentInChildren<TextMeshProUGUI>();
                    if (titleTMP != null)
                    {
                        titleTMP.text = "KHO VŨ KHÍ";
                        titleTMP.fontSize = 72f;
                        titleTMP.fontStyle = FontStyles.Bold;
                        titleTMP.alignment = TextAlignmentOptions.Center;
                        titleTMP.textWrappingMode = TextWrappingModes.NoWrap;
                        titleTMP.color = COL_ACCENT_CYAN;
                        if (font != null) titleTMP.font = font;
                    }
                }

                // Close Button ("X")
                Transform closeBtnTr = backPanelRect.Find("Back Button");
                if (closeBtnTr != null)
                {
                    closeBtnTr.gameObject.layer = 5;
                    RectTransform closeRt = closeBtnTr.GetComponent<RectTransform>();
                    closeRt.anchorMin = new Vector2(1f, 1f);
                    closeRt.anchorMax = new Vector2(1f, 1f);
                    closeRt.pivot = new Vector2(1f, 1f);
                    closeRt.anchoredPosition = new Vector2(-16f, -16f);
                    closeRt.sizeDelta = new Vector2(68f, 68f);

                    Image closeImg = closeBtnTr.GetComponent<Image>();
                    Sprite closeBtnSprite = GetCachedSprite("Btn_IconButton_Square01_White1");
                    if (closeImg != null && closeBtnSprite != null)
                    {
                        SetSprite(closeImg, closeBtnSprite);
                        closeImg.color = COL_BTN_BLUE;
                    }
                }

                // Inner panel holding the ScrollView
                Transform innerPanelTr = backPanelRect.Find("Panel");
                if (innerPanelTr != null)
                {
                    innerPanelTr.gameObject.layer = 5;
                    RectTransform innerPanelRt = innerPanelTr.GetComponent<RectTransform>();
                    innerPanelRt.anchorMin = Vector2.zero;
                    innerPanelRt.anchorMax = Vector2.one;
                    innerPanelRt.pivot = new Vector2(0.5f, 0.5f);
                    innerPanelRt.anchoredPosition = new Vector2(0f, -52f);
                    innerPanelRt.sizeDelta = new Vector2(-24f, -116f);

                    Image innerBg = innerPanelTr.GetComponent<Image>();
                    if (innerBg != null)
                    {
                        SetSprite(innerBg, innerBgSprite);
                        innerBg.color = COL_INNER_BG;
                    }

                    Transform outlineTop = innerPanelTr.Find("Outline Top");
                    if (outlineTop != null) outlineTop.gameObject.SetActive(false);
                }
            }

            // ScrollRect adjustments for Vertical List
            ScrollRect scrollView = page.ScrollView;
            if (scrollView == null) scrollView = page.GetComponentInChildren<ScrollRect>(true);
            if (scrollView != null)
            {
                scrollView.gameObject.layer = 5;
                RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
                scrollRect.anchorMin = Vector2.zero;
                scrollRect.anchorMax = Vector2.one;
                scrollRect.pivot = new Vector2(0.5f, 0.5f);
                scrollRect.anchoredPosition = Vector2.zero;
                scrollRect.sizeDelta = new Vector2(-10f, -10f);

                scrollView.horizontal = false;
                scrollView.vertical = true;
                scrollView.movementType = ScrollRect.MovementType.Elastic;

                if (scrollView.viewport != null)
                {
                    scrollView.viewport.gameObject.layer = 5;
                    scrollView.viewport.anchorMin = Vector2.zero;
                    scrollView.viewport.anchorMax = Vector2.one;
                    scrollView.viewport.anchoredPosition = Vector2.zero;
                    scrollView.viewport.sizeDelta = Vector2.zero;
                }

                Transform containerTr = page.PanelsContainer;
                if (containerTr == null && scrollView.content != null)
                    containerTr = scrollView.content;

                if (containerTr != null)
                {
                    containerTr.gameObject.layer = 5;
                    RectTransform contentRt = containerTr.GetComponent<RectTransform>();
                    contentRt.anchorMin = new Vector2(0f, 1f);
                    contentRt.anchorMax = new Vector2(1f, 1f);
                    contentRt.pivot = new Vector2(0.5f, 1f);
                    contentRt.anchoredPosition = Vector2.zero;
                    contentRt.sizeDelta = Vector2.zero;

                    // Remove old layout groups
                    HorizontalLayoutGroup oldHlg = containerTr.GetComponent<HorizontalLayoutGroup>();
                    if (oldHlg != null) Object.DestroyImmediate(oldHlg);
                    GridLayoutGroup oldGlg = containerTr.GetComponent<GridLayoutGroup>();
                    if (oldGlg != null) Object.DestroyImmediate(oldGlg);

                    VerticalLayoutGroup vlg = containerTr.GetComponent<VerticalLayoutGroup>();
                    if (vlg == null) vlg = containerTr.gameObject.AddComponent<VerticalLayoutGroup>();
                    if (vlg != null)
                    {
                        vlg.spacing = 12f;
                        vlg.padding = new RectOffset(8, 8, 10, 10);
                        vlg.childAlignment = TextAnchor.UpperCenter;
                        vlg.childControlWidth = true;
                        vlg.childControlHeight = false;
                        vlg.childForceExpandWidth = true;
                        vlg.childForceExpandHeight = false;
                    }

                    ContentSizeFitter fitter = containerTr.GetComponent<ContentSizeFitter>();
                    if (fitter == null) fitter = containerTr.gameObject.AddComponent<ContentSizeFitter>();
                    if (fitter != null)
                    {
                        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    }

                    // Clear existing children when rebuilding at runtime
                    if (Application.isPlaying)
                    {
                        for (int i = containerTr.childCount - 1; i >= 0; i--)
                        {
                            Object.Destroy(containerTr.GetChild(i).gameObject);
                        }
                    }
                }

                if (scrollView.horizontalScrollbar != null)
                    scrollView.horizontalScrollbar.gameObject.SetActive(false);
            }

            // 2. DESTROY ANY PREVIOUS LEFT PANEL
            for (int i = page.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = page.transform.GetChild(i);
                if (child.name == "Left Weapon Details Panel")
                {
                    if (Application.isPlaying) Object.Destroy(child.gameObject);
                    else Object.DestroyImmediate(child.gameObject);
                }
            }

            // 3. CREATE FULL-HEIGHT DOCKED LEFT DETAILS PANEL
            UIWeaponDetailsPanel detailsPanel = CreateDetailsPanel(page, font,
                panelBgSprite, innerBgSprite, statRowSprite, barFrameSprite, barFillSprite,
                raritySprite, btnPrimary, btnSecondary, btnTertiary, maxLevelSprite, dividerSprite, cardFrameSprite);

            // 4. LINK REFERENCES INTO UIWEAPONPAGE
            var flagsRefl = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var detailsField = typeof(UIWeaponPage).GetField("detailsPanel", flagsRefl);
            if (detailsField != null)
                detailsField.SetValue(page, detailsPanel);

            var leftRectField = typeof(UIWeaponPage).GetField("leftPanelRectTransform", flagsRefl);
            if (leftRectField != null && detailsPanel != null)
                leftRectField.SetValue(page, detailsPanel.GetComponent<RectTransform>());

            var rightRectField = typeof(UIWeaponPage).GetField("rightPanelRectTransform", flagsRefl);
            if (rightRectField != null && backPanelRect != null)
                rightRectField.SetValue(page, backPanelRect);

            // Final layer and scale safety
            SetLayerRecursively(page.gameObject, 5);
            page.transform.localScale = Vector3.one;
        }

        // ───────────────────────────────────────────────
        // CREATE LEFT DETAILS PANEL
        // ───────────────────────────────────────────────
        // ───────────────────────────────────────────────
        // CREATE LEFT DETAILS PANEL (x3 SCALED)
        // ───────────────────────────────────────────────
        private static UIWeaponDetailsPanel CreateDetailsPanel(
            UIWeaponPage page, TMP_FontAsset font,
            Sprite panelBg, Sprite innerBg, Sprite statRowBg,
            Sprite barFrame, Sprite barFill,
            Sprite rarityBadgeSp, Sprite btnPrimary, Sprite btnSecondary, Sprite btnTertiary,
            Sprite maxLevelSp, Sprite dividerSp, Sprite cardFrameSp)
        {
            GameObject detailsObj = CreateUIObject("Left Weapon Details Panel", page.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(UIWeaponDetailsPanel));

            UIWeaponDetailsPanel detailsPanel = detailsObj.GetComponent<UIWeaponDetailsPanel>();

            RectTransform rootRect = detailsObj.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 0f);
            rootRect.anchorMax = new Vector2(0f, 1f);
            rootRect.pivot = new Vector2(0f, 0.5f);
            rootRect.sizeDelta = new Vector2(620f, 0f);
            rootRect.anchoredPosition = new Vector2(60f, 0f);
            rootRect.localScale = Vector3.one;

            Image bgImage = detailsObj.GetComponent<Image>();
            SetSprite(bgImage, panelBg);
            bgImage.color = COL_PANEL_BG;

            // Right Neon Divider Line
            GameObject dividerObj = CreateUIObject("Neon Divider", detailsObj.transform, typeof(RectTransform), typeof(Image));
            RectTransform divRt = dividerObj.GetComponent<RectTransform>();
            divRt.anchorMin = new Vector2(1f, 0f);
            divRt.anchorMax = new Vector2(1f, 1f);
            divRt.pivot = new Vector2(1f, 0.5f);
            divRt.sizeDelta = new Vector2(4f, 0f);
            divRt.anchoredPosition = Vector2.zero;
            Image divImg = dividerObj.GetComponent<Image>();
            SetSprite(divImg, dividerSp);
            divImg.color = COL_DIVIDER_CYAN;

            // Content container with safe padding
            VerticalLayoutGroup vlg = detailsObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(26, 26, 26, 26);
            vlg.spacing = 14;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // ─── HEADER ───
            GameObject headerObj = CreateUIObject("Header", detailsObj.transform, typeof(RectTransform), typeof(VerticalLayoutGroup));
            VerticalLayoutGroup headerVlg = headerObj.GetComponent<VerticalLayoutGroup>();
            headerVlg.spacing = 6;
            headerVlg.childAlignment = TextAnchor.MiddleLeft;
            headerVlg.childControlWidth = true;
            headerVlg.childControlHeight = false;
            headerObj.GetComponent<RectTransform>().sizeDelta = new Vector2(568, 156);

            TextMeshProUGUI weaponName = CreateTMP(headerObj.transform, "WeaponName", "MINIGUN", 90, font, FontStyles.Bold, COL_TEXT_PRIMARY, TextAlignmentOptions.Left, 568);
            weaponName.enableAutoSizing = true;
            weaponName.fontSizeMin = 52f;
            weaponName.fontSizeMax = 90f;

            GameObject subHeader = CreateUIObject("SubHeader", headerObj.transform, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            HorizontalLayoutGroup subHlg = subHeader.GetComponent<HorizontalLayoutGroup>();
            subHlg.spacing = 16;
            subHlg.childAlignment = TextAnchor.MiddleLeft;
            subHlg.childControlWidth = false;
            subHlg.childControlHeight = false;
            subHeader.GetComponent<RectTransform>().sizeDelta = new Vector2(568, 54);

            // Rarity Badge — x3 BIGGER (210x52, 36px font)
            GameObject rarityBadge = CreateUIObject("Rarity Badge", subHeader.transform, typeof(RectTransform), typeof(Image));
            rarityBadge.GetComponent<RectTransform>().sizeDelta = new Vector2(210, 52);
            Image rarityImg = rarityBadge.GetComponent<Image>();
            SetSprite(rarityImg, rarityBadgeSp);
            rarityImg.color = new Color(0.2f, 0.6f, 1f);

            TextMeshProUGUI rarityTMP = CreateTMP(rarityBadge.transform, "RarityText", "LEGENDARY", 36, font, FontStyles.Bold, COL_TEXT_PRIMARY, TextAlignmentOptions.Center);
            rarityTMP.rectTransform.anchorMin = Vector2.zero;
            rarityTMP.rectTransform.anchorMax = Vector2.one;
            rarityTMP.rectTransform.sizeDelta = Vector2.zero;

            // Level Text — x3 BIGGER (48px font)
            TextMeshProUGUI levelTMP = CreateTMP(subHeader.transform, "LevelText", "CẤP 1", 48, font, FontStyles.Bold, COL_ACCENT_CYAN, TextAlignmentOptions.Left, 240);

            // ─── CARDS PROGRESS CONTAINER ───
            GameObject cardsObj = CreateUIObject("Cards Container", detailsObj.transform, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(Image));
            cardsObj.GetComponent<RectTransform>().sizeDelta = new Vector2(568, 156);
            Image cardsBg = cardsObj.GetComponent<Image>();
            SetSprite(cardsBg, cardFrameSp != null ? cardFrameSp : innerBg);
            cardsBg.color = COL_INNER_BG;

            VerticalLayoutGroup cardsVlg = cardsObj.GetComponent<VerticalLayoutGroup>();
            cardsVlg.padding = new RectOffset(22, 22, 14, 14);
            cardsVlg.spacing = 10;
            cardsVlg.childAlignment = TextAnchor.UpperCenter;
            cardsVlg.childControlWidth = true;
            cardsVlg.childControlHeight = false;

            GameObject cardRow = CreateUIObject("Card Row", cardsObj.transform, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            HorizontalLayoutGroup cardRowHlg = cardRow.GetComponent<HorizontalLayoutGroup>();
            cardRowHlg.childAlignment = TextAnchor.MiddleCenter;
            cardRowHlg.childControlWidth = false;
            cardRowHlg.childControlHeight = false;
            cardRow.GetComponent<RectTransform>().sizeDelta = new Vector2(524, 46);

            CreateTMP(cardRow.transform, "Label", "TIẾN ĐỘ THẺ", 40, font, FontStyles.Bold, COL_TEXT_SECONDARY, TextAlignmentOptions.Left, 260);
            TextMeshProUGUI cardsAmount = CreateTMP(cardRow.transform, "Amount", "12 / 20 THẺ", 44, font, FontStyles.Bold, COL_TEXT_PRIMARY, TextAlignmentOptions.Right, 250);

            // Card Fill Bar — SurvivalClean Slider Style
            GameObject barBg = CreateUIObject("Bar Bg", cardsObj.transform, typeof(RectTransform), typeof(Image));
            barBg.GetComponent<RectTransform>().sizeDelta = new Vector2(524, 26);
            Image barBgImg = barBg.GetComponent<Image>();
            SetSprite(barBgImg, barFrame);
            barBgImg.color = new Color(0.06f, 0.08f, 0.14f, 0.95f);

            GameObject barFillObj = CreateUIObject("Bar Fill", barBg.transform, typeof(RectTransform), typeof(Image));
            RectTransform fillRt = barFillObj.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;
            Image fillImg = barFillObj.GetComponent<Image>();
            if (barFill != null) { fillImg.sprite = barFill; fillImg.type = Image.Type.Filled; }
            else fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.color = COL_PURPLE_BAR;

            TextMeshProUGUI cardsStatus = CreateTMP(cardsObj.transform, "Status", "Cần thêm thẻ để nâng cấp", 32, font, FontStyles.Italic, COL_WARNING_GOLD, TextAlignmentOptions.Left, 524);

            // ─── STATS HUD CONTAINER ───
            GameObject statsObj = CreateUIObject("Stats Container", detailsObj.transform, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(Image));
            statsObj.GetComponent<RectTransform>().sizeDelta = new Vector2(568, 320);
            Image statsBg = statsObj.GetComponent<Image>();
            SetSprite(statsBg, innerBg);
            statsBg.color = COL_INNER_BG;

            VerticalLayoutGroup statsVlg = statsObj.GetComponent<VerticalLayoutGroup>();
            statsVlg.padding = new RectOffset(22, 22, 14, 14);
            statsVlg.spacing = 10;
            statsVlg.childAlignment = TextAnchor.UpperCenter;
            statsVlg.childControlWidth = true;
            statsVlg.childControlHeight = false;

            CreateStatRow(statsObj.transform, "SÁT THƯƠNG", font, statRowBg, out TextMeshProUGUI dmgVal, out TextMeshProUGUI dmgBonus);
            CreateStatRow(statsObj.transform, "TỐC ĐỘ BẮN", font, statRowBg, out TextMeshProUGUI fireVal, out TextMeshProUGUI fireBonus);
            CreateStatRow(statsObj.transform, "TẦM BẮN", font, statRowBg, out TextMeshProUGUI rangeVal, out _);
            CreateStatRow(statsObj.transform, "LỰC CHIẾN", font, statRowBg, out TextMeshProUGUI powerVal, out TextMeshProUGUI powerBonus);

            // Hidden stat rows for BindReferences compatibility
            CreateStatRow(statsObj.transform, "ĐỘ GIẬT", font, statRowBg, out TextMeshProUGUI spreadVal, out _);
            spreadVal.transform.parent.gameObject.SetActive(false);
            CreateStatRow(statsObj.transform, "SỐ ĐẠN BẮN", font, statRowBg, out TextMeshProUGUI bulletsVal, out _);
            bulletsVal.transform.parent.gameObject.SetActive(false);

            // ─── UPGRADES CONTAINER ───
            GameObject upgradesObj = CreateUIObject("Upgrades Container", detailsObj.transform, typeof(RectTransform), typeof(VerticalLayoutGroup));
            upgradesObj.GetComponent<RectTransform>().sizeDelta = new Vector2(568, 100);
            VerticalLayoutGroup upgVlg = upgradesObj.GetComponent<VerticalLayoutGroup>();
            upgVlg.spacing = 12;
            upgVlg.childAlignment = TextAnchor.UpperCenter;
            upgVlg.childControlWidth = true;
            upgVlg.childControlHeight = false;

            // Coin Upgrade Button — SurvivalClean gold style
            Button coinBtn = CreateActionButton(upgradesObj.transform, "Coin Upgrade Button", "NÂNG CẤP BẰNG VÀNG",
                COL_ACCENT_GOLD, GetCoinIcon(page), btnPrimary, font, out Image coinBtnImg, out TextMeshProUGUI coinPriceText);

            // Card Unlock Button — SurvivalClean green style
            Button unlockBtn = CreateActionButton(upgradesObj.transform, "Card Unlock Button", "MỞ KHÓA VŨ KHÍ",
                COL_ACCENT_GREEN, null, btnSecondary, font, out Image unlockBtnImg, out TextMeshProUGUI unlockBtnText);
            unlockBtn.gameObject.SetActive(false);

            // Max Level Banner
            GameObject maxBanner = CreateUIObject("Max Level Banner", upgradesObj.transform, typeof(RectTransform), typeof(Image));
            maxBanner.GetComponent<RectTransform>().sizeDelta = new Vector2(568, 80);
            Image maxBg = maxBanner.GetComponent<Image>();
            SetSprite(maxBg, maxLevelSp);
            maxBg.color = new Color(0.20f, 0.24f, 0.35f, 0.96f);
            CreateTMP(maxBanner.transform, "MaxText", "ĐẠT CẤP TỐI ĐA", 44, font, FontStyles.Bold, COL_WARNING_GOLD, TextAlignmentOptions.Center);
            maxBanner.SetActive(false);

            // ─── EQUIP BUTTON ("TRANG BỊ") ───
            GameObject equipCont = CreateUIObject("Equip Container", detailsObj.transform, typeof(RectTransform), typeof(VerticalLayoutGroup));
            equipCont.GetComponent<RectTransform>().sizeDelta = new Vector2(568, 100);
            VerticalLayoutGroup eqVlg = equipCont.GetComponent<VerticalLayoutGroup>();
            eqVlg.childAlignment = TextAnchor.MiddleCenter;
            eqVlg.childControlWidth = true;
            eqVlg.childControlHeight = false;

            Button equipBtn = CreateActionButton(equipCont.transform, "Equip Button", "TRANG BỊ",
                COL_BTN_BLUE, null, btnTertiary, font, out Image equipBtnImg, out TextMeshProUGUI equipBtnText);
            equipBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(568, 92);
            equipBtnText.fontSize = 54f;

            // Bind References
            detailsPanel.BindReferences(
                weaponName,
                rarityTMP,
                rarityImg,
                levelTMP,
                cardsObj,
                fillImg,
                cardsAmount,
                cardsStatus,
                powerVal,
                powerBonus,
                dmgVal,
                dmgBonus,
                fireVal,
                fireBonus,
                rangeVal,
                spreadVal,
                bulletsVal,
                upgradesObj,
                coinBtn,
                coinBtnImg,
                coinPriceText,
                unlockBtn,
                unlockBtnImg,
                unlockBtnText,
                maxBanner,
                equipBtn,
                equipBtnImg,
                equipBtnText
            );

            return detailsPanel;
        }

        // ───────────────────────────────────────────────
        // STAT ROW — SurvivalClean framed row
        // ───────────────────────────────────────────────
        private static void CreateStatRow(Transform parent, string title, TMP_FontAsset font, Sprite rowBgSprite, out TextMeshProUGUI valText, out TextMeshProUGUI bonusText)
        {
            GameObject row = CreateUIObject($"Row_{title}", parent, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(Image));
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(524, 60);

            Image rowBg = row.GetComponent<Image>();
            SetSprite(rowBg, rowBgSprite);
            rowBg.color = COL_STAT_ROW_BG;

            HorizontalLayoutGroup hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.padding = new RectOffset(18, 18, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            CreateTMP(row.transform, "Label", title, 42, font, FontStyles.Bold, COL_TEXT_SECONDARY, TextAlignmentOptions.Left, 250);
            valText = CreateTMP(row.transform, "Value", "100", 52, font, FontStyles.Bold, COL_TEXT_PRIMARY, TextAlignmentOptions.Right, 160);
            bonusText = CreateTMP(row.transform, "Bonus", "+10", 42, font, FontStyles.Bold, COL_STAT_BONUS, TextAlignmentOptions.Left, 95);
            bonusText.gameObject.SetActive(false);
        }

        // ───────────────────────────────────────────────
        // ACTION BUTTON — SurvivalClean styled
        // ───────────────────────────────────────────────
        private static Button CreateActionButton(Transform parent, string name, string label, Color color, Sprite icon, Sprite bgSprite, TMP_FontAsset font, out Image btnImg, out TextMeshProUGUI labelTmp)
        {
            GameObject btnObj = CreateUIObject(name, parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(568, 92);

            btnImg = btnObj.GetComponent<Image>();
            SetSprite(btnImg, bgSprite);
            btnImg.color = color;

            Button btn = btnObj.GetComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(Mathf.Min(color.r * 1.2f, 1f), Mathf.Min(color.g * 1.2f, 1f), Mathf.Min(color.b * 1.2f, 1f), 1f);
            cb.pressedColor = new Color(color.r * 0.80f, color.g * 0.80f, color.b * 0.80f, 1f);
            cb.disabledColor = new Color(0.30f, 0.33f, 0.38f, 0.55f);
            btn.colors = cb;

            GameObject cont = CreateUIObject("Container", btnObj.transform, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            RectTransform contRt = cont.GetComponent<RectTransform>();
            contRt.anchorMin = Vector2.zero;
            contRt.anchorMax = Vector2.one;
            contRt.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup hlg = cont.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            if (icon != null)
            {
                GameObject iconObj = CreateUIObject("Icon", cont.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                iconObj.GetComponent<RectTransform>().sizeDelta = new Vector2(48, 48);
                Image iconImg = iconObj.GetComponent<Image>();
                iconImg.sprite = icon;
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;
            }

            labelTmp = CreateTMP(cont.transform, "Label", label, 44, font, FontStyles.Bold, COL_TEXT_PRIMARY, TextAlignmentOptions.Center, 420);
            labelTmp.raycastTarget = false;

            return btn;
        }

        // ───────────────────────────────────────────────
        // TEXT HELPER
        // ───────────────────────────────────────────────
        private static TextMeshProUGUI CreateTMP(Transform parent, string name, string text, float size, TMP_FontAsset font, FontStyles style, Color color, TextAlignmentOptions align, float width = 0)
        {
            GameObject obj = CreateUIObject(name, parent, typeof(RectTransform), typeof(TextMeshProUGUI));

            RectTransform rt = obj.GetComponent<RectTransform>();
            if (width > 0) rt.sizeDelta = new Vector2(width, size + 20);
            else rt.sizeDelta = new Vector2(rt.sizeDelta.x, size + 20);

            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = align;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.raycastTarget = false;

            return tmp;
        }

        // ───────────────────────────────────────────────
        // SPRITE HELPER
        // ───────────────────────────────────────────────
        private static void SetSprite(Image img, Sprite sprite)
        {
            if (img == null) return;
            if (sprite != null)
            {
                img.sprite = sprite;
                img.type = Image.Type.Sliced;
            }
        }

        // ───────────────────────────────────────────────
        // COIN ICON FINDER
        // ───────────────────────────────────────────────
        private static Sprite GetCoinIcon(UIWeaponPage page)
        {
            if (Application.isPlaying)
            {
                try
                {
                    var curr = CurrenciesController.GetCurrency(CurrencyType.Coins);
                    if (curr != null) return curr.Icon;
                }
                catch {}
            }

            if (page != null)
            {
                Image[] allImages = page.GetComponentsInChildren<Image>(true);
                foreach (var img in allImages)
                {
                    if (img.sprite != null && img.sprite.name.ToLower().Contains("coin"))
                    {
                        return img.sprite;
                    }
                }
            }

#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite Coin");
            if (guids != null && guids.Length > 0)
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                    Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (s != null) return s;
                }
            }
#endif

            return null;
        }
    }
}

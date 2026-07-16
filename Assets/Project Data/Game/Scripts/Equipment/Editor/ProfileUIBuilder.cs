#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

namespace Watermelon.SquadShooter
{
    public class ProfileUIBuilder : EditorWindow
    {
        private const string PREFAB_PATH = "Assets/Project Data/Game/Prefabs/UI/Pages/UI Main Menu.prefab";

        [MenuItem("Tools/Squad Shooter/Profile UI Builder")]
        public static void ShowWindow()
        {
            GetWindow<ProfileUIBuilder>("Profile UI Builder").Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("USER PROFILE - UI BUILDER", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH) != null;

            if (prefabExists)
            {
                EditorGUILayout.HelpBox(
                    "San sang tao giao dien User Profile va Account Info Popup cho prefab UI Main Menu.\n" +
                    "Cong cu se tu dong thiet lap cau truc UI, tim kiem sprite phu hop, va gan references vao script.",
                    MessageType.Info);

                EditorGUILayout.Space(10);

                GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
                if (GUILayout.Button("BUILD USER PROFILE UI IN PREFAB", GUILayout.Height(50)))
                {
                    BuildProfileUI();
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                EditorGUILayout.HelpBox($"Khong tim thay prefab tai path: {PREFAB_PATH}", MessageType.Error);
            }
        }

        private void BuildProfileUI()
        {
            // 1. Load prefab contents
            GameObject menuPrefabObj = PrefabUtility.LoadPrefabContents(PREFAB_PATH);
            if (menuPrefabObj == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not load prefab contents!", "OK");
                return;
            }

            UIMainMenu mainMenu = menuPrefabObj.GetComponent<UIMainMenu>();
            if (mainMenu == null)
            {
                EditorUtility.DisplayDialog("Error", "Prefab is missing UIMainMenu script!", "OK");
                PrefabUtility.UnloadPrefabContents(menuPrefabObj);
                return;
            }

            // Clean up existing elements if already built previously
            Transform oldHeader = FindChildRecursive(menuPrefabObj.transform, "[USER PROFILE HEADER]");
            if (oldHeader != null) DestroyImmediate(oldHeader.gameObject);

            Transform oldPopup = FindChildRecursive(menuPrefabObj.transform, "[USER PROFILE POPUP]");
            if (oldPopup != null) DestroyImmediate(oldPopup.gameObject);

            // Find parent Notch Panel (Safe Area)
            Transform parentCanvas = menuPrefabObj.transform;
            Transform notchPanel = FindChildRecursive(menuPrefabObj.transform, "Notch Panel");
            Transform headerParent = notchPanel != null ? notchPanel : parentCanvas;

            // Search for some beautiful sprites in assets
            Sprite bgSprite = FindSprite("panel_background") ?? FindSprite("background_panel") ?? FindSprite("popup_background") ?? FindSprite("border_wood") ?? FindSprite("panel_rect");
            Sprite circleFrameSprite = FindSprite("circle_gold") ?? FindSprite("frame_circle") ?? FindSprite("avatar_circle") ?? FindSprite("circle_frame");
            Sprite editIconSprite = FindSprite("icon_pencil") ?? FindSprite("icon_edit") ?? FindSprite("pencil") ?? FindSprite("edit");
            Sprite closeIconSprite = FindSprite("button_close") ?? FindSprite("close_red") ?? FindSprite("close") ?? FindSprite("btn_close");
            Sprite swordIconSprite = FindSprite("icon_sword") ?? FindSprite("sword") ?? FindSprite("attack_icon");
            Sprite heartIconSprite = FindSprite("icon_heart") ?? FindSprite("heart") ?? FindSprite("health_icon");
            Sprite bgHeaderSprite = FindSprite("panel_short") ?? FindSprite("bar_bg") ?? FindSprite("panel_black_translucent");

            // -------------------------------------------------------------
            // A. CREATE HEADER PANEL (TOP-LEFT)
            // -------------------------------------------------------------
            GameObject headerRoot = new GameObject("[USER PROFILE HEADER]");
            headerRoot.transform.SetParent(headerParent, false);
            var headerRect = headerRoot.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(0, 1);
            headerRect.pivot = new Vector2(0, 1);
            headerRect.anchoredPosition = new Vector2(25, -25);
            headerRect.sizeDelta = new Vector2(260, 80);

            // Background
            GameObject headerBg = new GameObject("Bg");
            headerBg.transform.SetParent(headerRoot.transform, false);
            var bgHeaderRect = headerBg.AddComponent<RectTransform>();
            bgHeaderRect.anchorMin = Vector2.zero;
            bgHeaderRect.anchorMax = Vector2.one;
            bgHeaderRect.offsetMin = Vector2.zero;
            bgHeaderRect.offsetMax = Vector2.zero;
            var bgHeaderImg = headerBg.AddComponent<Image>();
            if (bgHeaderSprite != null)
            {
                bgHeaderImg.sprite = bgHeaderSprite;
                bgHeaderImg.type = Image.Type.Sliced;
            }
            else
            {
                bgHeaderImg.color = new Color(0f, 0f, 0f, 0.45f);
            }

            // Avatar Container / Button
            GameObject avatarHeaderBtnObj = new GameObject("AvatarButton");
            avatarHeaderBtnObj.transform.SetParent(headerRoot.transform, false);
            var avatarHeaderBtnRect = avatarHeaderBtnObj.AddComponent<RectTransform>();
            avatarHeaderBtnRect.anchorMin = new Vector2(0, 0.5f);
            avatarHeaderBtnRect.anchorMax = new Vector2(0, 0.5f);
            avatarHeaderBtnRect.pivot = new Vector2(0, 0.5f);
            avatarHeaderBtnRect.anchoredPosition = new Vector2(10, 0);
            avatarHeaderBtnRect.sizeDelta = new Vector2(65, 65);
            var avatarBtn = avatarHeaderBtnObj.AddComponent<Button>();

            // Golden circle frame for header avatar
            GameObject avatarHeaderFrameObj = new GameObject("Frame");
            avatarHeaderFrameObj.transform.SetParent(avatarHeaderBtnObj.transform, false);
            var avatarHeaderFrameRect = avatarHeaderFrameObj.AddComponent<RectTransform>();
            avatarHeaderFrameRect.anchorMin = Vector2.zero;
            avatarHeaderFrameRect.anchorMax = Vector2.one;
            avatarHeaderFrameRect.offsetMin = Vector2.zero;
            avatarHeaderFrameRect.offsetMax = Vector2.zero;
            var avatarHeaderFrameImg = avatarHeaderFrameObj.AddComponent<Image>();
            if (circleFrameSprite != null)
            {
                avatarHeaderFrameImg.sprite = circleFrameSprite;
            }
            else
            {
                avatarHeaderFrameImg.color = new Color(0.9f, 0.75f, 0.2f, 1f);
            }

            // Avatar actual image inside frame
            GameObject avatarHeaderIconObj = new GameObject("AvatarIcon");
            avatarHeaderIconObj.transform.SetParent(avatarHeaderFrameObj.transform, false);
            var avatarHeaderIconRect = avatarHeaderIconObj.AddComponent<RectTransform>();
            avatarHeaderIconRect.anchorMin = new Vector2(0.1f, 0.1f);
            avatarHeaderIconRect.anchorMax = new Vector2(0.9f, 0.9f);
            avatarHeaderIconRect.offsetMin = Vector2.zero;
            avatarHeaderIconRect.offsetMax = Vector2.zero;
            var avatarHeaderIconImg = avatarHeaderIconObj.AddComponent<Image>();
            avatarHeaderIconImg.color = Color.white;

            // Mask for avatar image to keep it rounded
            var mask = avatarHeaderFrameObj.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            // Name text next to avatar (Centred vertically)
            GameObject nameHeaderObj = new GameObject("NameText");
            nameHeaderObj.transform.SetParent(headerRoot.transform, false);
            var nameHeaderRect = nameHeaderObj.AddComponent<RectTransform>();
            nameHeaderRect.anchorMin = new Vector2(0, 0.5f);
            nameHeaderRect.anchorMax = new Vector2(1, 0.5f);
            nameHeaderRect.pivot = new Vector2(0, 0.5f);
            nameHeaderRect.anchoredPosition = new Vector2(85, 0);
            nameHeaderRect.sizeDelta = new Vector2(-95, 40);
            var nameHeaderTxt = nameHeaderObj.AddComponent<TextMeshProUGUI>();
            nameHeaderTxt.fontSize = 18;
            nameHeaderTxt.alignment = TextAlignmentOptions.MidlineLeft;
            nameHeaderTxt.fontStyle = FontStyles.Bold;
            nameHeaderTxt.color = Color.white;
            nameHeaderTxt.text = "User Name";

            // -------------------------------------------------------------
            // B. CREATE POPUP PANEL (CENTER)
            // -------------------------------------------------------------
            GameObject popupRoot = new GameObject("[USER PROFILE POPUP]");
            popupRoot.transform.SetParent(parentCanvas, false);
            var popupRootRect = popupRoot.AddComponent<RectTransform>();
            popupRootRect.anchorMin = Vector2.zero;
            popupRootRect.anchorMax = Vector2.one;
            popupRootRect.offsetMin = Vector2.zero;
            popupRootRect.offsetMax = Vector2.zero;
            popupRoot.SetActive(false); // Hidden by default

            // Dark Blocker Button
            GameObject blockerObj = new GameObject("Blocker");
            blockerObj.transform.SetParent(popupRoot.transform, false);
            var blockerRect = blockerObj.AddComponent<RectTransform>();
            blockerRect.anchorMin = Vector2.zero;
            blockerRect.anchorMax = Vector2.one;
            blockerRect.offsetMin = Vector2.zero;
            blockerRect.offsetMax = Vector2.zero;
            var blockerImg = blockerObj.AddComponent<Image>();
            blockerImg.color = new Color(0f, 0f, 0f, 0.65f);
            var blockerBtn = blockerObj.AddComponent<Button>();

            // Center Panel
            GameObject panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(popupRoot.transform, false);
            var panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(620, 340);
            var panelImg = panelObj.AddComponent<Image>();
            if (bgSprite != null)
            {
                panelImg.sprite = bgSprite;
                panelImg.type = Image.Type.Sliced;
            }
            else
            {
                panelImg.color = new Color(0.24f, 0.16f, 0.1f, 1f); // Wooden brown
            }

            // Outline/Border if no sprite
            if (bgSprite == null)
            {
                GameObject borderObj = new GameObject("Outline");
                borderObj.transform.SetParent(panelObj.transform, false);
                var borderRect = borderObj.AddComponent<RectTransform>();
                borderRect.anchorMin = Vector2.zero;
                borderRect.anchorMax = Vector2.one;
                borderRect.offsetMin = new Vector2(3, 3);
                borderRect.offsetMax = new Vector2(-3, -3);
                var borderImg = borderObj.AddComponent<Image>();
                borderImg.color = Color.clear;
                var outline = borderObj.AddComponent<Outline>();
                outline.effectColor = new Color(0.85f, 0.7f, 0.25f, 1f); // Gold outline
                outline.effectDistance = new Vector2(3, 3);
            }

            // Title Header Panel
            GameObject titleBgObj = new GameObject("TitleBg");
            titleBgObj.transform.SetParent(panelObj.transform, false);
            var titleBgRect = titleBgObj.AddComponent<RectTransform>();
            titleBgRect.anchorMin = new Vector2(0.5f, 1);
            titleBgRect.anchorMax = new Vector2(0.5f, 1);
            titleBgRect.pivot = new Vector2(0.5f, 1);
            titleBgRect.anchoredPosition = new Vector2(0, -10);
            titleBgRect.sizeDelta = new Vector2(320, 40);
            var titleBgImg = titleBgObj.AddComponent<Image>();
            titleBgImg.color = new Color(0.4f, 0.25f, 0.15f, 1f);

            GameObject titleTxtObj = new GameObject("TitleText");
            titleTxtObj.transform.SetParent(titleBgObj.transform, false);
            var titleTxtRect = titleTxtObj.AddComponent<RectTransform>();
            titleTxtRect.anchorMin = Vector2.zero;
            titleTxtRect.anchorMax = Vector2.one;
            titleTxtRect.offsetMin = Vector2.zero;
            titleTxtRect.offsetMax = Vector2.zero;
            var titleTxt = titleTxtObj.AddComponent<TextMeshProUGUI>();
            titleTxt.fontSize = 18;
            titleTxt.alignment = TextAlignmentOptions.Center;
            titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.color = new Color(0.95f, 0.85f, 0.4f, 1f); // Golden yellow
            titleTxt.text = "Thông tin tài khoản";

            // Large Avatar Circle Frame (Left Side)
            GameObject avatarFrameObj = new GameObject("AvatarFrame");
            avatarFrameObj.transform.SetParent(panelObj.transform, false);
            var avatarFrameRect = avatarFrameObj.AddComponent<RectTransform>();
            avatarFrameRect.anchorMin = new Vector2(0, 0.5f);
            avatarFrameRect.anchorMax = new Vector2(0, 0.5f);
            avatarFrameRect.pivot = new Vector2(0, 0.5f);
            avatarFrameRect.anchoredPosition = new Vector2(40, -15);
            avatarFrameRect.sizeDelta = new Vector2(170, 170);
            var avatarFrameImg = avatarFrameObj.AddComponent<Image>();
            if (circleFrameSprite != null)
            {
                avatarFrameImg.sprite = circleFrameSprite;
            }
            else
            {
                avatarFrameImg.color = new Color(0.9f, 0.75f, 0.2f, 1f);
            }

            GameObject avatarIconObj = new GameObject("AvatarIcon");
            avatarIconObj.transform.SetParent(avatarFrameObj.transform, false);
            var avatarIconRect = avatarIconObj.AddComponent<RectTransform>();
            avatarIconRect.anchorMin = new Vector2(0.12f, 0.12f);
            avatarIconRect.anchorMax = new Vector2(0.88f, 0.88f);
            avatarIconRect.offsetMin = Vector2.zero;
            avatarIconRect.offsetMax = Vector2.zero;
            var avatarIconImg = avatarIconObj.AddComponent<Image>();
            avatarIconImg.color = Color.white;

            var frameMask = avatarFrameObj.AddComponent<Mask>();
            frameMask.showMaskGraphic = true;

            // Name Container Box (Right Side)
            GameObject nameContainerObj = new GameObject("NameBox");
            nameContainerObj.transform.SetParent(panelObj.transform, false);
            var nameContainerRect = nameContainerObj.AddComponent<RectTransform>();
            nameContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
            nameContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
            nameContainerRect.pivot = new Vector2(0, 0.5f);
            nameContainerRect.anchoredPosition = new Vector2(-40, 45);
            nameContainerRect.sizeDelta = new Vector2(340, 50);
            var nameContainerImg = nameContainerObj.AddComponent<Image>();
            nameContainerImg.color = new Color(0.12f, 0.08f, 0.05f, 0.85f); // Dark edit box

            // Display Name Text inside Box
            GameObject nameTxtObj = new GameObject("NameText");
            nameTxtObj.transform.SetParent(nameContainerObj.transform, false);
            var nameTxtRect = nameTxtObj.AddComponent<RectTransform>();
            nameTxtRect.anchorMin = new Vector2(0, 0);
            nameTxtRect.anchorMax = new Vector2(1, 1);
            nameTxtRect.offsetMin = new Vector2(15, 0);
            nameTxtRect.offsetMax = new Vector2(-55, 0);
            var nameTxt = nameTxtObj.AddComponent<TextMeshProUGUI>();
            nameTxt.fontSize = 18;
            nameTxt.alignment = TextAlignmentOptions.MidlineLeft;
            nameTxt.fontStyle = FontStyles.Bold;
            nameTxt.color = Color.white;
            nameTxt.text = "User Name";

            // Edit Name Button
            GameObject editBtnObj = new GameObject("EditButton");
            editBtnObj.transform.SetParent(nameContainerObj.transform, false);
            var editBtnRect = editBtnObj.AddComponent<RectTransform>();
            editBtnRect.anchorMin = new Vector2(1, 0.5f);
            editBtnRect.anchorMax = new Vector2(1, 0.5f);
            editBtnRect.pivot = new Vector2(1, 0.5f);
            editBtnRect.anchoredPosition = new Vector2(-8, 0);
            editBtnRect.sizeDelta = new Vector2(35, 35);
            var editBtnImg = editBtnObj.AddComponent<Image>();
            if (editIconSprite != null)
            {
                editBtnImg.sprite = editIconSprite;
            }
            else
            {
                editBtnImg.color = new Color(0.8f, 0.65f, 0.2f, 1f);
            }
            var editBtn = editBtnObj.AddComponent<Button>();

            // TMP_InputField for inline editing
            GameObject inputFieldObj = new GameObject("InputField");
            inputFieldObj.transform.SetParent(nameContainerObj.transform, false);
            var inputFieldRect = inputFieldObj.AddComponent<RectTransform>();
            inputFieldRect.anchorMin = Vector2.zero;
            inputFieldRect.anchorMax = Vector2.one;
            inputFieldRect.offsetMin = new Vector2(15, 5);
            inputFieldRect.offsetMax = new Vector2(-55, -5);
            var inputFieldImg = inputFieldObj.AddComponent<Image>();
            inputFieldImg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            GameObject textViewportObj = new GameObject("TextArea");
            textViewportObj.transform.SetParent(inputFieldObj.transform, false);
            var textViewportRect = textViewportObj.AddComponent<RectTransform>();
            textViewportRect.anchorMin = Vector2.zero;
            textViewportRect.anchorMax = Vector2.one;
            textViewportRect.offsetMin = new Vector2(5, 5);
            textViewportRect.offsetMax = new Vector2(-5, -5);
            var rectMask2d = textViewportObj.AddComponent<RectMask2D>();

            GameObject textInputTextObj = new GameObject("Text");
            textInputTextObj.transform.SetParent(textViewportObj.transform, false);
            var textInputTextRect = textInputTextObj.AddComponent<RectTransform>();
            textInputTextRect.anchorMin = Vector2.zero;
            textInputTextRect.anchorMax = Vector2.one;
            textInputTextRect.offsetMin = Vector2.zero;
            textInputTextRect.offsetMax = Vector2.zero;
            var textInputText = textInputTextObj.AddComponent<TextMeshProUGUI>();
            textInputText.fontSize = 18;
            textInputText.alignment = TextAlignmentOptions.MidlineLeft;
            textInputText.color = Color.white;

            var inputField = inputFieldObj.AddComponent<TMP_InputField>();
            inputField.textViewport = textViewportRect;
            inputField.textComponent = textInputText;
            inputFieldObj.SetActive(false); // Hidden by default

            // Stats Area (Bottom-Right)
            GameObject statsContainerObj = new GameObject("Stats");
            statsContainerObj.transform.SetParent(panelObj.transform, false);
            var statsContainerRect = statsContainerObj.AddComponent<RectTransform>();
            statsContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
            statsContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
            statsContainerRect.pivot = new Vector2(0, 0.5f);
            statsContainerRect.anchoredPosition = new Vector2(-40, -40);
            statsContainerRect.sizeDelta = new Vector2(340, 50);

            // Horizontal Layout Group for damage and HP boxes
            var horLayout = statsContainerObj.AddComponent<HorizontalLayoutGroup>();
            horLayout.spacing = 15;
            horLayout.childAlignment = TextAnchor.MiddleLeft;
            horLayout.childControlHeight = true;
            horLayout.childControlWidth = true;
            horLayout.childForceExpandHeight = true;
            horLayout.childForceExpandWidth = true;

            // Damage Stat Box
            GameObject dmgBox = new GameObject("DamageBox");
            dmgBox.transform.SetParent(statsContainerObj.transform, false);
            var dmgBoxImg = dmgBox.AddComponent<Image>();
            dmgBoxImg.color = new Color(0.12f, 0.08f, 0.05f, 0.85f);

            GameObject dmgIconObj = new GameObject("Icon");
            dmgIconObj.transform.SetParent(dmgBox.transform, false);
            var dmgIconRect = dmgIconObj.AddComponent<RectTransform>();
            dmgIconRect.anchorMin = new Vector2(0, 0.5f);
            dmgIconRect.anchorMax = new Vector2(0, 0.5f);
            dmgIconRect.pivot = new Vector2(0, 0.5f);
            dmgIconRect.anchoredPosition = new Vector2(10, 0);
            dmgIconRect.sizeDelta = new Vector2(30, 30);
            var dmgIconImg = dmgIconObj.AddComponent<Image>();
            if (swordIconSprite != null)
            {
                dmgIconImg.sprite = swordIconSprite;
            }
            else
            {
                dmgIconImg.color = new Color(0.9f, 0.3f, 0.3f, 1f);
            }

            GameObject dmgTxtObj = new GameObject("Text");
            dmgTxtObj.transform.SetParent(dmgBox.transform, false);
            var dmgTxtRect = dmgTxtObj.AddComponent<RectTransform>();
            dmgTxtRect.anchorMin = Vector2.zero;
            dmgTxtRect.anchorMax = Vector2.one;
            dmgTxtRect.offsetMin = new Vector2(50, 0);
            dmgTxtRect.offsetMax = Vector2.zero;
            var dmgTxt = dmgTxtObj.AddComponent<TextMeshProUGUI>();
            dmgTxt.fontSize = 18;
            dmgTxt.alignment = TextAlignmentOptions.MidlineLeft;
            dmgTxt.fontStyle = FontStyles.Bold;
            dmgTxt.color = Color.white;
            dmgTxt.text = "500";

            // HP Stat Box
            GameObject hpBox = new GameObject("HPBox");
            hpBox.transform.SetParent(statsContainerObj.transform, false);
            var hpBoxImg = hpBox.AddComponent<Image>();
            hpBoxImg.color = new Color(0.12f, 0.08f, 0.05f, 0.85f);

            GameObject hpIconObj = new GameObject("Icon");
            hpIconObj.transform.SetParent(hpBox.transform, false);
            var hpIconRect = hpIconObj.AddComponent<RectTransform>();
            hpIconRect.anchorMin = new Vector2(0, 0.5f);
            hpIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            hpIconRect.pivot = new Vector2(0, 0.5f);
            hpIconRect.anchoredPosition = new Vector2(10, 0);
            hpIconRect.sizeDelta = new Vector2(30, 30);
            var hpIconImg = hpIconObj.AddComponent<Image>();
            if (heartIconSprite != null)
            {
                hpIconImg.sprite = heartIconSprite;
            }
            else
            {
                hpIconImg.color = new Color(0.9f, 0.1f, 0.3f, 1f);
            }

            GameObject hpTxtObj = new GameObject("Text");
            hpTxtObj.transform.SetParent(hpBox.transform, false);
            var hpTxtRect = hpTxtObj.AddComponent<RectTransform>();
            hpTxtRect.anchorMin = Vector2.zero;
            hpTxtRect.anchorMax = Vector2.one;
            hpTxtRect.offsetMin = new Vector2(50, 0);
            hpTxtRect.offsetMax = Vector2.zero;
            var hpTxt = hpTxtObj.AddComponent<TextMeshProUGUI>();
            hpTxt.fontSize = 18;
            hpTxt.alignment = TextAlignmentOptions.MidlineLeft;
            hpTxt.fontStyle = FontStyles.Bold;
            hpTxt.color = Color.white;
            hpTxt.text = "1000";

            // Close Button
            GameObject closeBtnObj = new GameObject("CloseButton");
            closeBtnObj.transform.SetParent(panelObj.transform, false);
            var closeBtnRect = closeBtnObj.AddComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(1, 1);
            closeBtnRect.anchorMax = new Vector2(1, 1);
            closeBtnRect.pivot = new Vector2(0.5f, 0.5f);
            closeBtnRect.anchoredPosition = new Vector2(10, 10);
            closeBtnRect.sizeDelta = new Vector2(45, 45);
            var closeBtnImg = closeBtnObj.AddComponent<Image>();
            if (closeIconSprite != null)
            {
                closeBtnImg.sprite = closeIconSprite;
            }
            else
            {
                closeBtnImg.color = new Color(0.85f, 0.2f, 0.2f, 1f);
            }
            var closeBtn = closeBtnObj.AddComponent<Button>();

            // -------------------------------------------------------------
            // C. BIND SERIALIZED REFERENCES TO SCRIPT UIMAINMENU
            // -------------------------------------------------------------
            mainMenu.SetFieldValue("profileAvatarButton", avatarBtn);
            mainMenu.SetFieldValue("profileAvatarIcon", avatarHeaderIconImg);
            mainMenu.SetFieldValue("profileNameText", nameHeaderTxt);

            mainMenu.SetFieldValue("profilePopupObject", popupRoot);
            mainMenu.SetFieldValue("profilePopupCloseButton", closeBtn);
            mainMenu.SetFieldValue("profilePopupBlockerButton", blockerBtn);
            mainMenu.SetFieldValue("profilePopupAvatarIcon", avatarIconImg);
            mainMenu.SetFieldValue("profilePopupNameText", nameTxt);
            mainMenu.SetFieldValue("profilePopupNameInputField", inputField);
            mainMenu.SetFieldValue("profilePopupEditNameButton", editBtn);
            mainMenu.SetFieldValue("profilePopupDmgText", dmgTxt);
            mainMenu.SetFieldValue("profilePopupHpText", hpTxt);

            // 4. Save prefab contents and unload
            PrefabUtility.SaveAsPrefabAsset(menuPrefabObj, PREFAB_PATH);
            PrefabUtility.UnloadPrefabContents(menuPrefabObj);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", "Build User Profile UI successfully! Check the UI Main Menu prefab in Scene/Lobby now.", "OK");
        }

        private Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                Transform result = FindChildRecursive(child, name);
                if (result != null) return result;
            }
            return null;
        }

        private Sprite FindSprite(string name)
        {
            string[] guids = AssetDatabase.FindAssets(name + " t:Sprite");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            return null;
        }
    }

    public static class ReflectionExtensions
    {
        public static void SetFieldValue(this object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
        }
    }
}
#endif

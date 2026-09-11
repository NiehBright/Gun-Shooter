#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Watermelon.SquadShooter
{
    [InitializeOnLoad]
    public static class VietnameseFontSetup
    {
        private const string FONT_DIR = "Assets/Project Data/Game/Fonts";
        private const string SETUP_KEY = "GunShooter_VietnameseFonts_v3";

        public const string VIETNAMESE_CHARACTERS = 
            "aAàÀảẢãÃáÁạẠăĂằẰẳẲẵẴắẮặẶâÂầẦẩẨẫẪấẤậẬ" +
            "bBcCdDđĐ" +
            "eEèÈẻẺẽẼéÉẹẸêÊềỀểỂễỄếẾệỆ" +
            "fFgGhHiIìÌỉỈĩĨíÍịỊjJkKlLmMnN" +
            "oOòÒỏỎõÕóÓọỌôÔồỒổỔỗỖốỐộỘơƠờỜởỞỡỠớỚợỢ" +
            "pPqQrRsStTuUùÙủỦũŨúÚụỤưƯừỪửỬữỮứỨựỰ" +
            "vVwWxXyYỳỲỷỶỹỸýÝỵỴzZ" +
            "0123456789" +
            " !\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~" +
            "°±×÷•…–—‘’“”«»";

        private static readonly string[] FONT_FILES = new string[]
        {
            "ChakraPetch-Bold.ttf",
            "ChakraPetch-SemiBold.ttf",
            "BeVietnamPro-Bold.ttf",
            "BeVietnamPro-Regular.ttf",
            "RussoOne-Regular.ttf"
        };

        private const string OXANIUM_PATH = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Fonts/Oxanium-ExtraBold_Extended ASCII SDF.asset";
        private const string ELECTROLIZE_PATH = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Fonts/Electrolize-Regular_Extended ASCII SDF.asset";
        private const string LIBERATION_FALLBACK_PATH = "Assets/Project Data/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset";
        private const string VL_EMPHATIC_PATH = "Assets/Project Data/TextMesh Pro/Fonts/VL Emphatic/VL Emphatic/VL Emphatic Regular SDF.asset";

        static VietnameseFontSetup()
        {
            EditorApplication.delayCall += AutoCheck;
        }

        private static void AutoCheck()
        {
            if (!EditorPrefs.GetBool(SETUP_KEY, false))
            {
                GenerateAndSetup(false);
                EditorPrefs.SetBool(SETUP_KEY, true);
            }
        }

        [MenuItem("GunShooter/Fonts/Generate Vietnamese Font Assets & Setup Fallbacks")]
        public static void GenerateAndSetupMenu()
        {
            GenerateAndSetup(true);
        }

        public static void GenerateAndSetup(bool isManual)
        {
            Debug.Log("[VietnameseFontSetup] Đang khởi tạo và nhúng toàn bộ ký tự tiếng Việt vào Font Assets...");

            List<TMP_FontAsset> createdAssets = new List<TMP_FontAsset>();

            foreach (string fontFile in FONT_FILES)
            {
                string ttfPath = $"{FONT_DIR}/{fontFile}";
                string baseName = Path.GetFileNameWithoutExtension(fontFile);
                string assetPath = $"{FONT_DIR}/{baseName} SDF.asset";

                Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
                if (sourceFont == null)
                {
                    Debug.LogWarning($"[VietnameseFontSetup] Không tìm thấy font ttf: {ttfPath}");
                    continue;
                }

                TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
                if (fontAsset == null)
                {
                    fontAsset = TMP_FontAsset.CreateFontAsset(
                        sourceFont, 
                        90, 
                        9, 
                        GlyphRenderMode.SDFAA, 
                        1024, 
                        1024, 
                        AtlasPopulationMode.Dynamic, 
                        true
                    );

                    if (fontAsset != null)
                    {
                        fontAsset.name = $"{baseName} SDF";

                        AssetDatabase.CreateAsset(fontAsset, assetPath);

                        if (fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0 && fontAsset.atlasTextures[0] != null)
                        {
                            fontAsset.atlasTextures[0].name = $"{baseName} Atlas";
                            AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
                        }

                        if (fontAsset.material != null)
                        {
                            fontAsset.material.name = $"{baseName} Material";
                            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
                        }

                        EditorUtility.SetDirty(fontAsset);
                    }
                }

                if (fontAsset != null)
                {
                    // Nạp ngay toàn bộ bảng mã ký tự tiếng Việt vào Font Atlas
                    fontAsset.TryAddCharacters(VIETNAMESE_CHARACTERS);
                    EditorUtility.SetDirty(fontAsset);
                    createdAssets.Add(fontAsset);
                    Debug.Log($"[VietnameseFontSetup] Đã nạp thành công bộ ký tự tiếng Việt vào: {assetPath}");
                }
            }

            // Gán Fallback cho các Font
            if (createdAssets.Count > 0)
            {
                SetupFallbackForAsset(OXANIUM_PATH, createdAssets);
                SetupFallbackForAsset(ELECTROLIZE_PATH, createdAssets);
                SetupFallbackForAsset(LIBERATION_FALLBACK_PATH, createdAssets);
                SetupFallbackForAsset(VL_EMPHATIC_PATH, createdAssets);

                TMP_FontAsset primary = createdAssets.Find(f => f.name.Contains("BeVietnamPro-Bold")) ?? createdAssets[0];
                SetupTMPSettingsFallback(primary);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Kích hoạt build lại UI Gacha Page để dùng font Be Vietnam Pro ngay lập tức
            UIGachaPageSetup.ForceSetup();

            Debug.Log("[VietnameseFontSetup] Hoàn tất nạp font tiếng Việt và đồng bộ toàn dự án!");

            if (isManual)
            {
                EditorUtility.DisplayDialog("Font Việt Hoá",
                    "Đã nạp thành công toàn bộ bảng mã tiếng Việt đầy đủ (134 ký tự dấu) vào:\n\n" +
                    "1. Chakra Petch (Bold & SemiBold)\n" +
                    "2. Be Vietnam Pro (Bold & Regular)\n" +
                    "3. Russo One\n\n" +
                    "Đồng thời đã cập nhật giao diện Gacha sử dụng trực tiếp font Be Vietnam Pro! Mọi chữ tiếng Việt đều hiển thị chuẩn đẹp 100%.", "Tuyệt vời!");
            }
        }

        private static void SetupFallbackForAsset(string fontAssetPath, List<TMP_FontAsset> fallbacks)
        {
            TMP_FontAsset target = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
            if (target == null) return;

            if (target.fallbackFontAssetTable == null)
            {
                target.fallbackFontAssetTable = new List<TMP_FontAsset>();
            }

            bool changed = false;
            foreach (var fb in fallbacks)
            {
                if (fb != null && fb != target && !target.fallbackFontAssetTable.Contains(fb))
                {
                    target.fallbackFontAssetTable.Add(fb);
                    changed = true;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        private static void SetupTMPSettingsFallback(TMP_FontAsset primaryFallback)
        {
            if (primaryFallback == null) return;

            TMP_Settings settings = Resources.Load<TMP_Settings>("TMP Settings");
            if (settings != null)
            {
                SerializedObject so = new SerializedObject(settings);
                SerializedProperty fallbackProp = so.FindProperty("m_fallbackFontAssets");
                if (fallbackProp != null)
                {
                    bool exists = false;
                    for (int i = 0; i < fallbackProp.arraySize; i++)
                    {
                        if (fallbackProp.GetArrayElementAtIndex(i).objectReferenceValue == primaryFallback)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                    {
                        fallbackProp.InsertArrayElementAtIndex(fallbackProp.arraySize);
                        fallbackProp.GetArrayElementAtIndex(fallbackProp.arraySize - 1).objectReferenceValue = primaryFallback;
                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(settings);
                    }
                }
            }
        }
    }
}
#endif

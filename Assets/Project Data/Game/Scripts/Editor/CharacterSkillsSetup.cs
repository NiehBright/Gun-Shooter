#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    [InitializeOnLoad]
    public static class CharacterSkillsSetup
    {
        private const string SETUP_KEY = "GunShooter_CharacterSkills_v1";

        // Asset Paths
        private const string CHARACTERS_DATABASE_PATH = "Assets/Project Data/Content/Data/Characters System/Characters Database.asset";
        private const string LASER_AOE_PREFAB_PATH = "Assets/Hovl Studio/Magic effects pack/Prefabs/AoE effects/Laser AOE.prefab";
        private const string ICON_THUNDER_PATH = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Icon_PictoIcons(x2)/128/Icon_Thunder.Png";
        private const string ICON_MASK_PATH = "Assets/Layer Lab/GUI Pro-SurvivalClean/ResourcesData/Sprites/Components/Icon_PictoIcons(x2)/128/Icon_Mask.Png";
        private const string SMOKE_PUFF_PATH = "Assets/Hovl Studio/Magic effects pack/Prefabs/Smoke effects/Smoke puff.prefab";
        private const string ANIME_EXPLOSION_PATH = "Assets/ThirdParty/NamuFX/Stylized VFX Doodles/Prefabs/AnimeExplosion.prefab";
        private const string SLASH_A_PREFAB_PATH = "Assets/ThirdParty/NamuFX_Slash/Simple Stylized Slash vol2/Prefabs/Slash_A.prefab";
        private const string PLEXUS_AURA_PATH = "Assets/Hovl Studio/Magic effects pack/Prefabs/Character auras/Plexus.prefab";
        private const string NINNIN_GRAPHIC_PATH = "Assets/Project Data/Game/Prefabs/Characters/Player/Graphics/NinNin.prefab";
        private const string SHADOW_CLONE_PREFAB_PATH = "Assets/Project Data/Game/Prefabs/Characters/Player/Skills/ShadowClone.prefab";

        static CharacterSkillsSetup()
        {
            EditorApplication.delayCall += AutoRunCheck;
        }

        private static void AutoRunCheck()
        {
            if (!EditorPrefs.GetBool(SETUP_KEY, false))
            {
                RunSetup(false);
                EditorPrefs.SetBool(SETUP_KEY, true);
            }
        }

        [MenuItem("GunShooter/Setup Character Skills")]
        public static void ForceSetup()
        {
            RunSetup(true);
        }

        public static void RunSetup(bool isManual)
        {
            Debug.Log("[CharacterSkillsSetup] Bắt đầu thiết lập kỹ năng cho Nieh (Laser Pháo Kích) và NinNin (Thuật Phân Thân)...");

            // 1. Tạo Prefab ShadowClone cho NinNin
            GameObject shadowClonePrefab = BuildShadowClonePrefab();

            // 2. Cấu hình Characters Database
            ConfigureCharactersDatabase(shadowClonePrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[CharacterSkillsSetup] Hoàn tất thiết lập kỹ năng thành công!");

            if (isManual)
            {
                EditorUtility.DisplayDialog("Character Skills Setup",
                    "Đã thiết lập thành công kỹ năng cho Nieh và NinNin:\n\n" +
                    "1. Nieh: Laser Pháo Kích (Orbital Laser)\n" +
                    "   - VFX: Laser AOE (Hovl Studio)\n" +
                    "   - Icon: Thunder (GUI Pro)\n" +
                    "   - Cơ chế: Nổ AOE ban đầu + thiêu đốt bức xạ + làm chậm 50% tốc độ di chuyển quái\n\n" +
                    "2. NinNin: Thuật Phân Thân Hư Ảo (Shadow Clone)\n" +
                    "   - VFX: ShadowClone.prefab (Khói ninja + Aura Cyber + Nổ Anime)\n" +
                    "   - Icon: Ninja Mask (GUI Pro)\n" +
                    "   - Cơ chế: NinNin lướt né đòn + Tàng hình 1.2s; Phân thân khiêu khích quái vật bu vào rồi phát nổ AOE cực mạnh làm choáng 1.5s!",
                    "OK");
            }
        }

        private static GameObject BuildShadowClonePrefab()
        {
            string dir = Path.GetDirectoryName(SHADOW_CLONE_PREFAB_PATH);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            GameObject root = new GameObject("ShadowClone");
            root.layer = LayerMask.NameToLayer("Default");

            // Thêm ShadowCloneBehaviour
            ShadowCloneBehaviour cloneBehaviour = root.AddComponent<ShadowCloneBehaviour>();

            // Gán Explosion VFX
            GameObject explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ANIME_EXPLOSION_PATH);
            cloneBehaviour.ExplosionVfxPrefab = explosionPrefab;
            cloneBehaviour.TauntRadius = 12f;
            cloneBehaviour.ExplosionRadius = 3.2f;
            cloneBehaviour.ExplosionDamage = 250f;
            cloneBehaviour.Duration = 3.5f;
            cloneBehaviour.StunDuration = 1.5f;

            // Thêm mô hình NinNin làm phân thân
            GameObject ninninGraphic = AssetDatabase.LoadAssetAtPath<GameObject>(NINNIN_GRAPHIC_PATH);
            if (ninninGraphic != null)
            {
                GameObject graphicInstance = Object.Instantiate(ninninGraphic, root.transform);
                graphicInstance.name = "Graphic";
                graphicInstance.transform.localPosition = Vector3.zero;
                graphicInstance.transform.localRotation = Quaternion.identity;

                // Tắt các component không cần thiết trên phân thân
                var characterGraphics = graphicInstance.GetComponent<BaseCharacterGraphics>();
                if (characterGraphics != null) Object.DestroyImmediate(characterGraphics);

                var animator = graphicInstance.GetComponent<Animator>();
                if (animator != null) animator.enabled = true; // Giữ pose idle
            }

            // Thêm hiệu ứng Aura Cyber (Plexus)
            GameObject plexusPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PLEXUS_AURA_PATH);
            if (plexusPrefab != null)
            {
                GameObject auraInstance = Object.Instantiate(plexusPrefab, root.transform);
                auraInstance.name = "CyberAura";
                auraInstance.transform.localPosition = new Vector3(0, 0.5f, 0);
            }

            // Thêm hiệu ứng Khói khi xuất hiện (Smoke puff)
            GameObject smokePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SMOKE_PUFF_PATH);
            if (smokePrefab != null)
            {
                GameObject smokeInstance = Object.Instantiate(smokePrefab, root.transform);
                smokeInstance.name = "SmokePuff";
                smokeInstance.transform.localPosition = Vector3.zero;
            }

            // Lưu Prefab
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, SHADOW_CLONE_PREFAB_PATH);
            Object.DestroyImmediate(root);

            Debug.Log($"[CharacterSkillsSetup] Đã tạo thành công ShadowClone.prefab tại: {SHADOW_CLONE_PREFAB_PATH}");
            return savedPrefab;
        }

        private static void ConfigureCharactersDatabase(GameObject shadowClonePrefab)
        {
            CharactersDatabase database = AssetDatabase.LoadAssetAtPath<CharactersDatabase>(CHARACTERS_DATABASE_PATH);
            if (database == null)
            {
                Debug.LogError($"[CharacterSkillsSetup] Không tìm thấy Characters Database tại: {CHARACTERS_DATABASE_PATH}");
                return;
            }

            Undo.RecordObject(database, "Setup Character Skills");

            Sprite thunderIcon = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_THUNDER_PATH);
            Sprite maskIcon = AssetDatabase.LoadAssetAtPath<Sprite>(ICON_MASK_PATH);
            GameObject laserPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LASER_AOE_PREFAB_PATH);

            var characters = database.Characters;
            for (int i = 0; i < characters.Length; i++)
            {
                var character = characters[i];
                if (character == null) continue;

                // 1. Thiết lập Nieh (Type = 1 hoặc Name = "Nieh")
                if (character.Type == CharacterType.Character_02 || character.Name == "Nieh")
                {
                    var skill = character.SkillData;
                    if (skill != null)
                    {
                        skill.SkillName = "Laser Pháo Kích";
                        skill.SkillType = SkillType.OrbitalLaser;
                        skill.ButtonIcon = thunderIcon;
                        skill.VFXPrefab = laserPrefab;
                        skill.Cooldown = 16f;
                        skill.Duration = 4.0f;
                        skill.AoeRadius = 1.5f;
                        skill.PullSpeed = 0f;
                        skill.DamageMultiplier = 2.0f;
                        skill.TickInterval = 0.5f;

                        Debug.Log("[CharacterSkillsSetup] Đã cấu hình kỹ năng Laser Pháo Kích cho Nieh.");
                    }
                }
                // 2. Thiết lập NinNin (Type = 2 hoặc Name = "NinNin")
                else if (character.Type == CharacterType.Character_03 || character.Name == "NinNin")
                {
                    var skill = character.SkillData;
                    if (skill != null)
                    {
                        skill.SkillName = "Thuật Phân Thân";
                        skill.SkillType = SkillType.ShadowClone;
                        skill.ButtonIcon = maskIcon;
                        skill.VFXPrefab = shadowClonePrefab;
                        skill.Cooldown = 14f;
                        skill.Duration = 3.5f;
                        skill.AoeRadius = 3.2f;
                        skill.PullSpeed = 0f;
                        skill.DamageMultiplier = 2.5f;
                        skill.TickInterval = 0.5f;

                        Debug.Log("[CharacterSkillsSetup] Đã cấu hình kỹ năng Thuật Phân Thân cho NinNin.");
                    }
                }
            }

            EditorUtility.SetDirty(database);
        }
    }
}
#endif

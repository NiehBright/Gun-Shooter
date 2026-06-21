using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using Watermelon.LevelSystem;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public static class ActionsMenu
    {
#if UNITY_EDITOR
        [MenuItem("Actions/Next XP Lvl", priority = 36)]
        private static void GetNextLevel()
        {
            if (Application.isPlaying)
            {
                ExperienceController.SetLevelDev(ExperienceController.CurrentLevel + 1);
            }
        }

        [MenuItem("Actions/Set No XP", priority = 37)]
        private static void NoXP()
        {
            if (Application.isPlaying)
            {
                ExperienceController.SetLevelDev(1);
            }
        }

        [MenuItem("Actions/All Weapons", priority = 51)]
        private static void UnlockAllWeapons()
        {
            if (Application.isPlaying)
            {
                WeaponsController.UnlockAllWeaponsDev();
                UIController.GetPage<UIWeaponPage>().UpdateUI();
            }
        }

        [MenuItem("Actions/Prev Level (menu) [P]", priority = 71)]
        public static void PrevLevel()
        {
            LevelController.PrevLevelDev();
        }

        [MenuItem("Actions/Next Level (menu) [N]", priority = 72)]
        public static void NextLevel()
        {
            LevelController.NextLevelDev();
        }

        [MenuItem("Actions/Print Shorcuts", priority = 150)]
        private static void PrintShortcuts()
        {
            Debug.Log("H - heal player \nD - toggle player damage \nN - skip level\nR - skip room\n\n");
        }

#endif
    }
}
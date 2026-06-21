using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    [CreateAssetMenu(fileName = "Game Settings", menuName = "Content/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [SerializeField] LevelsDatabase levelsDatabase;
        public LevelsDatabase LevelsDatabase => levelsDatabase;

        [LineSpacer("Player")]
        [SerializeField] GameObject playerPrefab;
        public GameObject PlayerPrefab => playerPrefab;

        [SerializeField] float invulnerabilityAfrerReviveDuration;
        public float InvulnerabilityAfrerReviveDuration => invulnerabilityAfrerReviveDuration;

        [SerializeField] bool useAttackButton;
        public bool UseAttackButton => useAttackButton;

        [LineSpacer("Environment")]
        [SerializeField] NavMeshData navMeshData;
        public NavMeshData NavMeshData => navMeshData;

        [SerializeField] GameObject backWallCollider;
        public GameObject BackWallCollider => backWallCollider;

        [Space(5f)]
        [SerializeField] ChestData[] chestData;
        public ChestData[] ChestData => chestData;

        [LineSpacer("Drop")]
        [SerializeField] DropableItemSettings dropSettings;

        [LineSpacer("Minimap")]
        [SerializeField] LevelTypeSettings[] levelTypes;

        [SerializeField] Sprite defaultWorldSprite;
        public Sprite DefaultWorldSprite => defaultWorldSprite;

        [LineSpacer("Rewarded Video")]
        [SerializeField] CurrencyPrice revivePrice;
        public CurrencyPrice RevivePrice => revivePrice;

        private Dictionary<LevelType, int> levelTypesLink;

        public void Initialise()
        {
            levelTypesLink = new Dictionary<LevelType, int>();
            for (int i = 0; i < levelTypes.Length; i++)
            {
                if (!levelTypesLink.ContainsKey(levelTypes[i].LevelType))
                {
                    levelTypes[i].Initialise();

                    levelTypesLink.Add(levelTypes[i].LevelType, i);
                }
                else
                {
                    Debug.LogError(string.Format("[Levels]: Duplicate is found - {0}", levelTypes[i].LevelType));
                }
            }
            
            Drop.Initialise(dropSettings);

            for(int i = 0; i < chestData.Length; i++)
            {
                chestData[i].Initialise();
            }
        }

        public LevelTypeSettings GetLevelSettings(LevelType levelType)
        {
            if (levelTypesLink.ContainsKey(levelType))
                return levelTypes[levelTypesLink[levelType]];

            Debug.LogError(string.Format("[Levels]: Level with type '{0}' is missing", levelType));

            return null;
        }

        public ChestData GetChestData(LevelChestType chestType)
        {
            for(int i = 0; i < chestData.Length; i++)
            {
                var data = chestData[i];
                if(chestType == data.Type)
                {
                    return data;
                }
            }

            Debug.LogError(string.Format("[Level]: Chest preset with type {0} is missing!", chestType));

            return null;
        }
    }
}
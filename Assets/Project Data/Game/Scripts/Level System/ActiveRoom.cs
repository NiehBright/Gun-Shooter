using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    public static class ActiveRoom
    {
        private static GameObject levelObject;

        private static RoomData roomData;
        public static RoomData RoomData => roomData;

        private static LevelData levelData;
        public static LevelData LevelData => levelData;

        private static List<GameObject> activeObjects;

        private static List<BaseEnemyBehavior> enemies;
        public static List<BaseEnemyBehavior> Enemies => enemies;

        private static List<AbstractChestBehavior> chests;
        public static List<AbstractChestBehavior> Chests => chests;

        private static int currentLevelIndex;
        public static int CurrentLevelIndex => currentLevelIndex;

        private static int currentWorldIndex;
        public static int CurrentWorldIndex => currentWorldIndex;

        private static List<ExitPointBehaviour> exitPoints;
        public static List<ExitPointBehaviour> ExitPoints => exitPoints;

        private static List<GameObject> customObjects;
        public static List<GameObject> CustomObjects => customObjects;

        public static void Initialise(GameObject levelObject)
        {
            ActiveRoom.levelObject = levelObject;

            activeObjects = new List<GameObject>();
            enemies = new List<BaseEnemyBehavior>();
            chests = new List<AbstractChestBehavior>();
            customObjects = new List<GameObject>();
            exitPoints = new List<ExitPointBehaviour>();
        }

        public static void SetLevelData(int currentWorldIndex, int currentLevelIndex)
        {
            ActiveRoom.currentWorldIndex = currentWorldIndex;
            ActiveRoom.currentLevelIndex = currentLevelIndex;
        }

        public static void SetLevelData(LevelData levelData)
        {
            ActiveRoom.levelData = levelData;
        }

        public static void SetRoomData(RoomData roomData)
        {
            ActiveRoom.roomData = roomData;
        }

        public static void Unload()
        {
            // Unload created obstacles
            for (int i = 0; i < activeObjects.Count; i++)
            {
                activeObjects[i].transform.SetParent(null);
                activeObjects[i].SetActive(false);
            }

            activeObjects.Clear();

            // Unload enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Unload();

                Object.Destroy(enemies[i].gameObject);
            }

            enemies.Clear();

            if(!exitPoints.IsNullOrEmpty())
            {
                foreach(ExitPointBehaviour exitPoint in exitPoints)
                {
                    exitPoint.Unload();
                }

                exitPoints.Clear();
            }

            // Unload custom objects
            UnloadCustomObjects();
        }

        #region Environment/Obstacles
        public static void SpawnItem(LevelItem item, ItemEntityData itemEntityData)
        {
            GameObject itemObject = item.Pool.GetPooledObject(false);
            itemObject.transform.SetParent(levelObject.transform);
            itemObject.transform.SetPositionAndRotation(itemEntityData.Position, itemEntityData.Rotation);
            itemObject.transform.localScale = itemEntityData.Scale;
            itemObject.SetActive(true);

            activeObjects.Add(itemObject);
        }

        public static void RegisterExitPoint(ExitPointBehaviour exitPointBehaviour)
        {
            exitPoints.Add(exitPointBehaviour);

            exitPointBehaviour.Initialise();
        }

        public static void SpawnChest(ChestEntityData chestEntityData, ChestData chestData)
        {
            GameObject chestObject = chestData.Pool.GetPooledObject(false);
            chestObject.transform.SetParent(levelObject.transform);
            chestObject.transform.SetPositionAndRotation(chestEntityData.Position, chestEntityData.Rotation);
            chestObject.transform.localScale = chestEntityData.Scale;
            chestObject.SetActive(true);

            chests.Add(chestObject.GetComponent<AbstractChestBehavior>());

            activeObjects.Add(chestObject);
        }

        #endregion

        #region Enemies
        public static BaseEnemyBehavior SpawnEnemy(EnemyData enemyData, EnemyEntityData enemyEntityData, bool isActive)
        {
            BaseEnemyBehavior enemy = Object.Instantiate(enemyData.Prefab, enemyEntityData.Position, enemyEntityData.Rotation, levelObject.transform).GetComponent<BaseEnemyBehavior>();
            enemy.transform.localScale = enemyEntityData.Scale;
            enemy.SetEnemyData(enemyData, enemyEntityData.IsElite);
            enemy.SetPatrollingPoints(enemyEntityData.PathPoints);

            // Place enemy on the middle of the path if there are two or more waypoints
            if (enemyEntityData.PathPoints.Length > 1)
                enemy.transform.position = enemyEntityData.PathPoints[0] + (enemyEntityData.PathPoints[1] - enemyEntityData.PathPoints[0]) * 0.5f;

            if (isActive)
                enemy.Initialise();

            enemies.Add(enemy);

            return enemy;
        }

        public static void ActivateEnemies()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Initialise();
            }
        }

        public static void ClearEnemies()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Unload();

                Object.Destroy(enemies[i].gameObject);
            }

            enemies.Clear();
        }

        public static BaseEnemyBehavior GetEnemyForSpecialReward()
        {
            BaseEnemyBehavior result = enemies.Find(e => e.Tier == EnemyTier.Boss);

            if (result != null)
                return result;

            result = enemies.Find(e => e.Tier == EnemyTier.Elite);

            if (result != null)
                return result;

            result = enemies[0];

            for (int i = 1; i < enemies.Count; i++)
            {
                if (enemies[i].transform.position.z > result.transform.position.z)
                {
                    result = enemies[i];
                }
            }

            return result;
        }

        public static void InitialiseDrop(List<DropData> enemyDrop, List<DropData> chestDrop)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].ResetDrop();
            }

            for (int i = 0; i < enemyDrop.Count; i++)
            {
                if (enemyDrop[i].dropType == DropableItemType.Currency && enemyDrop[i].currencyType == CurrencyType.Coins)
                {
                    List<int> coins = LevelController.SplitIntEqually(enemyDrop[i].amount, enemies.Count);

                    for (int j = 0; j < enemies.Count; j++)
                    {
                        enemies[j].AddDrop(new DropData() { dropType = DropableItemType.Currency, currencyType = CurrencyType.Coins, amount = coins[j] });
                    }
                }
                else
                {
                    GetEnemyForSpecialReward().AddDrop(enemyDrop[i]);
                }
            }

            for (int i = 0; i < chests.Count; i++)
            {
                chests[i].Init(chestDrop);
            }
        }

        public static List<BaseEnemyBehavior> GetAliveEnemies()
        {
            List<BaseEnemyBehavior> result = new List<BaseEnemyBehavior>();

            for (int i = 0; i < enemies.Count; i++)
            {
                if (!enemies[i].IsDead)
                {
                    result.Add(enemies[i]);
                }
            }

            return result;
        }

        public static bool AreAllEnemiesDead()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (!enemies[i].IsDead)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Custom Objects

        public static void SpawnCustomObject(CustomObjectData objectData)
        {
            GameObject customObject = Tween.Instantiate(objectData.PrefabRef);
            customObject.transform.SetParent(levelObject.transform);
            customObject.transform.SetPositionAndRotation(objectData.Position, objectData.Rotation);
            customObject.transform.localScale = objectData.Scale;
            customObject.SetActive(true);

            customObjects.Add(customObject);
        }

        public static void UnloadCustomObjects()
        {
            if (customObjects.IsNullOrEmpty())
                return;

            for (int i = 0; i < customObjects.Count; i++)
            {
                Tween.Destroy(customObjects[i]);
            }

            customObjects.Clear();
        }

        #endregion
    }
}
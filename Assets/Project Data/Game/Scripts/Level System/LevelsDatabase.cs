using System.Collections.Generic;
using UnityEngine;


namespace Watermelon.LevelSystem
{
    [CreateAssetMenu(fileName = "Levels Database", menuName = "Content/New Level/Levels Database")]
    public class LevelsDatabase : ScriptableObject
    {
        [SerializeField] WorldData[] worlds;
        public WorldData[] Worlds => worlds;

        public void Initialise()
        {
            for (int i = 0; i < worlds.Length; i++)
            {
                worlds[i].Initialise();
            }
        }

        public WorldData GetWorld(int worldIndex)
        {
            if (worlds.IsInRange(worldIndex))
            {
                return worlds[worldIndex];
            }

            return worlds[worldIndex % worlds.Length];
        }

        public LevelData GetRandomLevel()
        {
            LevelData tempLevel = null;

            do
            {
                WorldData randomWorld = worlds.GetRandomItem();
                if (randomWorld != null)
                {
                    LevelData randomLevel = randomWorld.Levels.GetRandomItem();
                    if (randomLevel != null)
                        tempLevel = randomLevel;
                }
            }
            while (tempLevel == null);

            return tempLevel;
        }

        public LevelData GetLevel(int worldIndex, int levelIndex)
        {
            WorldData world = GetWorld(worldIndex);
            if(world != null)
            {
                if (world.Levels.IsInRange(levelIndex))
                {
                    return world.Levels[levelIndex];
                }
            }

            return GetRandomLevel();
        }

        public bool DoesNextLevelExist(int worldIndex, int levelIndex)
        {
            WorldData world = GetWorld(worldIndex);
            if (world != null)
            {
                if (world.Levels.IsInRange(levelIndex + 1))
                {
                    return true;
                }
            }

            return false;
        }

        public int GetWorldsAmount()
        {
            return worlds.Length;
        }
    }
}

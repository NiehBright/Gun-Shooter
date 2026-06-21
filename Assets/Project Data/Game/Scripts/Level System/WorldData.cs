using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.LevelSystem
{
    [CreateAssetMenu(fileName = "World", menuName = "Content/New Level/World")]
    public class WorldData : ScriptableObject
    {
        [SerializeField] Sprite previewSprite;
        public Sprite PreviewSprite => previewSprite;

        [SerializeField] AudioClip uniqueWorldMusicClip;
        public AudioClip UniqueWorldMusicClip => uniqueWorldMusicClip;

        [SerializeField] WorldType worldType;
        public WorldType WorldType => worldType;

        [SerializeField] LevelData[] levels;
        public LevelData[] Levels => levels;

        [SerializeField] LevelItem[] items;
        public LevelItem[] Items => items;

        [SerializeField] RoomEnvironmentPreset[] roomEnvPresets;
        public RoomEnvironmentPreset[] RoomEnvPresets => roomEnvPresets;

        [SerializeField] CustomObjectData[] worldCustomObjects;
        public CustomObjectData[] WorldCustomObjects => worldCustomObjects;

        private Dictionary<int, LevelItem> itemsDisctionary;

        public void Initialise()
        {
            for (int i = 0; i < levels.Length; i++)
            {
                levels[i].Initialise(this);
            }

            itemsDisctionary = new Dictionary<int, LevelItem>();

            for (int i = 0; i < items.Length; i++)
            {
                itemsDisctionary.Add(items[i].Hash, items[i]);
            }

        }

        public void LoadWorld()
        {
            // creating items pools
            for (int i = 0; i < items.Length; i++)
            {
                items[i].OnWorldLoaded();
            }
        }

        public void UnloadWorld()
        {
            // releasing items pools
            for (int i = 0; i < items.Length; i++)
            {
                items[i].OnWorldUnloaded();
            }
        }

        public LevelItem GetLevelItem(int hash)
        {
            return itemsDisctionary[hash];
        }
    }
}

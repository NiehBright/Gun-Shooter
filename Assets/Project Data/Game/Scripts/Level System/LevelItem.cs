using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.LevelSystem
{
    [System.Serializable]
    public class LevelItem
    {
        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        [HideInInspector][SerializeField] int hash;
        public int Hash => hash;

        [SerializeField] LevelItemType type;
        public LevelItemType Type => type;

        private Pool pool;
        public Pool Pool => pool;

        public void OnWorldLoaded()
        {
            if (prefab == null)
                return;

            if (pool != null)
            {
                pool.Clear();
                pool = null;
            }

            pool = new Pool(new PoolSettings(prefab.name, prefab, 0, true));
            pool.Initialize();
        }

        public void OnWorldUnloaded()
        {
            if (pool != null)
            {
                pool.Clear();
                pool = null;
            }
        }
    }
}

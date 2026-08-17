using UnityEngine;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class EnemyData
    {
        [SerializeField] EnemyType enemyType;
        public EnemyType EnemyType => enemyType;

        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        [System.NonSerialized] private Pool pool;
        public Pool Pool => pool;

        public void InitPool()
        {
            if (pool == null)
            {
                if (prefab != null)
                {
                    string poolName = prefab.name + "_" + enemyType.ToString();
                    if (PoolManager.PoolExists(poolName))
                    {
                        pool = PoolManager.GetPoolByName(poolName);
                    }
                    else
                    {
                        pool = PoolManager.AddPool(new PoolSettings(poolName, prefab, 3, true));
                    }
                }
                else
                {
                    Debug.LogWarning($"[EnemyData] Prefab is missing for enemy type: {enemyType}. Pooling skipped.");
                }
            }
        }

        [SerializeField] EnemyStats stats;
        public EnemyStats Stats => stats;

        [Header("Editor")]
        [SerializeField] Texture2D icon;
        public Texture2D Icon => icon;

        [SerializeField] Color iconTint;
        public Color IconTint => iconTint;
    }
}
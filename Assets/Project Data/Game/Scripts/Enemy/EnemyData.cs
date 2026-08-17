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

        private Pool pool;
        public Pool Pool => pool;

        public void InitPool()
        {
            if (pool == null)
            {
                pool = new Pool(new PoolSettings(prefab.name, prefab, 3, true));
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
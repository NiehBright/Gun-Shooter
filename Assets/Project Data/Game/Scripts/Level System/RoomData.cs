using UnityEngine;

namespace Watermelon.LevelSystem
{
    [System.Serializable]
    public class RoomData
    {
        [SerializeField] Vector3 spawnPoint;
        public Vector3 SpawnPoint => spawnPoint;

        [Space]
        [SerializeField] EnemyEntityData[] enemyEntities;
        public EnemyEntityData[] EnemyEntities => enemyEntities;

        [SerializeField] ItemEntityData[] itemEntities;
        public ItemEntityData[] ItemEntities => itemEntities;

        [SerializeField] ChestEntityData[] chestEntities;
        public ChestEntityData[] ChestEntities => chestEntities;

        [SerializeField] CustomObjectData[] roomCustomObjects;
        public CustomObjectData[] RoomCustomObjects => roomCustomObjects;

    }
}
using UnityEngine;

namespace Watermelon.LevelSystem
{
    [System.Serializable]
    public class ChestEntityData
    {
        public bool IsInited = false;
        public LevelChestType ChestType;

        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public ChestEntityData(LevelChestType chestType, Vector3 position, Quaternion rotation, Vector3 scale, bool isInited)
        {
            IsInited = isInited;
            ChestType = chestType;
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }
}
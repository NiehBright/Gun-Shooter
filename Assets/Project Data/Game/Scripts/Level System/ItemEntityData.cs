using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.LevelSystem
{
    [System.Serializable]
    public class ItemEntityData
    {
        public int Hash;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public ItemEntityData(int hash, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Hash = hash;
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }
}

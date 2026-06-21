using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.LevelSystem
{
    [System.Serializable]
    public class CustomObjectData 
    {
        public GameObject PrefabRef;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public CustomObjectData(GameObject prefabRef, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            PrefabRef = prefabRef;
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }
}

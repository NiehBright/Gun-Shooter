using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    [System.Serializable]
    public class EnemyEntityData
    {
        public EnemyType EnemyType;

        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale = Vector3.one;

        public bool IsElite;

        public Vector3[] PathPoints;

        public EnemyEntityData(EnemyType enemyType, Vector3 position, Quaternion rotation, Vector3 scale, bool isElite, Vector3[] pathPoints)
        {
            EnemyType = enemyType;
            Position = position;
            Rotation = rotation;
            Scale = scale;
            IsElite = isElite;
            PathPoints = pathPoints;
        }
    }
}
using UnityEngine;

namespace Watermelon.LevelSystem
{
    [System.Serializable]
    public class RoomEnvironmentPreset 
    {
        [SerializeField] string name;
        [SerializeField] ItemEntityData[] itemEntities;
        [SerializeField] Vector3 spawnPos;
        [SerializeField] Vector3 exitPointPos;
    }
}
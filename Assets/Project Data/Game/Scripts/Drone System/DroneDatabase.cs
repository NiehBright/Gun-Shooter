using UnityEngine;

namespace Watermelon.SquadShooter
{
    [CreateAssetMenu(fileName = "Drone Database", menuName = "Content/Drone Database")]
    public class DroneDatabase : ScriptableObject
    {
        [SerializeField] DroneData[] drones;
        public DroneData[] Drones => drones;

        public DroneData GetDrone(DroneType type)
        {
            for (int i = 0; i < drones.Length; i++)
            {
                if (drones[i].Type.Equals(type))
                    return drones[i];
            }

            Debug.LogError("Drone data of type: " + type + " is not found");
            return drones[0];
        }

        public DroneData GetDroneByIndex(int index)
        {
            return drones[index % drones.Length];
        }
    }
}

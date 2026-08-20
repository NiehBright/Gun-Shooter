using UnityEngine;

namespace Watermelon.Upgrades
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "Drone Upgrade", menuName = "Content/Upgrades/Drone Upgrade")]
    public class BaseDroneUpgrade : Upgrade<BaseDroneUpgradeStage>
    {
        public override void Initialise()
        {

        }
    }

    [System.Serializable]
    public class BaseDroneUpgradeStage : BaseUpgradeStage
    {
        [Header("Prefabs")]
        [SerializeField] GameObject dronePrefab;
        public GameObject DronePrefab => dronePrefab;

        [SerializeField] GameObject bulletPrefab;
        public GameObject BulletPrefab => bulletPrefab;

        [Header("Data")]
        [SerializeField] DuoInt damage;
        public DuoInt Damage => damage;

        [SerializeField] float rangeRadius = 5f;
        public float RangeRadius => rangeRadius;

        [SerializeField, Tooltip("Shots Per Second")] float fireRate = 2f;
        public float FireRate => fireRate;

        [SerializeField] float movementSpeed = 5f;
        public float MovementSpeed => movementSpeed;

        [SerializeField] DuoFloat bulletSpeed;
        public DuoFloat BulletSpeed => bulletSpeed;
        
        [SerializeField] int power = 10;
        public int Power => power;
    }
}

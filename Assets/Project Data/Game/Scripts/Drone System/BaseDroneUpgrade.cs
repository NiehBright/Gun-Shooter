using UnityEngine;

using UnityEngine.AddressableAssets;

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
        [SerializeField] AssetReferenceGameObject dronePrefab;
        public AssetReferenceGameObject DronePrefab => dronePrefab;

        [SerializeField] AssetReferenceGameObject bulletPrefab;
        public AssetReferenceGameObject BulletPrefab => bulletPrefab;

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

        [Header("Upgrade")]
        [SerializeField, Tooltip("Số Cards cần để nâng lên level này")] int cardsRequired = 5;
        public int CardsRequired => cardsRequired;
        
        [SerializeField] int power = 10;
        public int Power => power;
    }
}

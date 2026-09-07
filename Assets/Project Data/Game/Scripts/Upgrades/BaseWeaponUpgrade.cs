using UnityEngine;

namespace Watermelon.Upgrades
{
    [System.Serializable]
    public class BaseWeaponUpgrade : Upgrade<BaseWeaponUpgradeStage>
    {
        public override void Initialise()
        {

        }
    }

    [System.Serializable]
    public class BaseWeaponUpgradeStage : BaseUpgradeStage
    {
        [Header("Prefabs")]
        [SerializeField] UnityEngine.AddressableAssets.AssetReferenceGameObject weaponPrefabRef;
        public UnityEngine.AddressableAssets.AssetReferenceGameObject WeaponPrefabRef => weaponPrefabRef;

        private GameObject loadedWeaponPrefab;
        public GameObject WeaponPrefab
        {
            get
            {
                if (loadedWeaponPrefab == null && weaponPrefabRef != null && weaponPrefabRef.RuntimeKeyIsValid())
                {
                    loadedWeaponPrefab = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(weaponPrefabRef).WaitForCompletion();
                }
                return loadedWeaponPrefab;
            }
        }

        [SerializeField] UnityEngine.AddressableAssets.AssetReferenceGameObject bulletPrefabRef;
        public UnityEngine.AddressableAssets.AssetReferenceGameObject BulletPrefabRef => bulletPrefabRef;

        private GameObject loadedBulletPrefab;
        public GameObject BulletPrefab
        {
            get
            {
                if (loadedBulletPrefab == null && bulletPrefabRef != null && bulletPrefabRef.RuntimeKeyIsValid())
                {
                    loadedBulletPrefab = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(bulletPrefabRef).WaitForCompletion();
                }
                return loadedBulletPrefab;
            }
        }

        [Header("Data")]
        [SerializeField] DuoInt damage;
        public DuoInt Damage => damage;

        [SerializeField] float rangeRadius;
        public float RangeRadius => rangeRadius;

        [SerializeField, Tooltip("Shots Per Second")] float fireRate;
        public float FireRate => fireRate;

        [SerializeField] float spread;
        public float Spread => spread;

        [SerializeField] int power;
        public int Power => power;

        [SerializeField] DuoInt bulletsPerShot = new DuoInt(1,1);
        public DuoInt BulletsPerShot => bulletsPerShot;

        [SerializeField] DuoFloat bulletSpeed;
        public DuoFloat BulletSpeed => bulletSpeed;

        // key upgrade - "ideal" way to play the game, based on this upgrades sequence is built economy
        [SerializeField] int keyUpgradeNumber = -1;
        public int KeyUpgradeNumber => keyUpgradeNumber;
    }
}

using System.Collections.Generic;
using UnityEngine;
using Watermelon;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class DronesController : MonoBehaviour
    {
        [SerializeField] DroneDatabase database;
        public static DroneDatabase Database => instance.database;

        [Header("Drop")]
        [SerializeField] GameObject cardPrefab;

        private static DronesController instance;

        private static GlobalDronesSave save;

        private static DroneData[] drones;
        private static Dictionary<DroneType, int> dronesLink;

        public static int SelectedDroneIndex
        {
            get { return save.selectedDroneIndex; }
            private set { save.selectedDroneIndex = value; }
        }

        public delegate void DroneDelagate(DroneData drone);

        public static event SimpleCallback OnNewDroneSelected;
        public static event SimpleCallback OnDroneUpgraded;
        public static event SimpleCallback OnDroneCardsAmountChanged;
        public static event DroneDelagate OnDroneUnlocked;

        public void Initialise()
        {
            instance = this;

            save = SaveController.GetSaveObject<GlobalDronesSave>("drone_save");

            if (cardPrefab != null)
            {
                // Register drop item if needed
                // Drop.RegisterDropItem(new CustomDropItem(DropableItemType.DroneCard, cardPrefab));
            }

            dronesLink = new Dictionary<DroneType, int>();
            drones = database.Drones;

            for (int i = 0; i < drones.Length; i++)
            {
                drones[i].Initialise();
                dronesLink.Add(drones[i].Type, i);
            }

            CheckDroneUpdateState();
        }

        public void CheckDroneUpdateState()
        {
            if (drones == null) return;
            
            for (int i = 0; i < drones.Length; i++)
            {
                if (drones[i].UpgradeType == UpgradeType.None) continue;
                
                BaseUpgrade upgrade = UpgradesController.GetUpgrade<BaseUpgrade>(drones[i].UpgradeType);
                if (upgrade != null)
                {
                    if (upgrade.UpgradeLevel == 0 && drones[i].CardsAmount >= upgrade.NextStage.Price)
                    {
                        upgrade.UpgradeStage();

                        OnDroneUnlocked?.Invoke(drones[i]);
                    }
                }
            }
        }

        public static void SelectDrone(DroneType droneType)
        {
            int droneIndex = 0;
            for (int i = 0; i < instance.database.Drones.Length; i++)
            {
                if (instance.database.Drones[i].Type == droneType)
                {
                    droneIndex = i;
                    break;
                }
            }

            if (SelectedDroneIndex == droneIndex) return;

            SelectedDroneIndex = droneIndex;

            if (CharacterBehaviour.GetBehaviour() != null)
            {
                CharacterBehaviour.GetBehaviour().UpdateDrone();
            }

            OnNewDroneSelected?.Invoke();
        }

        public static void UnequipDrone()
        {
            if (SelectedDroneIndex == -1) return;

            SelectedDroneIndex = -1;

            if (CharacterBehaviour.GetBehaviour() != null)
            {
                CharacterBehaviour.GetBehaviour().UpdateDrone();
            }

            OnNewDroneSelected?.Invoke();
        }

        public static DroneData GetSelectedDroneData()
        {
            if (SelectedDroneIndex < 0 || SelectedDroneIndex >= drones.Length)
                return null;

            return drones[SelectedDroneIndex];
        }

        public static DroneData GetDroneData(DroneType droneType)
        {
            return drones[dronesLink[droneType]];
        }

        public static DroneData GetDroneData(UpgradeType upgradeType)
        {
            for (int i = 0; i < drones.Length; i++)
            {
                if (drones[i].UpgradeType == upgradeType)
                    return drones[i];
            }

            return null;
        }

        public static void AddCards(DroneType droneType, int amount)
        {
            DroneData droneData = GetDroneData(droneType);
            droneData.Save.CardsAmount += amount;

            instance.CheckDroneUpdateState();

            OnDroneCardsAmountChanged?.Invoke();
        }

        public static void AddCards(List<DroneType> cards)
        {
            if (cards.IsNullOrEmpty())
                return;

            for (int i = 0; i < cards.Count; i++)
            {
                GetDroneData(cards[i]).Save.CardsAmount++;
            }

            instance.CheckDroneUpdateState();

            OnDroneCardsAmountChanged?.Invoke();
        }

        public static bool IsDroneUnlocked(DroneType type)
        {
            DroneData droneData = GetDroneData(type);
            BaseUpgrade upgrade = UpgradesController.GetUpgrade<BaseUpgrade>(droneData.UpgradeType);

            return upgrade.UpgradeLevel > 0;
        }

        public static void OnUpgradeBuyed(DroneData droneData)
        {
            BaseDroneUpgrade upgrade = UpgradesController.GetUpgrade<BaseDroneUpgrade>(droneData.UpgradeType);
            BaseDroneUpgradeStage nextStage = upgrade.NextStage as BaseDroneUpgradeStage;

            if (nextStage != null)
            {
                droneData.Save.CardsAmount -= nextStage.CardsRequired;
            }

            upgrade.UpgradeStage();

            OnDroneUpgraded?.Invoke();
        }
        
        public static DroneBehavior SpawnDrone(CharacterBehaviour player)
        {
            if (drones == null || drones.Length == 0)
            {
                Debug.LogError("[DronesController] drones array is null or empty!");
                return null;
            }

            DroneData droneData = GetSelectedDroneData();
            if (droneData == null)
            {
                // Không có drone nào đang được trang bị
                return null;
            }

            // Chỉ spawn drone nếu đã được mở khoá qua Gacha
            if (!droneData.Save.IsOwned)
            {
                Debug.Log("[DronesController] Drone chưa được mở khoá qua Gacha, không spawn.");
                return null;
            }

            BaseDroneUpgrade upgrade = UpgradesController.GetUpgrade<BaseDroneUpgrade>(droneData.UpgradeType);

            if (upgrade == null)
            {
                Debug.LogError("[DronesController] UpgradesController returned null for UpgradeType: " + droneData.UpgradeType);
                return null;
            }

            BaseDroneUpgradeStage currentStage = (BaseDroneUpgradeStage)upgrade.GetCurrentStage();

            // Fallback logic: If the current stage is missing prefabs (e.g. user forgot to assign in Inspector for level > 0),
            // search backwards to find the latest valid prefab.
            GameObject validDronePrefab = currentStage.DronePrefab;
            if (validDronePrefab == null)
            {
                for (int i = upgrade.UpgradeLevel; i >= 0; i--)
                {
                    BaseDroneUpgradeStage stage = (BaseDroneUpgradeStage)upgrade.Upgrades[i];
                    if (stage.DronePrefab != null)
                    {
                        validDronePrefab = stage.DronePrefab;
                        break;
                    }
                }
            }

            if (validDronePrefab != null)
            {
                GameObject droneObj = Object.Instantiate(validDronePrefab);
                droneObj.SetActive(true);

                DroneBehavior droneBehaviour = droneObj.GetComponent<DroneBehavior>();
                if (droneBehaviour != null)
                {
                    droneBehaviour.Initialise(player, currentStage);
                    return droneBehaviour;
                }
                else
                {
                    Debug.LogError("[DronesController] The DronePrefab " + validDronePrefab.name + " is missing the 'DroneBehavior' script!");
                }
            }
            else
            {
                Debug.LogError("[DronesController] validDronePrefab is NULL! Please assign a Drone Prefab in the Upgrade Data.");
            }

            return null;
        }

        [System.Serializable]
        public class GlobalDronesSave : ISaveObject
        {
            public int selectedDroneIndex = -1;

            public void Flush() { }
        }
    }
}

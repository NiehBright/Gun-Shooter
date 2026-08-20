using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class DroneData
    {
        [SerializeField] string name;
        public string Name => name;

        [SerializeField] DroneType type;
        public DroneType Type => type;

        [SerializeField] UpgradeType upgradeType;
        public UpgradeType UpgradeType => upgradeType;

        [SerializeField] Rarity rarity;
        public Rarity Rarity => rarity;

        [SerializeField] Sprite icon;
        public Sprite Icon => icon;

        public RarityData RarityData => WeaponsController.GetRarityData(rarity); // We can reuse WeaponsController's rarity data, or create a generic one

        private DroneSave save;
        public DroneSave Save => save;

        public int CardsAmount => save.CardsAmount;

        public void Initialise()
        {
            save = SaveController.GetSaveObject<DroneSave>(string.Format("Drone_{0}", type));
        }
    }
}

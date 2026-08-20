using Watermelon;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class DropData
    {
        public DropableItemType dropType;

        public CurrencyType currencyType;
        public WeaponType cardType;
        public DroneType droneType;

        public int amount;

        public DropData() { }

        public DropData Clone()
        {
            var data = new DropData();

            data.dropType = dropType;
            data.currencyType = currencyType;
            data.cardType = cardType;
            data.droneType = droneType;
            data.amount = amount;

            return data;
        }
    }
}
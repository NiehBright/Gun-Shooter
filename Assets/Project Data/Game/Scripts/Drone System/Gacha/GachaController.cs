using UnityEngine;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class GachaController : MonoBehaviour
    {
        [SerializeField] GachaDatabase database;
        public static GachaDatabase Database { get; private set; }

        private static GachaController instance;

        public static event System.Action<GachaResult> OnGachaCompleted;

        public void Initialise()
        {
            instance = this;
            Database = database;

            // Cho Gems miễn phí lần đầu chơi
            if (!SaveController.GetSaveObject<GachaSave>("gacha_save").HasReceivedStartingGems)
            {
                CurrenciesController.Add(CurrencyType.Gems, database.StartingGems);
                var save = SaveController.GetSaveObject<GachaSave>("gacha_save");
                save.HasReceivedStartingGems = true;
                SaveController.MarkAsSaveIsRequired();
                Debug.Log($"[Gacha] Đã tặng {database.StartingGems} Gems miễn phí cho người chơi mới!");
            }
        }

        /// <summary>
        /// Quay Gacha 1 lần
        /// </summary>
        public static GachaResult PullSingle()
        {
            if (!CurrenciesController.HasAmount(CurrencyType.Gems, Database.SinglePullPrice))
            {
                Debug.LogWarning("[Gacha] Không đủ Gems!");
                return null;
            }

            CurrenciesController.Substract(CurrencyType.Gems, Database.SinglePullPrice);
            
            GachaResult result = RollDrone();
            OnGachaCompleted?.Invoke(result);
            
            SaveController.MarkAsSaveIsRequired();
            return result;
        }

        /// <summary>
        /// Quay Gacha 10 lần
        /// </summary>
        public static GachaResult[] PullMulti()
        {
            if (!CurrenciesController.HasAmount(CurrencyType.Gems, Database.MultiPullPrice))
            {
                Debug.LogWarning("[Gacha] Không đủ Gems cho multi pull!");
                return null;
            }

            CurrenciesController.Substract(CurrencyType.Gems, Database.MultiPullPrice);

            GachaResult[] results = new GachaResult[Database.MultiPullCount];
            for (int i = 0; i < Database.MultiPullCount; i++)
            {
                results[i] = RollDrone();
                OnGachaCompleted?.Invoke(results[i]);
            }

            SaveController.MarkAsSaveIsRequired();
            return results;
        }

        /// <summary>
        /// Random chọn 1 drone từ database và xử lý kết quả
        /// </summary>
        private static GachaResult RollDrone()
        {
            DroneData[] drones = DronesController.Database.Drones;
            
            // Random chọn 1 drone (sau này có thể thêm weight theo rarity)
            int randomIndex = Random.Range(0, drones.Length);
            DroneData selectedDrone = drones[randomIndex];

            bool isNew = !selectedDrone.Save.IsOwned;

            if (isNew)
            {
                // Lần đầu: Mở khoá drone
                selectedDrone.Save.IsOwned = true;

                // Nếu chưa có drone nào được trang bị, tự động trang bị drone này
                if (DronesController.SelectedDroneIndex == -1)
                {
                    DronesController.SelectDrone(selectedDrone.Type);
                }

                Debug.Log($"[Gacha] MỞ KHOÁ DRONE MỚI: {selectedDrone.Name}!");
            }
            else
            {
                // Đã có: Nhận cards
                selectedDrone.Save.CardsAmount += Database.CardsPerDuplicate;
                Debug.Log($"[Gacha] 📦 Drone trùng: {selectedDrone.Name} → +{Database.CardsPerDuplicate} Cards (Tổng: {selectedDrone.Save.CardsAmount})");
            }

            return new GachaResult(selectedDrone, isNew, isNew ? 0 : Database.CardsPerDuplicate);
        }

        /// <summary>
        /// Kiểm tra đủ Gems cho quay đơn
        /// </summary>
        public static bool CanPullSingle()
        {
            return CurrenciesController.HasAmount(CurrencyType.Gems, Database.SinglePullPrice);
        }

        /// <summary>
        /// Kiểm tra đủ Gems cho quay x10
        /// </summary>
        public static bool CanPullMulti()
        {
            return CurrenciesController.HasAmount(CurrencyType.Gems, Database.MultiPullPrice);
        }
    }

    /// <summary>
    /// Kết quả 1 lần quay gacha
    /// </summary>
    public class GachaResult
    {
        public DroneData Drone { get; private set; }
        public bool IsNewDrone { get; private set; }
        public int CardsReceived { get; private set; }

        public GachaResult(DroneData drone, bool isNew, int cards)
        {
            Drone = drone;
            IsNewDrone = isNew;
            CardsReceived = cards;
        }
    }

    /// <summary>
    /// Save data cho hệ thống Gacha
    /// </summary>
    [System.Serializable]
    public class GachaSave : ISaveObject
    {
        public bool HasReceivedStartingGems = false;
        public int TotalPulls = 0;

        public void Flush() { }
    }
}

using UnityEngine;

namespace Watermelon.SquadShooter
{
    [CreateAssetMenu(fileName = "Gacha Database", menuName = "Content/Gacha Database")]
    public class GachaDatabase : ScriptableObject
    {
        [Header("Giá Gacha")]
        [SerializeField] int singlePullPrice = 50;
        public int SinglePullPrice => singlePullPrice;

        [SerializeField] int multiPullPrice = 450;
        public int MultiPullPrice => multiPullPrice;

        [SerializeField] int multiPullCount = 10;
        public int MultiPullCount => multiPullCount;

        [Header("Phần thưởng khi trùng")]
        [SerializeField] int cardsPerDuplicate = 5;
        public int CardsPerDuplicate => cardsPerDuplicate;

        [Header("Gems miễn phí lúc đầu (để test)")]
        [SerializeField] int startingGems = 100;
        public int StartingGems => startingGems;
    }
}

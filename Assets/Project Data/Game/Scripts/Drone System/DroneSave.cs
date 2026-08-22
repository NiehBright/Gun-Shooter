namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class DroneSave : ISaveObject
    {
        public int CardsAmount = 0;
        public bool IsOwned = false; // True khi đã gacha được drone này

        public void Flush()
        {

        }
    }
}

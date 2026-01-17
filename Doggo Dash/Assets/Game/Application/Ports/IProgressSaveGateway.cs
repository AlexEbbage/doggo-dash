namespace Game.Application.Ports
{
    public interface IProgressSaveGateway
    {
        PlayerProgressData Load();
        void Save(PlayerProgressData data);
    }

    [System.Serializable]
    public sealed class PlayerProgressData
    {
        public int totalKibble;
        public int totalGems;

        public int bestScore;
        public float bestDistanceMeters;

        public int xp;
        public int level = 1;
        public int xpToNext = 100;

        public string selectedPetId = "dog_default";
        public string selectedOutfitId = "outfit_default";
    }
}

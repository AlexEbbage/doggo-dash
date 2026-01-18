using System.Collections.Generic;

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

        public int level = 1;
        public int xp;
        public int xpToNext = 100;

        public float energyCurrent = 100f;
        public float energyMax = 100f;

        public long lastEnergyTimestampUtc;
        public long lastXpTimestampUtc;

        public string selectedPetId = "dog_default";
        public string selectedOutfitId = "outfit_default";

        public List<string> ownedPets = new List<string>();
        public List<string> ownedOutfits = new List<string>();
    }
}

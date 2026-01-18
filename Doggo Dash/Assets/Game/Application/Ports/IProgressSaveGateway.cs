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

        public long lastDailyChallengesResetUtc;
        public long lastWeeklyChallengesResetUtc;

        public List<ChallengeProgressEntry> challengeProgress = new();

        public string selectedPetId = "dog_default";
        public string selectedOutfitId = "outfit_default";
    }

    [System.Serializable]
    public sealed class ChallengeProgressEntry
    {
        public string id = string.Empty;
        public float progress;
        public bool completed;
        public long completedUtcSeconds;
    }
}

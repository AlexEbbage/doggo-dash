namespace Game.Application.Ports
{
    public interface IProgressSaveGateway
    {
        PlayerProgressData Load();
        void Save(PlayerProgressData data);
    }

        [System.Serializable]
    public sealed class SavedMissionData
    {
        public Game.Domain.ValueObjects.MissionType type;
        public int target;
        public int current;
        public bool completed;

        public int rewardKibble;
        public int rewardGems;
    }

    [System.Serializable]
    public sealed class PlayerProgressData
    {
        public int totalKibble;
        public int totalGems;

        public int bestScore;
        public float bestDistanceMeters;

        public string selectedPetId = "dog_default";
        public string selectedOutfitId = "outfit_default";
        public string selectedBiomeId = "farm_fields";

        public string[] ownedItemIds;

        // Missions (Feature 20)
        public SavedMissionData[] activeMissions;
        public int missionMultiplierLevel;
    }
}

using Game.Domain.ValueObjects;

namespace Game.Application.DTOs
{
    [System.Serializable]
    public struct MissionDefinitionData
    {
        public MissionType Type;
        public int Target;
        public int RewardKibble;
        public int RewardGems;
        public int Weight;
        public string DisplayName;
    }
}

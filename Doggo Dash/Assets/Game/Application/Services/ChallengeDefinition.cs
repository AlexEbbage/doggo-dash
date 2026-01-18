namespace Game.Application.Services
{
    public enum ChallengeMetric
    {
        DistanceMeters,
        TreatPickups,
        GemPickups,
        BadFoodHits
    }

    public enum ChallengePeriod
    {
        Daily,
        Weekly
    }

    [System.Serializable]
    public sealed class ChallengeDefinition
    {
        public string id = string.Empty;
        public string title = string.Empty;
        public string description = string.Empty;
        public ChallengeMetric metric;
        public ChallengePeriod period;
        public float target;
        public int rewardKibble;
        public int rewardGems;
    }
}

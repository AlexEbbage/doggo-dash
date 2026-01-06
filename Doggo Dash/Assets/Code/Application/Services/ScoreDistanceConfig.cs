namespace Game.Application.Services
{
    public sealed class ScoreDistanceConfig
    {
        public float ScorePerMeter { get; set; } = 1.0f;
        public float BaseMultiplier { get; set; } = 1.0f;
        public float MaxMultiplier { get; set; } = 30.0f;
    }
}

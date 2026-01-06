namespace Game.Application.Services
{
    public sealed class EnergySpeedConfig
    {
        public float MaxEnergy { get; set; } = 100f;
        public float DrainPerSecond { get; set; } = 2.5f;
        public float TreatRestore { get; set; } = 10f;
        public float BadFoodEnergyPenalty { get; set; } = 12f;
        public float BadFoodSlowMultiplier { get; set; } = 0.75f;
        public float BadFoodSlowDuration { get; set; } = 1.25f;
        public float MinSpeedMultiplier { get; set; } = 0.45f;
    }
}

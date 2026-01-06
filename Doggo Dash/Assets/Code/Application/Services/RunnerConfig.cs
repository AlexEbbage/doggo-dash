namespace Game.Application.Services
{
    /// <summary>
    /// Application-level config (Unity-free). Populate from ScriptableObject in Presentation.
    /// </summary>
    public sealed class RunnerConfig
    {
        public float LaneWidth { get; set; } = 2.2f;
        public float LaneChangeDuration { get; set; } = 0.12f;

        public float JumpHeight { get; set; } = 1.8f;
        public float JumpDuration { get; set; } = 0.65f;

        public float SlideDuration { get; set; } = 0.75f;

        public float BaseForwardSpeed { get; set; } = 9.0f;
    }
}

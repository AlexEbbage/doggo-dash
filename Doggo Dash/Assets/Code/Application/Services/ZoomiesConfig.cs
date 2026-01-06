namespace Game.Application.Services
{
    public sealed class ZoomiesConfig
    {
        public float SpeedMultiplier { get; set; } = 1.5f;
        public float DurationSeconds { get; set; } = 3.0f;
        public bool RefreshOnPickup { get; set; } = true;
        public bool AllowPickupWhileActive { get; set; } = true;
    }
}

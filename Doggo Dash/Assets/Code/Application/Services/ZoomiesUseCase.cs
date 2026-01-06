namespace Game.Application.Services
{
    public sealed class ZoomiesUseCase
    {
        private readonly ZoomiesConfig _config;
        private float _remaining;

        public ZoomiesUseCase(ZoomiesConfig config)
        {
            _config = config;
        }

        public bool IsActive => _remaining > 0f;
        public float SpeedMultiplier => IsActive ? _config.SpeedMultiplier : 1f;
        public float RemainingSeconds => _remaining;

        public void Tick(float dt)
        {
            if (_remaining <= 0f) return;
            _remaining -= dt;
            if (_remaining < 0f) _remaining = 0f;
        }

        public bool TryActivate()
        {
            if (IsActive)
            {
                if (!_config.AllowPickupWhileActive) return false;
                if (_config.RefreshOnPickup) _remaining = _config.DurationSeconds;
                return true;
            }

            _remaining = _config.DurationSeconds;
            return true;
        }

        public void Reset() => _remaining = 0f;
    }
}

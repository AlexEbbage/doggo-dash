using Game.Domain.Entities;

namespace Game.Application.Services
{
    public sealed class ScoreDistanceUseCase
    {
        private readonly ScoreDistanceConfig _config;
        private readonly ScoreDistanceState _state;

        private float _multiplier;
        private float _scoreRemainder;

        public ScoreDistanceUseCase(ScoreDistanceConfig config, ScoreDistanceState state)
        {
            _config = config;
            _state = state;
            _multiplier = ClampMultiplier(_config.BaseMultiplier);
        }

        public float DistanceMeters => _state.DistanceMeters;
        public int Score => _state.Score;
        public float Multiplier => _multiplier;

        public void Reset()
        {
            _state.Reset();
            _scoreRemainder = 0f;
            _multiplier = ClampMultiplier(_config.BaseMultiplier);
        }

        public void SetMultiplier(float multiplier) => _multiplier = ClampMultiplier(multiplier);

        public void Tick(float dt, float forwardSpeedMetersPerSec)
        {
            if (dt <= 0f) return;
            if (forwardSpeedMetersPerSec < 0f) forwardSpeedMetersPerSec = 0f;

            float d = forwardSpeedMetersPerSec * dt;
            _state.DistanceMeters += d;

            float scoreDelta = d * _config.ScorePerMeter * _multiplier;
            _scoreRemainder += scoreDelta;

            int add = (int)_scoreRemainder;
            if (add > 0)
            {
                _state.Score += add;
                _scoreRemainder -= add;
            }
        }

        private float ClampMultiplier(float m)
        {
            if (m < 1f) m = 1f;
            if (m > _config.MaxMultiplier) m = _config.MaxMultiplier;
            return m;
        }
    }
}

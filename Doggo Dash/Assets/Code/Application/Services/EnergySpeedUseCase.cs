using Game.Application.Ports;
using Game.Domain.Entities;

namespace Game.Application.Services
{
    public sealed class EnergySpeedUseCase
    {
        private readonly EnergySpeedConfig _config;
        private readonly EnergyState _energy;
        private readonly IRunFailSink _failSink;

        private float _slowRemaining;

        public EnergySpeedUseCase(EnergySpeedConfig config, EnergyState energy, IRunFailSink failSink)
        {
            _config = config;
            _energy = energy;
            _failSink = failSink;
        }

        public float Energy01 => _energy.Current / (_energy.Max <= 0f ? 1f : _energy.Max);
        public float EnergyCurrent => _energy.Current;
        public float EnergyMax => _energy.Max;

        public bool IsSlowed => _slowRemaining > 0f;

        public float CurrentSpeedMultiplier
        {
            get
            {
                float mult = IsSlowed ? _config.BadFoodSlowMultiplier : 1f;
                if (mult < _config.MinSpeedMultiplier) mult = _config.MinSpeedMultiplier;
                return mult;
            }
        }

        public void Reset()
        {
            _energy.Max = _config.MaxEnergy <= 0f ? 1f : _config.MaxEnergy;
            _energy.Current = _energy.Max;
            _slowRemaining = 0f;
        }

        public void Tick(float dt)
        {
            if (dt <= 0f) return;

            _energy.Current -= _config.DrainPerSecond * dt;
            if (_energy.Current <= 0f)
            {
                _energy.Current = 0f;
                _failSink.OnRunFailed(RunFailReason.EnergyDepleted, obstacleType: null);
                return;
            }

            if (_slowRemaining > 0f)
            {
                _slowRemaining -= dt;
                if (_slowRemaining < 0f) _slowRemaining = 0f;
            }
        }

        public void ApplyTreat(int amount)
        {
            if (amount <= 0) return;
            _energy.Current += _config.TreatRestore * amount;
            if (_energy.Current > _energy.Max) _energy.Current = _energy.Max;
        }

        public void ApplyBadFood()
        {
            _energy.Current -= _config.BadFoodEnergyPenalty;
            if (_energy.Current < 0f) _energy.Current = 0f;

            _slowRemaining = _config.BadFoodSlowDuration;

            if (_energy.Current <= 0f)
                _failSink.OnRunFailed(RunFailReason.EnergyDepleted, obstacleType: null);
        }
    }
}

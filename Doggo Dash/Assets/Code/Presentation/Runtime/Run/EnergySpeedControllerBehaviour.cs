using UnityEngine;
using Game.Application.Ports;
using Game.Application.Services;
using Game.Domain.Entities;
using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.Run
{
    public sealed class EnergySpeedControllerBehaviour : MonoBehaviour, IPickupCollectedSink, ISpeedModifierSource
    {
        [Header("Config")]
        public EnergySpeedConfigSO config = default!;

        [Header("Refs")]
        public RunStateControllerBehaviour runState = default!;

        [Header("Debug")]
        public bool logEnergy = false;

        private EnergySpeedUseCase _useCase = default!;
        private float _lastLogTime;

        public float Energy01 => _useCase.Energy01;
        public float SpeedMultiplier => _useCase.CurrentSpeedMultiplier;

        private void Awake()
        {
            var appConfig = new EnergySpeedConfig
            {
                MaxEnergy = config.maxEnergy,
                DrainPerSecond = config.drainPerSecond,
                TreatRestore = config.treatRestore,
                BadFoodEnergyPenalty = config.badFoodEnergyPenalty,
                BadFoodSlowMultiplier = config.badFoodSlowMultiplier,
                BadFoodSlowDuration = config.badFoodSlowDuration,
                MinSpeedMultiplier = config.minSpeedMultiplier
            };

            var energy = new EnergyState(appConfig.MaxEnergy);
            _useCase = new EnergySpeedUseCase(appConfig, energy, runState);
            _useCase.Reset();
        }

        private void Update()
        {
            if (runState != null && runState.IsFailed) return;

            _useCase.Tick(Time.deltaTime);

            if (logEnergy && Time.time - _lastLogTime > 0.5f)
            {
                _lastLogTime = Time.time;
                Debug.Log($"[Energy] {_useCase.EnergyCurrent:0.0}/{_useCase.EnergyMax:0.0}  SpeedMult={_useCase.CurrentSpeedMultiplier:0.00}");
            }
        }

        public void OnPickupCollected(PickupType type, int amount)
        {
            switch (type)
            {
                case PickupType.Treat:
                    _useCase.ApplyTreat(amount);
                    break;
                case PickupType.BadFood:
                    _useCase.ApplyBadFood();
                    break;
                default:
                    break;
            }
        }

        public void ResetRun() => _useCase.Reset();
    }
}

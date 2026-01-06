using UnityEngine;
using Game.Application.Services;
using Game.Domain.Entities;
using Game.Presentation.Runtime.Runner;

namespace Game.Presentation.Runtime.Run
{
    public sealed class ScoreDistanceControllerBehaviour : MonoBehaviour
    {
        [Header("Config")]
        public ScoreDistanceConfigSO config = default!;

        [Header("Refs")]
        public RunnerControllerBehaviour runner = default!;
        public RunStateControllerBehaviour runState = default!;

        private ScoreDistanceUseCase _useCase = default!;

        public float DistanceMeters => _useCase.DistanceMeters;
        public int Score => _useCase.Score;
        public float Multiplier => _useCase.Multiplier;

        private void Awake()
        {
            var appConfig = new ScoreDistanceConfig
            {
                ScorePerMeter = config.scorePerMeter,
                BaseMultiplier = config.baseMultiplier,
                MaxMultiplier = config.maxMultiplier
            };

            var state = new ScoreDistanceState();
            _useCase = new ScoreDistanceUseCase(appConfig, state);
            _useCase.Reset();
        }

        private void Update()
        {
            if (runState != null && runState.IsFailed) return;
            float speed = runner != null ? runner.CurrentForwardSpeed : 0f;
            _useCase.Tick(Time.deltaTime, speed);
        }

        public void ResetRun() => _useCase.Reset();
        public void SetMultiplier(float multiplier) => _useCase.SetMultiplier(multiplier);
    }
}

using UnityEngine;
using Game.Application.Ports;

namespace Game.Presentation.Runtime.Run
{
    public sealed class RunDifficultyControllerBehaviour : MonoBehaviour, ISpeedModifierSource
    {
        [Header("Config")]
        public DifficultyConfigSO config = default!;

        [Header("Refs")]
        [Tooltip("Assign a component implementing IRunnerDistanceProvider.")]
        public MonoBehaviour runnerDistanceProviderBehaviour = default!;
        public RunStateControllerBehaviour runState = default!;

        private IRunnerDistanceProvider _distance = default!;

        public float SpeedMultiplier { get; private set; } = 1f;
        public float DensityMultiplier { get; private set; } = 1f;

        public float DistanceMeters => _distance != null ? _distance.DistanceTravelledMeters : 0f;

        private void Awake()
        {
            _distance = runnerDistanceProviderBehaviour as IRunnerDistanceProvider;
            if (_distance == null)
            {
                Debug.LogError("[RunDifficulty] runnerDistanceProviderBehaviour must implement IRunnerDistanceProvider.");
                enabled = false;
                return;
            }

            if (config == null)
            {
                Debug.LogError("[RunDifficulty] config not assigned.");
                enabled = false;
                return;
            }
        }

        private void Update()
        {
            if (runState != null && runState.IsFailed) return;

            float denom = Mathf.Max(0.0001f, config.distanceForMax);
            float t = Mathf.Clamp01(_distance.DistanceTravelledMeters / denom);

            float speed = config.speedMultiplierByT != null ? config.speedMultiplierByT.Evaluate(t) : 1f;
            float dens = config.densityMultiplierByT != null ? config.densityMultiplierByT.Evaluate(t) : 1f;

            SpeedMultiplier = Mathf.Clamp(speed, config.minSpeedMultiplier, config.maxSpeedMultiplier);
            DensityMultiplier = Mathf.Clamp(dens, config.minDensityMultiplier, config.maxDensityMultiplier);
        }
    }
}

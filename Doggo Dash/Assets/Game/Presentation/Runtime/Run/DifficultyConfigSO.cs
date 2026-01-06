using UnityEngine;

namespace Game.Presentation.Runtime.Run
{
    [CreateAssetMenu(menuName = "Game/Configs/Difficulty Config", fileName = "DifficultyConfig")]
    public sealed class DifficultyConfigSO : ScriptableObject
    {
        [Header("Distance Domain")]
        [Tooltip("Curves evaluate at t = distanceMeters / distanceForMax (clamped 0..1).")]
        [Min(10f)] public float distanceForMax = 800f;

        [Header("Speed")]
        [Tooltip("Speed multiplier over run distance. 1 = no change, >1 faster.")]
        public AnimationCurve speedMultiplierByT = AnimationCurve.Linear(0f, 1f, 1f, 1.35f);

        [Header("Density")]
        [Tooltip("Spawn density multiplier over run distance. 1 = baseline.")]
        public AnimationCurve densityMultiplierByT = AnimationCurve.Linear(0f, 1f, 1f, 1.4f);

        [Header("Clamps")]
        [Min(0.1f)] public float minSpeedMultiplier = 0.8f;
        [Min(0.1f)] public float maxSpeedMultiplier = 2.2f;

        [Min(0.1f)] public float minDensityMultiplier = 0.6f;
        [Min(0.1f)] public float maxDensityMultiplier = 2.0f;
    }
}

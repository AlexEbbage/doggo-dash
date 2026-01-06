using UnityEngine;

namespace Game.Presentation.Runtime.Run
{
    [CreateAssetMenu(menuName = "Game/Configs/Energy Speed Config", fileName = "EnergySpeedConfig")]
    public sealed class EnergySpeedConfigSO : ScriptableObject
    {
        [Header("Energy")]
        [Min(1f)] public float maxEnergy = 100f;
        [Min(0f)] public float drainPerSecond = 2.5f;
        [Min(0f)] public float treatRestore = 10f;
        [Min(0f)] public float badFoodEnergyPenalty = 12f;

        [Header("Speed (Bad Food Slow)")]
        [Range(0.1f, 1f)] public float badFoodSlowMultiplier = 0.75f;
        [Min(0f)] public float badFoodSlowDuration = 1.25f;
        [Range(0.1f, 1f)] public float minSpeedMultiplier = 0.45f;
    }
}

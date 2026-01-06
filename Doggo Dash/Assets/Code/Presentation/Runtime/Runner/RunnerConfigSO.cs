using UnityEngine;

namespace Game.Presentation.Runtime.Runner
{
    [CreateAssetMenu(menuName = "Game/Configs/Runner Config", fileName = "RunnerConfig")]
    public sealed class RunnerConfigSO : ScriptableObject
    {
        [Header("Lanes")]
        [Min(0.1f)] public float laneWidth = 2.2f;
        [Min(0.02f)] public float laneChangeDuration = 0.12f;

        [Header("Jump")]
        [Min(0.1f)] public float jumpHeight = 1.8f;
        [Min(0.1f)] public float jumpDuration = 0.65f;

        [Header("Slide")]
        [Min(0.1f)] public float slideDuration = 0.75f;

        [Header("Forward")]
        [Min(0.1f)] public float baseForwardSpeed = 9.0f;

        [Header("Slide Collider")]
        [Range(0.2f, 1f)] public float slideHeightMultiplier = 0.5f;
        [Min(0f)] public float slideColliderLerpTime = 0.05f;
    }
}

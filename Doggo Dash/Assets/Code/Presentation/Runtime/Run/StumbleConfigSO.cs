using UnityEngine;

namespace Game.Presentation.Runtime.Run
{
    [CreateAssetMenu(menuName = "Game/Configs/Stumble Config", fileName = "StumbleConfig")]
    public sealed class StumbleConfigSO : ScriptableObject
    {
        [Header("Enable")]
        public bool useStumble = true;

        [Header("Speed")]
        [Range(0.1f, 1f)]
        public float stumbleSpeedMultiplier = 0.55f;

        [Min(0.05f)]
        public float stumbleDurationSeconds = 0.75f;

        [Header("Input Lock (optional)")]
        public bool lockInputDuringStumble = true;

        [Header("Invulnerability")]
        [Min(0f)]
        [Tooltip("During this time, obstacle hits won't retrigger stumble. If failOnSecondHitDuringInvuln is true, they will end the run.")]
        public float invulnerabilitySeconds = 0.9f;

        public bool failOnSecondHitDuringInvuln = true;
    }
}

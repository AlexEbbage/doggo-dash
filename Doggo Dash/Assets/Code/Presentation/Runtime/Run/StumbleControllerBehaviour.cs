using UnityEngine;
using Game.Application.Ports;
using Game.Presentation.Runtime.Input;
using Game.Presentation.Runtime.FX;

namespace Game.Presentation.Runtime.Run
{
    /// <summary>
    /// Handles stumble state:
    /// - Provides speed multiplier (for SpeedCompositor)
    /// - Optional input lock while stumbling
    /// - Invulnerability window (with optional fail-on-second-hit)
    /// </summary>
    public sealed class StumbleControllerBehaviour : MonoBehaviour, ISpeedModifierSource
    {
        [Header("Config")]
        public StumbleConfigSO config = default!;

        [Header("Refs")]
        public RunStateControllerBehaviour runState = default!;
        public SwipeInputBehaviour swipeInput = default!;

        [Header("Feedback (optional)")]
        public RunFeedbackControllerBehaviour feedback;

        private float _stumbleRemaining;
        private float _invulnRemaining;

        public bool IsStumbling => _stumbleRemaining > 0f;
        public bool IsInvulnerable => _invulnRemaining > 0f;

        public float SpeedMultiplier
        {
            get
            {
                if (config == null || !config.useStumble) return 1f;
                return IsStumbling ? Mathf.Clamp(config.stumbleSpeedMultiplier, 0f, 1f) : 1f;
            }
        }

        private void Update()
        {
            if (runState != null && runState.IsFailed) return;

            float dt = Time.deltaTime;

            if (_stumbleRemaining > 0f)
            {
                _stumbleRemaining -= dt;
                if (_stumbleRemaining <= 0f)
                {
                    _stumbleRemaining = 0f;
                    ApplyInputLock(false);
                }
            }

            if (_invulnRemaining > 0f)
            {
                _invulnRemaining -= dt;
                if (_invulnRemaining < 0f) _invulnRemaining = 0f;
            }
        }

        public void TriggerStumble()
        {
            if (config == null || !config.useStumble) return;

            _stumbleRemaining = Mathf.Max(_stumbleRemaining, config.stumbleDurationSeconds);
            _invulnRemaining = Mathf.Max(_invulnRemaining, config.invulnerabilitySeconds);

            if (config.lockInputDuringStumble)
                ApplyInputLock(true);

            feedback?.PlayStumble();
        }

        public bool ShouldFailOnHitDuringInvuln()
        {
            return config != null && config.useStumble && config.failOnSecondHitDuringInvuln && IsInvulnerable;
        }

        private void ApplyInputLock(bool locked)
        {
            if (swipeInput == null) return;
            swipeInput.enabled = !locked;
        }

        public void ResetRun()
        {
            _stumbleRemaining = 0f;
            _invulnRemaining = 0f;
            ApplyInputLock(false);
        }
    }
}

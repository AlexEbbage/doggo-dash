using UnityEngine;
using Game.Application.Ports;
using Game.Application.Services;
using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.Run
{
    public sealed class ZoomiesControllerBehaviour : MonoBehaviour, IPickupCollectedSink, ISpeedModifierSource
    {
        [Header("Config")]
        public ZoomiesConfigSO config = default!;

        [Header("Debug")]
        public bool logStateChanges = true;

        private ZoomiesUseCase _useCase = default!;
        private bool _wasActive;

        public float SpeedMultiplier => _useCase.SpeedMultiplier;

        public bool IsActive => _useCase.IsActive;
        public float RemainingSeconds => _useCase.RemainingSeconds;

        private void Awake()
        {
            var appConfig = new ZoomiesConfig
            {
                SpeedMultiplier = config.speedMultiplier,
                DurationSeconds = config.durationSeconds,
                RefreshOnPickup = config.refreshOnPickup,
                AllowPickupWhileActive = config.allowPickupWhileActive
            };

            _useCase = new ZoomiesUseCase(appConfig);
            _useCase.Reset();
        }

        private void Update()
        {
            _useCase.Tick(Time.deltaTime);

            bool isActive = _useCase.IsActive;
            if (logStateChanges && isActive != _wasActive)
            {
                _wasActive = isActive;
                if (isActive) Debug.Log($"[Zoomies] START ({config.speedMultiplier:0.00}x for {config.durationSeconds:0.0}s)");
                else Debug.Log("[Zoomies] END");
            }
        }

        public void OnPickupCollected(PickupType type, int amount)
        {
            if (type != PickupType.Zoomies) return;
            bool activated = _useCase.TryActivate();
            if (activated && logStateChanges)
                Debug.Log($"[Zoomies] Pickup collected. Remaining={_useCase.RemainingSeconds:0.0}s");
        }

        public void ResetRun()
        {
            _useCase.Reset();
            _wasActive = false;
        }
    }
}

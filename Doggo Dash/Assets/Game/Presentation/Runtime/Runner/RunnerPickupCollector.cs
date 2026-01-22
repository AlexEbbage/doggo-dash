using UnityEngine;
using Game.Application.Ports;
using Game.Domain.ValueObjects;
using Game.Presentation.Runtime.FX;
using Game.Presentation.Runtime.World.Pickups;

namespace Game.Presentation.Runtime.Runner
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class RunnerPickupCollector : MonoBehaviour
    {
        [Header("Wiring")]
        public MonoBehaviour pickupSinkBehaviour = default!;

        [Header("Collection")]
        public bool destroyOnCollect = true;

        [Header("Feedback")]
        public RunFeedbackControllerBehaviour feedback;

        private IPickupCollectedSink _sink = default!;

        private void Awake()
        {
            _sink = pickupSinkBehaviour as IPickupCollectedSink;
            if (_sink == null)
            {
                Debug.LogError("[RunnerPickupCollector] pickupSinkBehaviour must implement IPickupCollectedSink.");
                enabled = false;
            }

            if (feedback == null)
                feedback = FindObjectOfType<RunFeedbackControllerBehaviour>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null) return;

            if (other.TryGetComponent<PickupView>(out var pickup))
            {
                _sink.OnPickupCollected(pickup.pickupType, pickup.amount);
                PlayPickupFeedback(pickup);

                if (pickup.TryGetComponent<PickupPoolHandle>(out var poolHandle))
                {
                    poolHandle.Release(pickup);
                }
                else if (destroyOnCollect)
                {
                    Destroy(pickup.gameObject);
                }
                else
                {
                    pickup.gameObject.SetActive(false);
                }
            }
        }

        private void PlayPickupFeedback(PickupView pickup)
        {
            if (feedback == null || pickup == null) return;

            var position = pickup.transform.position;
            switch (pickup.pickupType)
            {
                case PickupType.Treat:
                    feedback.PlayPickupTreat(position);
                    break;
                case PickupType.Gem:
                    feedback.PlayPickupGem(position);
                    break;
                case PickupType.BadFood:
                    feedback.PlayPickupBadFood(position);
                    break;
                case PickupType.Zoomies:
                    feedback.PlayPickupZoomies(position);
                    break;
            }
        }
    }
}

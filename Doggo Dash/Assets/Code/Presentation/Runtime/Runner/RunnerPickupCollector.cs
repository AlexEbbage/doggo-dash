using UnityEngine;
using Game.Application.Ports;
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

        private IPickupCollectedSink _sink = default!;

        private void Awake()
        {
            _sink = pickupSinkBehaviour as IPickupCollectedSink;
            if (_sink == null)
            {
                Debug.LogError("[RunnerPickupCollector] pickupSinkBehaviour must implement IPickupCollectedSink.");
                enabled = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null) return;

            if (other.TryGetComponent<PickupView>(out var pickup))
            {
                _sink.OnPickupCollected(pickup.pickupType, pickup.amount);

                if (destroyOnCollect)
                    Destroy(pickup.gameObject);
                else
                    pickup.gameObject.SetActive(false);
            }
        }
    }
}

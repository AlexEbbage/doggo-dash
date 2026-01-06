using UnityEngine;
using Game.Domain.ValueObjects;
using Game.Presentation.Runtime.FX;

namespace Game.Presentation.Runtime.World.Pickups
{
    [RequireComponent(typeof(PickupView))]
    public sealed class PickupFeedbackOnTriggerBehaviour : MonoBehaviour
    {
        public RunFeedbackControllerBehaviour feedback = default!;
        private PickupView _pickup;

        private void Awake()
        {
            _pickup = GetComponent<PickupView>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (feedback == null) return;
            if (other == null) return;
            if (other.GetComponent<CharacterController>() == null) return;

            Vector3 at = transform.position;

            switch (_pickup.pickupType)
            {
                case PickupType.Treat: feedback.PlayPickupTreat(at); break;
                case PickupType.Gem: feedback.PlayPickupGem(at); break;
                case PickupType.BadFood: feedback.PlayPickupBadFood(at); break;
                case PickupType.Zoomies: feedback.PlayPickupZoomies(at); break;
            }
        }
    }
}

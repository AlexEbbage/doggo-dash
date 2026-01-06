using UnityEngine;
using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.World.Pickups
{
    public sealed class PickupView : MonoBehaviour
    {
        [Header("Pickup")]
        public PickupType pickupType = PickupType.Treat;
        [Min(1)] public int amount = 1;

        [Header("FX")]
        public bool rotate = true;
        public float rotateDegreesPerSecond = 180f;
        public bool bob = true;
        public float bobAmplitude = 0.15f;
        public float bobFrequency = 2.0f;

        private Vector3 _startLocalPos;

        private void Awake() => _startLocalPos = transform.localPosition;

        private void Update()
        {
            if (rotate)
                transform.Rotate(0f, rotateDegreesPerSecond * Time.deltaTime, 0f, Space.World);

            if (bob)
            {
                float y = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
                transform.localPosition = new Vector3(_startLocalPos.x, _startLocalPos.y + y, _startLocalPos.z);
            }
        }
    }
}

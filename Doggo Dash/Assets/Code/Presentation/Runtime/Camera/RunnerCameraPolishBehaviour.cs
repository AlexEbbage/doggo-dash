using UnityEngine;
using Game.Presentation.Runtime.Run;

namespace Game.Presentation.Runtime.Camera
{
    public sealed class RunnerCameraPolishBehaviour : MonoBehaviour
    {
        [Header("Refs")]
        public Transform runner = default!;
        public Camera cam = default!;
        public MonoBehaviour zoomiesBehaviour; // assign ZoomiesControllerBehaviour (optional)
        public RunStateControllerBehaviour runState = default!;

        [Header("Follow")]
        public Vector3 offset = new Vector3(0f, 5.0f, -9.0f);
        [Min(0f)] public float positionSmooth = 10f;
        [Min(0f)] public float rotationSmooth = 12f;
        public Vector3 lookOffset = new Vector3(0f, 2.0f, 10.0f);

        [Header("Lane Sway (continuous)")]
        public float laneSwayFactor = 0.18f;

        [Header("Lane Impulse (on lane change)")]
        public float laneImpulseAmount = 0.45f;
        public float laneImpulseDamping = 12f;

        [Header("FOV")]
        public float baseFov = 60f;
        public float zoomiesFov = 72f;
        [Min(0f)] public float fovSmooth = 8f;

        private float _laneImpulse;
        private float _lastRunnerX;

        private void Reset()
        {
            cam = UnityEngine.Camera.main;
        }

        private void Awake()
        {
            if (cam == null) cam = UnityEngine.Camera.main;
            if (runner != null) _lastRunnerX = runner.position.x;
        }

        private void LateUpdate()
        {
            if (runState != null && runState.IsFailed) return;
            if (runner == null || cam == null) return;

            float dx = runner.position.x - _lastRunnerX;
            _lastRunnerX = runner.position.x;

            float impulseAdd = Mathf.Clamp(dx * 10f, -1f, 1f) * laneImpulseAmount;
            _laneImpulse += impulseAdd;
            _laneImpulse = Mathf.Lerp(_laneImpulse, 0f, 1f - Mathf.Exp(-laneImpulseDamping * Time.deltaTime));

            float sway = runner.position.x * laneSwayFactor;

            Vector3 desiredPos = runner.position + offset + new Vector3(sway + _laneImpulse, 0f, 0f);
            transform.position = Vector3.Lerp(transform.position, desiredPos, 1f - Mathf.Exp(-positionSmooth * Time.deltaTime));

            Vector3 lookTarget = runner.position + lookOffset;
            Quaternion desiredRot = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, 1f - Mathf.Exp(-rotationSmooth * Time.deltaTime));

            bool zoomiesActive = false;
            if (zoomiesBehaviour != null)
            {
                var prop = zoomiesBehaviour.GetType().GetProperty("IsActive");
                if (prop != null && prop.PropertyType == typeof(bool))
                    zoomiesActive = (bool)prop.GetValue(zoomiesBehaviour);
            }

            float targetFov = zoomiesActive ? zoomiesFov : baseFov;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, 1f - Mathf.Exp(-fovSmooth * Time.deltaTime));
        }
    }
}

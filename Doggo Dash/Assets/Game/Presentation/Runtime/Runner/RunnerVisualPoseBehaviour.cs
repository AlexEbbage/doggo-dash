using UnityEngine;

namespace Game.Presentation.Runtime.Runner
{
    public sealed class RunnerVisualPoseBehaviour : MonoBehaviour
    {
        public RunnerControllerBehaviour runner = default!;
        public Transform modelRoot = default!;

        [Header("Slide")]
        public Vector3 slideScale = new Vector3(1f, 0.6f, 1.2f);
        public float slideYOffset = -0.4f;

        [Header("Jump")]
        public float jumpTiltDegrees = 8f;

        [Header("Smoothing")]
        public float smooth = 18f;

        private Vector3 _defaultScale;
        private Vector3 _defaultLocalPos;
        private Quaternion _defaultLocalRot;

        private void Awake()
        {
            if (runner == null) runner = GetComponentInParent<RunnerControllerBehaviour>();
            _defaultScale = modelRoot.localScale;
            _defaultLocalPos = modelRoot.localPosition;
            _defaultLocalRot = modelRoot.localRotation;
        }

        private void LateUpdate()
        {
            if (runner == null || modelRoot == null) return;

            Vector3 targetScale = _defaultScale;
            Vector3 targetPos = _defaultLocalPos;
            Quaternion targetRot = _defaultLocalRot;

            if (runner.IsSliding)
            {
                targetScale = Vector3.Scale(_defaultScale, slideScale);
                targetPos = _defaultLocalPos + new Vector3(0f, slideYOffset, 0f);
            }
            else if (runner.IsJumping)
            {
                targetRot = _defaultLocalRot * Quaternion.Euler(-jumpTiltDegrees, 0f, 0f);
            }

            float t = 1f - Mathf.Exp(-smooth * Time.deltaTime);
            modelRoot.localScale = Vector3.Lerp(modelRoot.localScale, targetScale, t);
            modelRoot.localPosition = Vector3.Lerp(modelRoot.localPosition, targetPos, t);
            modelRoot.localRotation = Quaternion.Slerp(modelRoot.localRotation, targetRot, t);
        }
    }
}

using UnityEngine;

namespace Game.Presentation.Runtime.World.Curvature
{
    public sealed class CurvedWorldControllerBehaviour : MonoBehaviour
    {
        [Header("Global Curvature")]
        [Min(0f)] public float curveStrength = 0.0025f;
        public float curveStartZ = 10f;

        [Header("Optional Vertical Dip")]
        [Min(0f)] public float verticalStrength = 0.0f;

        [Header("Apply")]
        public bool applyInEditMode = true;

        private static readonly int GlobalCurveStrengthId = Shader.PropertyToID("_GlobalCurveStrength");
        private static readonly int GlobalCurveStartZId = Shader.PropertyToID("_GlobalCurveStartZ");
        private static readonly int GlobalVerticalId = Shader.PropertyToID("_GlobalVerticalStrength");

        private void OnEnable() => Apply();

#if UNITY_EDITOR
        private void Update()
        {
            if (!UnityEngine.Application.isPlaying && applyInEditMode)
                Apply();
        }
#endif

        private void OnValidate()
        {
            if (applyInEditMode)
                Apply();
        }

        public void Apply()
        {
            Shader.SetGlobalFloat(GlobalCurveStrengthId, curveStrength);
            Shader.SetGlobalFloat(GlobalCurveStartZId, curveStartZ);
            Shader.SetGlobalFloat(GlobalVerticalId, verticalStrength);
        }

        public void Clear()
        {
            Shader.SetGlobalFloat(GlobalCurveStrengthId, 0f);
            Shader.SetGlobalFloat(GlobalCurveStartZId, 0f);
            Shader.SetGlobalFloat(GlobalVerticalId, 0f);
        }
    }
}

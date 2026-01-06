using UnityEngine;
using Game.Application.Ports;
using Game.Presentation.Runtime.Runner;

namespace Game.Presentation.Runtime.Run
{
    public sealed class SpeedCompositorBehaviour : MonoBehaviour
    {
        [Header("Refs")]
        public RunnerControllerBehaviour runner = default!;
        public MonoBehaviour[] multiplierSources = default!;
        public RunStateControllerBehaviour runState = default!;

        [Header("Safety")]
        [Min(0.1f)] public float maxSpeedMultiplier = 3.0f;

        private void Update()
        {
            if (runState != null && runState.IsFailed) return;
            if (runner == null) return;

            float mult = 1f;

            if (multiplierSources != null)
            {
                for (int i = 0; i < multiplierSources.Length; i++)
                {
                    if (multiplierSources[i] is ISpeedModifierSource src)
                        mult *= Mathf.Max(0f, src.SpeedMultiplier);
                }
            }

            mult = Mathf.Min(mult, maxSpeedMultiplier);

            float baseSpeed = runner.BaseForwardSpeed;
            runner.SetExternalForwardSpeed(baseSpeed * mult);
        }
    }
}

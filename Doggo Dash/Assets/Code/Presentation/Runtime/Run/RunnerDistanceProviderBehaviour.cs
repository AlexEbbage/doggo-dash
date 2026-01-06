using UnityEngine;
using Game.Presentation.Runtime.Runner;

namespace Game.Presentation.Runtime.Run
{
    public sealed class RunnerDistanceProviderBehaviour : MonoBehaviour, IRunnerDistanceProvider
    {
        [Header("Refs")]
        public RunnerControllerBehaviour runner = default!;
        public RunStateControllerBehaviour runState = default!;

        public float DistanceTravelledMeters { get; private set; }

        private void Update()
        {
            if (runState != null && runState.IsFailed) return;
            if (runner == null) return;

            float speed = runner.CurrentForwardSpeed;
            if (speed < 0f) speed = 0f;

            DistanceTravelledMeters += speed * Time.deltaTime;
        }
    }
}

using UnityEngine;
using Game.Application.Ports;
using Game.Domain.ValueObjects;
using Game.Presentation.Runtime.World.Obstacles;

namespace Game.Presentation.Runtime.Runner
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class RunnerCollisionReporter : MonoBehaviour
    {
        [Header("Wiring")]
        public MonoBehaviour failSinkBehaviour = default!; // must implement IRunFailSink

        private IRunFailSink _failSink = default!;
        private bool _hasFailed;

        private void Awake()
        {
            _failSink = failSinkBehaviour as IRunFailSink;
            if (_failSink == null)
            {
                Debug.LogError("[RunnerCollisionReporter] failSinkBehaviour must implement IRunFailSink.");
                enabled = false;
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (_hasFailed) return;
            if (hit.collider == null) return;
            if (hit.collider.isTrigger) return;

            if (hit.collider.TryGetComponent<ObstacleView>(out var obstacle) && obstacle.isFatal)
            {
                _hasFailed = true;
                _failSink.OnRunFailed(RunFailReason.ObstacleHit, obstacle.obstacleType);
            }
        }

        public void ResetFailedState() => _hasFailed = false;
    }
}

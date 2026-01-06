using UnityEngine;
using Game.Application.Ports;
using Game.Domain.ValueObjects;
using Game.Presentation.Runtime.World.Obstacles;
using Game.Presentation.Runtime.Run;

namespace Game.Presentation.Runtime.Runner
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class RunnerCollisionReporter : MonoBehaviour
    {
        [Header("Wiring")]
        [Tooltip("Must implement IRunFailSink (RunStateControllerBehaviour).")]
        public MonoBehaviour failSinkBehaviour = default!;

        [Tooltip("Runner controller that exposes IsJumping/IsSliding. (RunnerControllerBehaviour)")]
        public RunnerControllerBehaviour runnerController = default!;

        [Tooltip("Optional: if assigned, unsafe hits trigger stumble instead of instant fail (per config).")]
        public StumbleControllerBehaviour stumbleController;

        private IRunFailSink _failSink = default!;
        private bool _hasFailed;

        private void Awake()
        {
            _failSink = failSinkBehaviour as IRunFailSink;
            if (_failSink == null)
            {
                Debug.LogError("[RunnerCollisionReporter] failSinkBehaviour must implement IRunFailSink.");
                enabled = false;
                return;
            }

            if (runnerController == null)
            {
                Debug.LogError("[RunnerCollisionReporter] runnerController is not assigned.");
                enabled = false;
                return;
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (_hasFailed) return;
            if (hit.collider == null) return;
            if (hit.collider.isTrigger) return;

            // Ground ignore
            if (hit.moveDirection.y > 0.5f) return;

            if (!hit.collider.TryGetComponent<ObstacleView>(out var obstacle))
                return;

            if (!obstacle.isFatal)
                return;

            bool safe = IsSafeAgainstObstacle(obstacle);
            if (safe) return;

            if (stumbleController != null && stumbleController.config != null && stumbleController.config.useStumble)
            {
                if (stumbleController.ShouldFailOnHitDuringInvuln())
                {
                    _hasFailed = true;
                    _failSink.OnRunFailed(RunFailReason.ObstacleHit, obstacle.obstacleType);
                    return;
                }

                stumbleController.TriggerStumble();
                return;
            }

            _hasFailed = true;
            _failSink.OnRunFailed(RunFailReason.ObstacleHit, obstacle.obstacleType);
        }

        private bool IsSafeAgainstObstacle(ObstacleView obstacle)
        {
            return obstacle.obstacleType switch
            {
                ObstacleType.Low => runnerController.IsJumping,
                ObstacleType.High => runnerController.IsSliding,
                ObstacleType.FullBlock => false,
                _ => false
            };
        }

        public void ResetFailedState()
        {
            _hasFailed = false;
        }
    }
}

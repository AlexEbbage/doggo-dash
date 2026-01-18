using UnityEngine;
using Game.Presentation.Runtime.Runner;

namespace Game.Presentation.Runtime.Run
{
    public sealed class RunResetControllerBehaviour : MonoBehaviour
    {
        [Header("Refs")]
        public EnergySpeedControllerBehaviour energySpeed = default!;
        public ZoomiesControllerBehaviour zoomies = default!;
        public RunRewardTrackerBehaviour rewardTracker = default!;
        public ScoreDistanceControllerBehaviour scoreDistance = default!;
        public StumbleControllerBehaviour stumbleController = default!;
        public RunnerCollisionReporter runnerCollisionReporter = default!;

        public void ResetRun()
        {
            energySpeed?.ResetRun();
            zoomies?.ResetRun();
            rewardTracker?.ResetRun();
            scoreDistance?.ResetRun();
            stumbleController?.ResetRun();
            runnerCollisionReporter?.ResetFailedState();
        }
    }
}

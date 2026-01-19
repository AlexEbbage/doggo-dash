using UnityEngine;
using Game.Application.Ports;
using Game.Domain.ValueObjects;
#if FIREBASE_ANALYTICS
using Firebase.Analytics;
#endif

namespace Game.Presentation.Runtime.Run
{
    public sealed class RunAnalyticsReporter : MonoBehaviour
    {
        [Header("Refs")]
        public ScoreDistanceControllerBehaviour scoreDistance = default!;

        public void ReportRunFailed(RunFailReason reason, ObstacleType? obstacleType = null)
        {
            int meters = scoreDistance != null ? Mathf.FloorToInt(scoreDistance.DistanceMeters) : 0;
            int score = scoreDistance != null ? scoreDistance.Score : 0;
            string obstacle = obstacleType?.ToString() ?? "n/a";
#if FIREBASE_ANALYTICS
            FirebaseAnalytics.LogEvent(
                "run_failed",
                new Parameter("distance_meters", meters),
                new Parameter("score", score),
                new Parameter("reason", reason.ToString()),
                new Parameter("obstacle", obstacle));
#else
            Debug.LogWarning(
                $"[Analytics] Firebase Analytics not enabled. Run failed: distanceMeters={meters} score={score} reason={reason} obstacle={obstacle}");
#endif
        }
    }
}

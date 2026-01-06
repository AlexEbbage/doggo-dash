using UnityEngine;

namespace Game.Presentation.Runtime.Run
{
    [CreateAssetMenu(menuName = "Game/Configs/Score Distance Config", fileName = "ScoreDistanceConfig")]
    public sealed class ScoreDistanceConfigSO : ScriptableObject
    {
        [Min(0.01f)] public float scorePerMeter = 1.0f;
        [Min(1f)] public float baseMultiplier = 1.0f;
        [Min(1f)] public float maxMultiplier = 30.0f;
    }
}

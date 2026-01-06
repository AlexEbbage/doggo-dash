using UnityEngine;

namespace Game.Presentation.Runtime.World.Track
{
    [CreateAssetMenu(menuName = "Game/Configs/Track Biome Config", fileName = "TrackBiomeConfig_Farm")]
    public sealed class TrackBiomeConfigSO : ScriptableObject
    {
        [Header("Segment Prefabs (straight-only MVP)")]
        public TrackSegmentBehaviour[] segmentPrefabs = default!;

        [Header("Spawn Settings")]
        [Min(1)] public int initialSegments = 8;
        [Min(3)] public int maxAliveSegments = 10;

        [Min(1f)] public float spawnAheadThreshold = 25f;
        [Min(1f)] public float despawnBehindDistance = 40f;

        [Header("Turns (disabled for Subway-style straight)")]
        [Range(0f, 1f)] public float turnChance = 0.0f;
        [Min(1)] public int minStraightsBetweenTurns = 999;

        [Header("Debug")]
        public bool drawGizmos = true;
    }
}

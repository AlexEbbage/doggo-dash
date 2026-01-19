using System.Collections.Generic;
using UnityEngine;
using Game.Presentation.Runtime.World.Obstacles;
using Game.Presentation.Runtime.World.Pickups;
using Game.Presentation.Runtime.World.Track;

namespace Game.Presentation.Runtime.World.ContentPacks
{
    public static class ContentPackLoader
    {
        private const string ResourceConfigName = "ContentPackConfig";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplyContentPacks()
        {
            var config = Resources.Load<ContentPackConfigSO>(ResourceConfigName);
            if (config == null)
            {
                Debug.LogWarning("[ContentPackLoader] No ContentPackConfigSO found in Resources.");
                return;
            }

            if (config.activePacks == null || config.activePacks.Length == 0)
            {
                Debug.LogWarning("[ContentPackLoader] No active content packs configured.");
                return;
            }

            ApplyTrackSegments(config);
            ApplyObstaclePatterns(config);
            ApplyPickupPatterns(config);
        }

        private static void ApplyTrackSegments(ContentPackConfigSO config)
        {
            if (config.biomeTargets == null || config.biomeTargets.Length == 0) return;

            var packSegments = CollectSegments(config.activePacks);
            if (packSegments.Count == 0) return;

            foreach (var biome in config.biomeTargets)
            {
                if (biome == null) continue;
                var combined = new List<TrackSegmentBehaviour>();
                if (biome.segmentPrefabs != null) combined.AddRange(biome.segmentPrefabs);
                MergeDistinct(combined, packSegments);
                biome.segmentPrefabs = combined.ToArray();
            }
        }

        private static void ApplyObstaclePatterns(ContentPackConfigSO config)
        {
            if (config.obstaclePatternTargets == null || config.obstaclePatternTargets.Length == 0) return;

            var packPatterns = CollectObstaclePatterns(config.activePacks);
            if (packPatterns.Count == 0) return;

            foreach (var pattern in config.obstaclePatternTargets)
            {
                if (pattern == null) continue;
                pattern.SetPackPatterns(ExcludeSelf(pattern, packPatterns));
            }
        }

        private static void ApplyPickupPatterns(ContentPackConfigSO config)
        {
            if (config.pickupPatternTargets == null || config.pickupPatternTargets.Length == 0) return;

            var packPatterns = CollectPickupPatterns(config.activePacks);
            if (packPatterns.Count == 0) return;

            foreach (var pattern in config.pickupPatternTargets)
            {
                if (pattern == null) continue;
                pattern.SetPackPatterns(ExcludeSelf(pattern, packPatterns));
            }
        }

        private static List<TrackSegmentBehaviour> CollectSegments(ContentPackSO[] packs)
        {
            var list = new List<TrackSegmentBehaviour>();
            if (packs == null) return list;

            foreach (var pack in packs)
            {
                if (pack == null || pack.segmentPrefabs == null) continue;
                foreach (var segment in pack.segmentPrefabs)
                {
                    if (segment != null && !list.Contains(segment)) list.Add(segment);
                }
            }

            return list;
        }

        private static List<ObstaclePatternSO> CollectObstaclePatterns(ContentPackSO[] packs)
        {
            var list = new List<ObstaclePatternSO>();
            if (packs == null) return list;

            foreach (var pack in packs)
            {
                if (pack == null || pack.obstaclePatterns == null) continue;
                foreach (var pattern in pack.obstaclePatterns)
                {
                    if (pattern != null && !list.Contains(pattern)) list.Add(pattern);
                }
            }

            return list;
        }

        private static List<PickupPatternSO> CollectPickupPatterns(ContentPackSO[] packs)
        {
            var list = new List<PickupPatternSO>();
            if (packs == null) return list;

            foreach (var pack in packs)
            {
                if (pack == null || pack.pickupPatterns == null) continue;
                foreach (var pattern in pack.pickupPatterns)
                {
                    if (pattern != null && !list.Contains(pattern)) list.Add(pattern);
                }
            }

            return list;
        }

        private static void MergeDistinct<T>(List<T> target, List<T> source) where T : Object
        {
            foreach (var item in source)
            {
                if (item != null && !target.Contains(item)) target.Add(item);
            }
        }

        private static ObstaclePatternSO[] ExcludeSelf(ObstaclePatternSO self, List<ObstaclePatternSO> patterns)
        {
            var list = new List<ObstaclePatternSO>(patterns.Count);
            foreach (var pattern in patterns)
            {
                if (pattern != null && pattern != self) list.Add(pattern);
            }
            return list.ToArray();
        }

        private static PickupPatternSO[] ExcludeSelf(PickupPatternSO self, List<PickupPatternSO> patterns)
        {
            var list = new List<PickupPatternSO>(patterns.Count);
            foreach (var pattern in patterns)
            {
                if (pattern != null && pattern != self) list.Add(pattern);
            }
            return list.ToArray();
        }
    }
}

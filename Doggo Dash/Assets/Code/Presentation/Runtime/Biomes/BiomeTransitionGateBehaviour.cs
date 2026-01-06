using System;
using UnityEngine;

namespace Game.Presentation.Runtime.Biomes
{
    [RequireComponent(typeof(Collider))]
    public sealed class BiomeTransitionGateBehaviour : MonoBehaviour
    {
        public BiomeRunRuntimeSwitcherBehaviour switcher = default!;

        public string runnerComponentTypeName = "RunnerControllerBehaviour";
        public string runnerTag = "Player";

        public bool onlyOnce = true;
        public string overrideNextBiomeId;

        private bool _used;
        private Type _runnerType;

        private void Awake()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
            _runnerType = FindTypeByName(runnerComponentTypeName);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_used && onlyOnce) return;
            if (switcher == null || other == null) return;
            if (!IsRunner(other)) return;

            _used = true;

            if (!string.IsNullOrWhiteSpace(overrideNextBiomeId))
                switcher.ApplyBiome(overrideNextBiomeId, persistSelection: false);
            else
                switcher.ApplyNextBiome();
        }

        private bool IsRunner(Collider other)
        {
            if (_runnerType != null)
            {
                var comp = other.GetComponentInParent(_runnerType);
                if (comp != null) return true;
            }

            if (!string.IsNullOrWhiteSpace(runnerTag) && other.CompareTag(runnerTag))
                return true;

            return false;
        }

        private static Type FindTypeByName(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return null;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int a = 0; a < assemblies.Length; a++)
            {
                var types = assemblies[a].GetTypes();
                for (int t = 0; t < types.Length; t++)
                    if (types[t].Name == typeName) return types[t];
            }
            return null;
        }
    }
}

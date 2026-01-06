using System.Collections;
using UnityEngine;

namespace Game.Presentation.Runtime.Pooling
{
    public sealed class ReturnToPoolAfterSeconds : MonoBehaviour, IPoolable
    {
        public float seconds = 1.5f;
        public bool useUnscaledTime = false;
        public PoolRegistryBehaviour registry;

        private Coroutine _co;

        public void OnSpawned()
        {
            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(CoReturn());
        }

        public void OnDespawned()
        {
            if (_co != null) StopCoroutine(_co);
            _co = null;
        }

        private IEnumerator CoReturn()
        {
            if (seconds <= 0f) seconds = 0.01f;

            if (useUnscaledTime)
                yield return new WaitForSecondsRealtime(seconds);
            else
                yield return new WaitForSeconds(seconds);

            if (registry != null)
                registry.Despawn(gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}

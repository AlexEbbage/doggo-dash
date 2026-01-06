using System.Collections.Generic;
using UnityEngine;

namespace Game.Presentation.Runtime.Pooling
{
    public sealed class ObjectPool
    {
        private readonly GameObject _prefab;
        private readonly Transform _inactiveRoot;
        private readonly Stack<GameObject> _stack;

        public ObjectPool(GameObject prefab, Transform inactiveRoot, int prewarm)
        {
            _prefab = prefab;
            _inactiveRoot = inactiveRoot;
            _stack = new Stack<GameObject>(Mathf.Max(4, prewarm));

            for (int i = 0; i < prewarm; i++)
            {
                var go = CreateNew();
                Despawn(go);
            }
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent, PoolKey key)
        {
            GameObject go = _stack.Count > 0 ? _stack.Pop() : CreateNew();

            if (parent != null) go.transform.SetParent(parent, worldPositionStays: false);
            go.transform.SetPositionAndRotation(position, rotation);
            go.SetActive(true);

            var marker = go.GetComponent<PooledInstance>();
            if (marker == null) marker = go.AddComponent<PooledInstance>();
            marker.Init(key);

            var cache = go.GetComponent<PoolableCache>();
            if (cache != null)
            {
                var poolables = cache.Poolables;
                for (int i = 0; i < poolables.Length; i++) poolables[i].OnSpawned();
            }

            return go;
        }

        public void Despawn(GameObject go)
        {
            if (go == null) return;

            var cache = go.GetComponent<PoolableCache>();
            if (cache != null)
            {
                var poolables = cache.Poolables;
                for (int i = 0; i < poolables.Length; i++) poolables[i].OnDespawned();
            }

            go.SetActive(false);
            go.transform.SetParent(_inactiveRoot, worldPositionStays: false);
            _stack.Push(go);
        }

        private GameObject CreateNew()
        {
            var go = Object.Instantiate(_prefab);
            go.name = _prefab.name;

            if (go.GetComponent<PoolableCache>() == null)
                go.AddComponent<PoolableCache>();

            return go;
        }
    }
}

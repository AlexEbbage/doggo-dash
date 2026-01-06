using UnityEngine;

namespace Game.Presentation.Runtime.Pooling
{
    public sealed class PoolableCache : MonoBehaviour
    {
        public IPoolable[] Poolables { get; private set; }

        private void Awake()
        {
            var monos = GetComponentsInChildren<MonoBehaviour>(true);
            int count = 0;
            for (int i = 0; i < monos.Length; i++)
                if (monos[i] is IPoolable) count++;

            Poolables = count == 0 ? System.Array.Empty<IPoolable>() : new IPoolable[count];

            int w = 0;
            for (int i = 0; i < monos.Length; i++)
                if (monos[i] is IPoolable p) Poolables[w++] = p;
        }
    }
}

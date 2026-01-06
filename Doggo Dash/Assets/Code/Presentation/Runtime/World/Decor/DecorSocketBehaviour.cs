using UnityEngine;

namespace Game.Presentation.Runtime.World.Decor
{
    public sealed class DecorSocketBehaviour : MonoBehaviour
    {
        public DecorSocketType socketType = DecorSocketType.GroundEdgeLeft;

        [Range(0f, 1f)]
        public float spawnChance = 0.6f;

        public bool randomYaw = true;
        public Vector2 randomScaleRange = new Vector2(0.9f, 1.1f);

        [Header("Optional lane safety")]
        public bool keepRunnerClear = true;

#if UNITY_EDITOR
        private void OnDrawGizmos() => Gizmos.DrawWireSphere(transform.position, 0.18f);
#endif
    }
}

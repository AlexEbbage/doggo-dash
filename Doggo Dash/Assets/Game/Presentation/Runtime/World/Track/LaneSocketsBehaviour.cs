using UnityEngine;
using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.World.Track
{
    public sealed class LaneSocketsBehaviour : MonoBehaviour
    {
        public Transform left = default!;
        public Transform middle = default!;
        public Transform right = default!;

        public Transform Get(Lane lane)
        {
            return lane switch
            {
                Lane.Left => left,
                Lane.Middle => middle,
                Lane.Right => right,
                _ => middle
            };
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (left != null) Gizmos.DrawWireSphere(left.position, 0.12f);
            if (middle != null) Gizmos.DrawWireSphere(middle.position, 0.12f);
            if (right != null) Gizmos.DrawWireSphere(right.position, 0.12f);
        }
#endif
    }
}

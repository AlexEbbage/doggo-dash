using UnityEngine;

namespace Game.Presentation.Runtime.World.Track
{
    public sealed class TrackSegmentBehaviour : MonoBehaviour
    {
        [Header("Sockets")]
        public Transform startSocket = default!;
        public Transform endSocket = default!;

        [Header("Optional Lane Anchors (for pickups/obstacles later)")]
        public LaneSocketsBehaviour? laneSockets;

        public Transform StartSocket => startSocket;
        public Transform EndSocket => endSocket;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (startSocket != null) Gizmos.DrawSphere(startSocket.position, 0.2f);
            if (endSocket != null) Gizmos.DrawCube(endSocket.position, Vector3.one * 0.25f);
        }
#endif
    }
}

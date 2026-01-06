using UnityEngine;

namespace Game.Presentation.Runtime.World.Track
{
    public sealed class TrackSetChangeNotifierBehaviour : MonoBehaviour
    {
        public event System.Action<TrackSetSO>? OnTrackSetChanged;
        public void NotifyChanged(TrackSetSO set) => OnTrackSetChanged?.Invoke(set);
    }
}

using UnityEngine;

namespace Game.Presentation.Runtime.FX
{
    public sealed class RunFeedbackControllerBehaviour : MonoBehaviour
    {
        [Header("Refs")]
        public AudioPlayerBehaviour audioPlayer = default!;
        public VfxSpawnerBehaviour vfxSpawner = default!;

        [Header("Spawn anchors")]
        public Transform runnerAnchor;

        private void Awake()
        {
            if (audioPlayer == null)
                audioPlayer = FindObjectOfType<AudioPlayerBehaviour>();

            if (vfxSpawner == null)
                vfxSpawner = FindObjectOfType<VfxSpawnerBehaviour>();
        }

        public void PlayPickupTreat(Vector3 at) { audioPlayer?.Play(SfxId.PickupTreat); vfxSpawner?.Spawn(VfxId.PickupTreat, at); }
        public void PlayPickupGem(Vector3 at) { audioPlayer?.Play(SfxId.PickupGem); vfxSpawner?.Spawn(VfxId.PickupGem, at); }
        public void PlayPickupBadFood(Vector3 at) { audioPlayer?.Play(SfxId.PickupBadFood); vfxSpawner?.Spawn(VfxId.PickupBadFood, at); }
        public void PlayPickupZoomies(Vector3 at) { audioPlayer?.Play(SfxId.PickupZoomies); vfxSpawner?.Spawn(VfxId.PickupZoomies, at); }

        public void PlayZoomiesStart()
        {
            audioPlayer?.Play(SfxId.ZoomiesStart);
            if (runnerAnchor != null) vfxSpawner?.Spawn(VfxId.ZoomiesTrailStart, runnerAnchor.position);
        }

        public void PlayZoomiesEnd()
        {
            audioPlayer?.Play(SfxId.ZoomiesEnd);
            if (runnerAnchor != null) vfxSpawner?.Spawn(VfxId.ZoomiesTrailEnd, runnerAnchor.position);
        }

        public void PlayStumble()
        {
            audioPlayer?.Play(SfxId.Stumble);
            if (runnerAnchor != null) vfxSpawner?.Spawn(VfxId.StumbleBurst, runnerAnchor.position);
        }

        public void PlayGameOver()
        {
            audioPlayer?.Play(SfxId.GameOver);
            if (runnerAnchor != null) vfxSpawner?.Spawn(VfxId.GameOverBurst, runnerAnchor.position);
        }
    }
}

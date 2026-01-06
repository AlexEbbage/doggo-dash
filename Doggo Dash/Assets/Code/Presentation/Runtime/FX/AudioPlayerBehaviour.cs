using UnityEngine;

namespace Game.Presentation.Runtime.FX
{
    public sealed class AudioPlayerBehaviour : MonoBehaviour
    {
        [Header("Catalog")]
        public AudioCatalogSO catalog = default!;

        [Header("Sources")]
        public AudioSource sfxSource = default!;
        public AudioSource ambienceSource;

        [Header("Master")]
        [Range(0f, 1f)] public float masterSfx = 1f;
        [Range(0f, 1f)] public float masterAmbience = 1f;

        private void Awake()
        {
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.loop = false;
            }

            if (ambienceSource != null)
            {
                ambienceSource.playOnAwake = false;
                ambienceSource.loop = true;
            }
        }

        public void Play(SfxId id)
        {
            if (catalog == null || sfxSource == null) return;
            if (!catalog.TryGet(id, out var clip, out var vol, out var pitch)) return;

            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clip, vol * masterSfx);
        }

        public void PlayAmbience(SfxId id)
        {
            if (ambienceSource == null || catalog == null) return;
            if (!catalog.TryGet(id, out var clip, out var vol, out var pitch)) return;

            ambienceSource.clip = clip;
            ambienceSource.volume = vol * masterAmbience;
            ambienceSource.pitch = pitch;
            ambienceSource.loop = true;
            ambienceSource.Play();
        }

        public void StopAmbience()
        {
            if (ambienceSource == null) return;
            ambienceSource.Stop();
        }
    }
}

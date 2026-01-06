using UnityEngine;

namespace Game.Presentation.Runtime.FX
{
    [CreateAssetMenu(menuName = "Game/Configs/Audio Catalog", fileName = "AudioCatalog")]
    public sealed class AudioCatalogSO : ScriptableObject
    {
        [System.Serializable]
        public sealed class Entry
        {
            public SfxId id;
            public AudioClip clip = default!;
            [Range(0f, 1f)] public float volume = 1f;
            [Range(0.5f, 1.5f)] public float pitch = 1f;
        }

        public Entry[] entries = default!;

        public bool TryGet(SfxId id, out AudioClip clip, out float volume, out float pitch)
        {
            if (entries != null)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    var e = entries[i];
                    if (e != null && e.id == id && e.clip != null)
                    {
                        clip = e.clip;
                        volume = e.volume;
                        pitch = e.pitch;
                        return true;
                    }
                }
            }

            clip = null!;
            volume = 1f;
            pitch = 1f;
            return false;
        }
    }
}

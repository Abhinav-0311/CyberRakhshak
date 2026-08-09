using UnityEngine;

namespace CyberRakshak.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class SceneMusicPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip music;
        [Range(0f, 1f)] [SerializeField] private float volume = 0.58f;

        private void Awake()
        {
            var source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;
            source.mute = false;
            source.bypassEffects = true;
            source.bypassListenerEffects = true;
            source.bypassReverbZones = true;
            source.priority = 0;
            source.volume = volume;
            source.clip = music;

            if (music != null)
                source.Play();
        }
    }
}



using UnityEngine;

namespace CyberRakshak.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class SceneMusicPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip music;
        [Range(0f, 1f)] [SerializeField] private float volume = 0.58f;

        private AudioSource source;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;
            source.mute = false;
            source.bypassEffects = true;
            source.bypassListenerEffects = true;
            source.bypassReverbZones = true;
            source.priority = 0;
            source.clip = music;

            UpdateVolume();

            if (music != null)
                source.Play();
        }

        private void OnEnable()
        {
            SettingsOverlayController.OnSettingsChanged += UpdateVolume;
        }

        private void OnDisable()
        {
            SettingsOverlayController.OnSettingsChanged -= UpdateVolume;
        }

        private void UpdateVolume()
        {
            source.volume = volume * PlayerPrefs.GetFloat("CyberRakshak.Music", 1f);
        }
    }
}



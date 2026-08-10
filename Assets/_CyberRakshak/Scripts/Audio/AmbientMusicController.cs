using UnityEngine;

namespace CyberRakshak.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class AmbientMusicController : MonoBehaviour
    {
        [SerializeField] private AudioClip ambientLoop;
        [SerializeField, Range(0f, 1f)] private float volume = 0.35f;
        [SerializeField] private bool playOnStart = true;

        private AudioSource source;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            
            UpdateVolume();

            if (ambientLoop != null)
            {
                source.clip = ambientLoop;
            }
        }

        private void OnEnable()
        {
            Runtime.SettingsOverlayController.OnSettingsChanged += UpdateVolume;
        }

        private void OnDisable()
        {
            Runtime.SettingsOverlayController.OnSettingsChanged -= UpdateVolume;
        }

        private void UpdateVolume()
        {
            source.volume = volume * PlayerPrefs.GetFloat("CyberRakshak.Music", 1f);
        }

        private void Start()
        {
            if (playOnStart && source.clip != null)
            {
                source.Play();
            }
        }

        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            source.volume = volume;
        }
    }
}


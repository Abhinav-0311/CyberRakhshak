using UnityEngine;
using UnityEngine.UI;

namespace CyberRakshak.Runtime
{
    public sealed class SettingsOverlayController : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider sensitivitySlider;

        private const string MusicKey = "CyberRakshak.Music";
        private const string SfxKey = "CyberRakshak.Sfx";
        private const string SensitivityKey = "CyberRakshak.Sensitivity";

        private void Awake()
        {
            musicSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(MusicKey, .65f));
            sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(SfxKey, .70f));
            sensitivitySlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(SensitivityKey, .60f));
            ApplyMusic(musicSlider.value);
        }

        public void Open()
        {
            panel.SetActive(true);
        }

        public void Close()
        {
            panel.SetActive(false);
        }

        public void SetMusic(float value)
        {
            ApplyMusic(value);
        }

        public void SetSfx(float value)
        {
            PlayerPrefs.SetFloat(SfxKey, value);
            PlayerPrefs.Save();
        }

        public void SetSensitivity(float value)
        {
            PlayerPrefs.SetFloat(SensitivityKey, value);
            PlayerPrefs.Save();
        }

        private static void ApplyMusic(float value)
        {
            PlayerPrefs.SetFloat(MusicKey, value);
            PlayerPrefs.Save();
            AudioListener.volume = Mathf.Clamp01(value);
        }
    }
}
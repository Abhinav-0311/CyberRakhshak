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

        public static event System.Action OnSettingsChanged;

        private void Awake()
        {
            musicSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(MusicKey, 1f));
            sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(SfxKey, 1f));
            sensitivitySlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(SensitivityKey, 1f));

            musicSlider.onValueChanged.AddListener(SetMusic);
            sfxSlider.onValueChanged.AddListener(SetSfx);
            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
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
            PlayerPrefs.SetFloat(MusicKey, value);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }

        public void SetSfx(float value)
        {
            PlayerPrefs.SetFloat(SfxKey, value);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }

        public void SetSensitivity(float value)
        {
            PlayerPrefs.SetFloat(SensitivityKey, value);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }
    }
}
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
        private RectTransform backHitArea;

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

            // The visual Settings card has overlapping decorative images. Keep
            // the transparent Back hit-area above them and give it one reliable
            // close callback.
            foreach (var button in GetComponentsInChildren<Button>(true))
            {
                if (button.name != "SettingsBackHit")
                {
                    continue;
                }

                button.transform.SetAsLastSibling();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Close);
                backHitArea = button.transform as RectTransform;
                break;
            }
        }

        private void Update()
        {
            // Some decorative images in the imported card can swallow a UI
            // raycast. The screen-rectangle check keeps the visible Back area
            // clickable even in that layout.
            if (panel.activeSelf && Input.GetMouseButtonUp(0) &&
                (backHitArea != null && RectTransformUtility.RectangleContainsScreenPoint(backHitArea, Input.mousePosition, null) ||
                 Input.mousePosition.y < Screen.height * 0.32f))
            {
                Close();
            }
        }

        public void Open()
        {
            panel.SetActive(true);
        }

        public void Close()
        {
            // The Level 1 Back button is also a Unity Button callback.  Let the
            // pause controller close the panel so it can restore the paused menu.
            var pauseController = FindFirstObjectByType<GameplayPauseController>();
            if (pauseController != null)
            {
                pauseController.CloseSettings();
                return;
            }

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

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyberRakshak.Runtime
{
    public sealed class CyberRakshakMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject settingsMenu;
        [SerializeField] private Button mainStartButton;
        [SerializeField] private Button pauseResumeButton;
        [SerializeField] private Button settingsBackButton;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private ThirdPersonCameraController cameraController;

        private bool isMainMenuOpen = true;
        private bool isPauseMenuOpen;
        private bool isSettingsOpen;

        private void Awake()
        {
            musicSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("MenuMusic", 0.8f));
            sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("MenuSfx", 0.7f));
            sensitivitySlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("MenuSensitivity", 0.62f));
            ApplySettings();
            OpenMainMenu();
        }

        private void Start()
        {
            OpenMainMenu();
        }


        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape))
            {
                return;
            }

            if (isSettingsOpen)
            {
                CloseSettings();
            }
            else if (isPauseMenuOpen)
            {
                ResumeTraining();
            }
            else if (!isMainMenuOpen)
            {
                OpenPauseMenu();
            }
        }

        public void StartTraining()
        {
            isMainMenuOpen = false;
            mainMenu.SetActive(false);
            ResumeWorld();
        }

        public void ContinueTraining()
        {
            StartTraining();
        }

        public void OpenMainSettings()
        {
            isMainMenuOpen = true;
            settingsMenu.SetActive(true);
            isSettingsOpen = true;
            Select(settingsBackButton);
        }

        public void OpenPauseMenu()
        {
            if (isMainMenuOpen)
            {
                return;
            }

            isPauseMenuOpen = true;
            pauseMenu.SetActive(true);
            PauseWorld();
            Select(pauseResumeButton);
        }

        public void ResumeTraining()
        {
            isPauseMenuOpen = false;
            pauseMenu.SetActive(false);
            settingsMenu.SetActive(false);
            isSettingsOpen = false;
            ResumeWorld();
        }

        public void OpenPauseSettings()
        {
            settingsMenu.SetActive(true);
            isSettingsOpen = true;
            Select(settingsBackButton);
        }

        public void CloseSettings()
        {
            settingsMenu.SetActive(false);
            isSettingsOpen = false;
            Select(isPauseMenuOpen ? pauseResumeButton : mainStartButton);
        }

        public void SetMusic(float value)
        {
            PlayerPrefs.SetFloat("MenuMusic", value);
            AudioListener.volume = Mathf.Clamp01(value);
        }

        public void SetSfx(float value)
        {
            PlayerPrefs.SetFloat("MenuSfx", value);
        }

        public void SetCameraSensitivity(float value)
        {
            PlayerPrefs.SetFloat("MenuSensitivity", value);
            if (cameraController != null)
            {
                cameraController.SetSensitivity(Mathf.Lerp(90f, 260f, value));
            }
        }

        public void ExitGame()
        {
            Application.Quit();
        }

        public void OpenMainMenu()
        {
            isMainMenuOpen = true;
            isPauseMenuOpen = false;
            isSettingsOpen = false;
            mainMenu.SetActive(true);
            pauseMenu.SetActive(false);
            settingsMenu.SetActive(false);
            PauseWorld();
            Select(mainStartButton);
        }

        private void PauseWorld()
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ResumeWorld()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void ApplySettings()
        {
            SetMusic(musicSlider.value);
            SetSfx(sfxSlider.value);
            SetCameraSensitivity(sensitivitySlider.value);
        }

        private static void Select(Button button)
        {
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }
}
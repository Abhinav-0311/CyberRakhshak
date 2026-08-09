using UnityEngine;
using UnityEngine.SceneManagement;

namespace CyberRakshak.Runtime
{
    public sealed class GameplayPauseController : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private PauseBackdropController backdrop;

        private bool paused;

        private void Awake()
        {
            pausePanel.SetActive(false);
            settingsPanel.SetActive(false);
            backdrop.Hide();
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape))
                return;

            if (settingsPanel.activeSelf)
                CloseSettings();
            else if (paused)
                Resume();
            else
                Pause();
        }

        public void Pause()
        {
            paused = true;
            backdrop.Capture();
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Resume()
        {
            paused = false;
            pausePanel.SetActive(false);
            settingsPanel.SetActive(false);
            backdrop.Hide();
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void OpenSettings()
        {
            pausePanel.SetActive(false);
            settingsPanel.SetActive(true);
        }

        public void CloseSettings()
        {
            settingsPanel.SetActive(false);
            if (paused)
                pausePanel.SetActive(true);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}

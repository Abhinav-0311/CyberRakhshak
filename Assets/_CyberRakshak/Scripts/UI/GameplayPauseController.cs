using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;

namespace CyberRakshak.Runtime
{
    public sealed class GameplayPauseController : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private PauseBackdropController backdrop;

        private bool paused;
        private bool dialogueBlocking;
        private bool gameplayInputDisabled;
        private bool cursorLockedBeforePause = true;

        public bool IsPaused => paused;

        public void SetDialogueBlocking(bool blocked)
        {
            dialogueBlocking = blocked;
            ApplyGameplayState();
        }

        private void ApplyGameplayState()
        {
            bool blocked = paused || dialogueBlocking;
            SetGameplayInputEnabled(!blocked);
            Time.timeScale = blocked ? 0f : 1f;
            Cursor.lockState = blocked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = blocked;
        }

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

        private void OnDisable()
        {
            if (!paused && !dialogueBlocking) return;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Pause()
        {
            paused = true;
            backdrop.Capture();
            pausePanel.SetActive(true);
            ApplyGameplayState();
        }

        public void Resume()
        {
            paused = false;
            pausePanel.SetActive(false);
            settingsPanel.SetActive(false);
            backdrop.Hide();
            ApplyGameplayState();
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

        private void SetGameplayInputEnabled(bool enabled)
        {
            if (gameplayInputDisabled == !enabled)
                return;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                return;
            }
            gameplayInputDisabled = !enabled;

            Component playerInput = player.GetComponent("PlayerInput");
            playerInput?.GetType().GetMethod(enabled ? "ActivateInput" : "DeactivateInput")?.Invoke(playerInput, null);

            Component starterInputs = player.GetComponent("StarterAssetsInputs");
            if (starterInputs == null)
            {
                return;
            }

            FieldInfo cursorLocked = starterInputs.GetType().GetField("cursorLocked");
            if (!enabled)
            {
                if (cursorLocked != null)
                {
                    cursorLockedBeforePause = (bool)cursorLocked.GetValue(starterInputs);
                    cursorLocked.SetValue(starterInputs, false);
                }

                starterInputs.GetType().GetField("move")?.SetValue(starterInputs, Vector2.zero);
                starterInputs.GetType().GetField("look")?.SetValue(starterInputs, Vector2.zero);
                starterInputs.GetType().GetField("jump")?.SetValue(starterInputs, false);
            }
            else if (cursorLocked != null)
            {
                cursorLocked.SetValue(starterInputs, cursorLockedBeforePause);
            }
        }
    }
}

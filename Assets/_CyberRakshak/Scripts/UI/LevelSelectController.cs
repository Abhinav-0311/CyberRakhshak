using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyberRakshak.Runtime
{
    public sealed class LevelSelectController : MonoBehaviour
    {
        [SerializeField] private Button tutorialButton;
        [SerializeField] private Text tutorialStatus;
        [SerializeField] private Button levelOneButton;
        [SerializeField] private Button levelTwoButton;
        [SerializeField] private Text levelOneStatus;
        [SerializeField] private Text levelTwoStatus;

        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 1f;
            Refresh();
        }

        private void Start()
        {
            if (EventSystem.current != null && tutorialButton != null)
                EventSystem.current.SetSelectedGameObject(tutorialButton.gameObject);
        }

        public void Refresh()
        {
            ApplyCard(tutorialButton, tutorialStatus, true, GameProgression.HasCompletedTutorial);
            // Level 1 is the reviewable core module and should always be reachable from Level Select.
            ApplyCard(levelOneButton, levelOneStatus, true, GameProgression.HasCompletedLevelOne);
            // The maze is reviewable; phishing tasks and completion progression are not authored yet.
            ApplyCard(levelTwoButton, levelTwoStatus, true, false);
        }

        private static void ApplyCard(Button button, Text status, bool available, bool complete)
        {
            if (button != null)
                button.interactable = available;

            if (status == null)
                return;

            status.text = available ? (complete ? "REPLAY" : "PLAY") : "COMING SOON";
            status.color = available ? new Color(.32f, .94f, 1f) : new Color(.62f, .70f, .80f);
        }
    }
}

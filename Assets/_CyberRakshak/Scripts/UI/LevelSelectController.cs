using UnityEngine;
using UnityEngine.UI;

namespace CyberRakshak.Runtime
{
    public sealed class LevelSelectController : MonoBehaviour
    {
        [SerializeField] private Button levelOneButton;
        [SerializeField] private Button levelTwoButton;
        [SerializeField] private Text levelOneStatus;
        [SerializeField] private Text levelTwoStatus;

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            // Level 1 is the reviewable core module and should always be reachable from Level Select.
            ApplyCard(levelOneButton, levelOneStatus, true, GameProgression.HasCompletedLevelOne);
            ApplyCard(levelTwoButton, levelTwoStatus, GameProgression.IsLevelTwoUnlocked, false);
        }

        private static void ApplyCard(Button button, Text status, bool unlocked, bool complete)
        {
            if (button != null)
                button.interactable = unlocked;

            if (status == null)
                return;

            status.text = complete ? "REPLAY" : unlocked ? "PLAY" : "LOCKED";
            status.color = unlocked ? new Color(.32f, .94f, 1f, 1f) : new Color(.45f, .56f, .68f, .85f);
        }
    }
}

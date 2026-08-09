using UnityEngine;
using UnityEngine.UI;

namespace CyberRakshak.Runtime
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private GameObject settingsPanel;

        private void Awake()
        {
            settingsPanel.SetActive(false);
            continueButton.interactable = GameProgression.HasCompletedLevelOne;
        }

        private void Update()
        {
            if (settingsPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
                CloseSettings();
        }

        public void OpenSettings()
        {
            settingsPanel.SetActive(true);
        }

        public void CloseSettings()
        {
            settingsPanel.SetActive(false);
        }
    }
}
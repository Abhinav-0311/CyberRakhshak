using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CyberRakshak.Runtime
{
    public sealed class SceneNavigator : MonoBehaviour
    {
        [SerializeField] private string levelSelectScene = "LevelSelect";
        [SerializeField] private string mainMenuScene = "MainMenu";

        public void StartTraining() => SceneManager.LoadScene(levelSelectScene);

        public void ContinueTraining()
        {
            SceneManager.LoadScene(GameProgression.LatestUnlockedScene);
        }

        public void LoadTutorial() => SceneManager.LoadScene("Game_Tutorial");

        public void LoadLevelOne()
        {
            if (GameProgression.IsLevelOneUnlocked)
                SceneManager.LoadScene("Game_Level01");
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuScene);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

using UnityEngine;

namespace CyberRakshak.Runtime
{
    public static class GameProgression
    {
        private const string TutorialCompleteKey = "CyberRakshak.TutorialComplete";
        private const string LevelOneCompleteKey = "CyberRakshak.Level1Complete";
        private const string LatestSceneKey = "CyberRakshak.LatestUnlockedScene";

        public static bool HasCompletedTutorial => PlayerPrefs.GetInt(TutorialCompleteKey, 0) == 1;
        public static bool IsLevelOneUnlocked => HasCompletedTutorial;
        public static bool HasCompletedLevelOne => PlayerPrefs.GetInt(LevelOneCompleteKey, 0) == 1;
        public static bool IsLevelTwoUnlocked => HasCompletedLevelOne;

        public static string LatestUnlockedScene
        {
            get => PlayerPrefs.GetString(LatestSceneKey, "Game_Tutorial");
            private set
            {
                PlayerPrefs.SetString(LatestSceneKey, value);
                PlayerPrefs.Save();
            }
        }

        public static void CompleteTutorial()
        {
            PlayerPrefs.SetInt(TutorialCompleteKey, 1);
            LatestUnlockedScene = "Game_Level01";
            PlayerPrefs.Save();
        }

        public static void CompleteLevelOne(string nextScene)
        {
            PlayerPrefs.SetInt(LevelOneCompleteKey, 1);
            LatestUnlockedScene = nextScene;
            PlayerPrefs.Save();
        }
    }
}

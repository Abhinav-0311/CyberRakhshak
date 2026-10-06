using System.Collections;
using CyberRakshak.PATCH;
using CyberRakshak.Platformer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CyberRakshak.Runtime
{
    /// <summary>
    /// Completes one playable training module when the player enters its authored exit volume.
    /// The trigger is configured by the level bootstrap so completion is never inferred from a UI click.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public sealed class ModuleCompletionTrigger : MonoBehaviour
    {
        [SerializeField] private bool completesTutorial;
        [SerializeField] private string nextScene = "LevelSelect";
        [SerializeField, Min(0f)] private float transitionDelay = 2f;
        [SerializeField] private CoinManager mazeCoins;

        private bool completed;
        private bool IsPhishingMaze => gameObject.scene.name == "Game_Level02";

        public void Configure(bool tutorial, string destination, float delay)
        {
            completesTutorial = tutorial;
            nextScene = destination;
            transitionDelay = Mathf.Max(0f, delay);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (completed || (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player")))
            {
                return;
            }

            if (IsPhishingMaze && (mazeCoins == null || !mazeCoins.CanExit))
            {
                if (mazeCoins != null)
                    PatchDialoguePresenter.Ensure().Show("PATCH",
                        $"Collect {mazeCoins.RequiredCoins} coins to leave. You have {mazeCoins.CollectedCoins}.", 2f);
                return;
            }

            completed = true;
            if (completesTutorial)
            {
                GameProgression.CompleteTutorial();
            }
            else if (!IsPhishingMaze)
            {
                GameProgression.CompleteLevelOne(nextScene);
            }

            StartCoroutine(CompleteRoutine());
        }

        private IEnumerator CompleteRoutine()
        {
            PatchDialoguePresenter.Ensure().Show(
                "PATCH",
                IsPhishingMaze ? "Phishing Maze cleared! Remember: check the actual link, not just its label." :
                    completesTutorial ? "Training complete. Next: Firewall Foundations." : "Firewall Foundations complete. Training data secured.",
                transitionDelay);

            if (IsPhishingMaze) PlatformerSfx.PlayCoinChime(true);
            ShowCompletionOverlay(IsPhishingMaze ? "PHISHING MAZE COMPLETE" : completesTutorial ? "TUTORIAL COMPLETE" : "LEVEL 1 COMPLETE");
            yield return new WaitForSecondsRealtime(transitionDelay);
            SceneManager.LoadScene(nextScene);
        }

        private static void ShowCompletionOverlay(string message)
        {
            GameObject canvasObject = new GameObject("ModuleCompleteUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            GameObject textObject = new GameObject("CompletionMessage", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(canvasObject.transform, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(.5f, .5f);
            rect.anchorMax = new Vector2(.5f, .5f);
            rect.sizeDelta = new Vector2(900f, 170f);

            Text text = textObject.GetComponent<Text>();
            text.text = message + "\nRETURNING TO MODULE SELECT";
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 38;
            text.color = new Color(.32f, .94f, 1f, 1f);
        }
    }
}

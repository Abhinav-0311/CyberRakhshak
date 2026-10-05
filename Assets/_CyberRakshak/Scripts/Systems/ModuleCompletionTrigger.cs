using System.Collections;
using CyberRakshak.PATCH;
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

        private bool completed;
        private bool IsMazePrototype => gameObject.scene.name == "Game_Level02";

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

            completed = true;
            if (completesTutorial)
            {
                GameProgression.CompleteTutorial();
            }
            else if (!IsMazePrototype)
            {
                GameProgression.CompleteLevelOne(nextScene);
            }

            StartCoroutine(CompleteRoutine());
        }

        private IEnumerator CompleteRoutine()
        {
            PatchDialoguePresenter.Ensure().Show(
                "PATCH",
                IsMazePrototype ? "Maze exit reached. Returning to module select." :
                    completesTutorial ? "Training complete. Next: Firewall Foundations." : "Firewall Foundations complete. Training data secured.",
                transitionDelay);

            ShowCompletionOverlay(IsMazePrototype ? "MAZE EXIT REACHED" : completesTutorial ? "TUTORIAL COMPLETE" : "LEVEL 1 COMPLETE");
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

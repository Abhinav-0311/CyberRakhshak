using CyberRakshak.PATCH;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CyberRakshak.Prototype
{
    [RequireComponent(typeof(Collider))]
    public sealed class PrototypeLevelGoal : MonoBehaviour
    {
        [SerializeField] private string nextSceneName;
        [SerializeField] private PrototypeRunStats runStats;
        [SerializeField] private PatchDialoguePresenter patchPresenter;
        [SerializeField] private bool loadNextScene = true;

        private bool completed;

        private void Reset()
        {
            Collider triggerCollider = GetComponent<Collider>();
            triggerCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (completed || !other.CompareTag("Player"))
            {
                return;
            }

            completed = true;

            if (runStats == null)
            {
                runStats = FindFirstObjectByType<PrototypeRunStats>();
            }

            if (patchPresenter == null)
            {
                patchPresenter = FindFirstObjectByType<PatchDialoguePresenter>();
            }

            string rating = runStats != null ? runStats.GetPrototypeRating() : "Prototype Complete";
            patchPresenter?.Show("PATCH", $"Level complete. Prototype rating: {rating}.", 5f);

            if (loadNextScene && !string.IsNullOrWhiteSpace(nextSceneName))
            {
                Invoke(nameof(LoadNextScene), 5f);
            }
        }

        private void LoadNextScene()
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}


using CyberRakshak.PATCH;
using UnityEngine;

namespace CyberRakshak.Prototype
{
    [RequireComponent(typeof(Collider))]
    public sealed class PhishingChoiceTrigger : MonoBehaviour
    {
        [SerializeField] private bool isSafeRoute;
        [SerializeField] private PrototypeRunStats runStats;
        [SerializeField] private PatchDialoguePresenter patchPresenter;
        [SerializeField, TextArea(2, 4)] private string safeMessage =
            "Good check. The safer path did not pressure you, hide the destination, or ask for credentials suddenly.";
        [SerializeField, TextArea(2, 4)] private string phishingMessage =
            "Phishing warning. Urgency plus a suspicious destination is a common trap. Check the URL before trusting it.";

        private bool hasTriggered;

        private void Reset()
        {
            Collider triggerCollider = GetComponent<Collider>();
            triggerCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered || !other.CompareTag("Player"))
            {
                return;
            }

            if (runStats == null)
            {
                runStats = FindFirstObjectByType<PrototypeRunStats>();
            }

            if (patchPresenter == null)
            {
                patchPresenter = FindFirstObjectByType<PatchDialoguePresenter>();
            }

            if (isSafeRoute)
            {
                patchPresenter?.Show("PATCH", safeMessage, 4f);
            }
            else
            {
                runStats?.RecordWrongInteraction();
                patchPresenter?.Show("PATCH", phishingMessage, 5f);
            }

            hasTriggered = true;
        }
    }
}


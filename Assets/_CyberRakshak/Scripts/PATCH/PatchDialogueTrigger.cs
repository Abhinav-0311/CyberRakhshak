using UnityEngine;

namespace CyberRakshak.PATCH
{
    [RequireComponent(typeof(Collider))]
    public sealed class PatchDialogueTrigger : MonoBehaviour
    {
        [SerializeField] private PatchDialoguePresenter presenter;
        [SerializeField] private PatchDialogueLine line;
        [SerializeField] private bool playOnce = true;
        [SerializeField] private string playerTag = "Player";

        private bool hasPlayed;

        private void Reset()
        {
            Collider triggerCollider = GetComponent<Collider>();
            triggerCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasPlayed && playOnce)
            {
                return;
            }

            if (!other.CompareTag(playerTag))
            {
                return;
            }

            if (presenter == null)
            {
                presenter = FindFirstObjectByType<PatchDialoguePresenter>();
            }

            presenter?.Show(line);
            hasPlayed = true;
        }
    }
}


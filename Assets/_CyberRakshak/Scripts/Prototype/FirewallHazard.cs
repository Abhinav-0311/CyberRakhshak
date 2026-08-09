using CyberRakshak.PATCH;
using UnityEngine;

namespace CyberRakshak.Prototype
{
    [RequireComponent(typeof(Collider))]
    public sealed class FirewallHazard : MonoBehaviour
    {
        [SerializeField] private PrototypeRunStats runStats;
        [SerializeField] private CheckpointRespawn respawn;
        [SerializeField] private PatchDialoguePresenter patchPresenter;
        [SerializeField, TextArea(2, 4)] private string patchWarning =
            "Firewall hit. In real systems, firewalls block unsafe traffic before it reaches protected data.";

        private void Reset()
        {
            Collider triggerCollider = GetComponent<Collider>();
            triggerCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            if (runStats == null)
            {
                runStats = FindFirstObjectByType<PrototypeRunStats>();
            }

            if (respawn == null)
            {
                respawn = FindFirstObjectByType<CheckpointRespawn>();
            }

            if (patchPresenter == null)
            {
                patchPresenter = FindFirstObjectByType<PatchDialoguePresenter>();
            }

            runStats?.RecordFirewallHit();
            patchPresenter?.Show("PATCH", patchWarning, 4f);
            respawn?.Respawn();
        }
    }
}


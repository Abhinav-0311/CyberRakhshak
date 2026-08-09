using UnityEngine;
using System.Collections;

namespace CyberRakshak
{
    public class FirewallHazard : MonoBehaviour
    {
        [Tooltip("The time in seconds it takes to extinguish the fire after water hits it.")]
        public float extinguishDelay = 2.0f;

        [Tooltip("The particle systems to stop when extinguished.")]
        public ParticleSystem[] fireParticles;
        
        private bool isExtinguished = false;
        private Collider hazardCollider;

        private void Start()
        {
            hazardCollider = GetComponent<Collider>();
            if (fireParticles == null || fireParticles.Length == 0)
            {
                fireParticles = GetComponentsInChildren<ParticleSystem>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isExtinguished) return;

            // Check if the object is water
            if (other.GetComponent<WaterObject>() != null || other.CompareTag("Water"))
            {
                StartCoroutine(ExtinguishRoutine());
            }
        }

        private IEnumerator ExtinguishRoutine()
        {
            isExtinguished = true;
            Debug.Log("Firewall is being extinguished...");

            yield return new WaitForSeconds(extinguishDelay);

            // Stop emitting fire
            foreach (var ps in fireParticles)
            {
                if (ps != null)
                {
                    var emission = ps.emission;
                    emission.enabled = false;
                }
            }

            // Disable the collider so the player can pass through
            if (hazardCollider != null)
            {
                hazardCollider.enabled = false;
            }

            Debug.Log("Firewall extinguished. Path is clear.");
        }
    }
}

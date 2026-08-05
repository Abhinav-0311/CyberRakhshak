using UnityEngine;

namespace CyberRakshak
{
    public class EnemyStompTrigger : MonoBehaviour
    {
        public StompableEnemy parentEnemy;
        public float bounceForce = 10f;

        private void Start()
        {
            if (parentEnemy == null)
            {
                parentEnemy = GetComponentInParent<StompableEnemy>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Only trigger if the player jumps on head. We assume the colliding object has a CharacterController or Rigidbody
            if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null || other.attachedRigidbody != null)
            {
                // Ensure player is moving downwards relative to the enemy.
                // For simplicity, we just trigger the stomp and apply upward force.
                parentEnemy.Die();

                // Apply bounce to player
                CharacterController cc = other.GetComponent<CharacterController>();
                if (cc != null)
                {
                    // CharacterController requires script-based velocity manipulation in the player's script.
                    // This is a simplified stand-in.
                    Debug.Log("Apply upward bounce to Player's CharacterController.");
                }

                Rigidbody rb = other.attachedRigidbody;
                if (rb != null)
                {
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, bounceForce, rb.linearVelocity.z);
                }
            }
        }
    }
}

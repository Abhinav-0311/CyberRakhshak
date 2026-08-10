using UnityEngine;

namespace CyberRakshak
{
    public class StompableEnemy : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 2f;
        public float patrolDistance = 5f;

        [Header("Animation Settings")]
        public string runStateName = "Run";
        public string dieStateName = "Die"; // Note: SpaceMan might not have a Die state, but we'll try to play it anyway.

        private Vector3 startPos;
        private bool movingRight = true;
        private bool isDead = false;

        private Animator animator;

        private void Start()
        {
            startPos = transform.position;
            animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            if (isDead) return;
            Patrol();
        }

        private void Patrol()
        {
            if (movingRight)
            {
                transform.Translate(Vector3.right * moveSpeed * Time.deltaTime, Space.World);
                transform.rotation = Quaternion.Euler(0, 90, 0);

                if (Vector3.Distance(startPos, transform.position) > patrolDistance)
                {
                    movingRight = false;
                }
            }
            else
            {
                transform.Translate(Vector3.left * moveSpeed * Time.deltaTime, Space.World);
                transform.rotation = Quaternion.Euler(0, -90, 0);

                if (Vector3.Distance(startPos, transform.position) > patrolDistance)
                {
                    movingRight = true;
                }
            }

            if (animator != null && !string.IsNullOrEmpty(runStateName))
            {
                // Play walking/running animation state directly
                animator.Play(runStateName);
            }
        }

        public void Die()
        {
            if (isDead) return;
            isDead = true;

            Debug.Log("Enemy Stomped!");
            
            if (animator != null && !string.IsNullOrEmpty(dieStateName))
            {
                // Try to play a death animation or fall over.
                animator.Play(dieStateName);
            }
            
            // Disable colliders so it no longer blocks or hurts player
            foreach (var col in GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }

            // Destroy object after 2 seconds to let death animation play
            Destroy(gameObject, 2f);
        }
    }
}

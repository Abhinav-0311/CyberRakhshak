using CyberRakshak.Runtime;
using UnityEngine;

namespace CyberRakshak.Platformer
{
    /// <summary>A compact patrol enemy: stomp its head from above, or take firewall contact damage.</summary>
    [RequireComponent(typeof(CapsuleCollider))]
    public sealed class PlatformerEnemy : MonoBehaviour
    {
        [SerializeField] private float patrolSpeed = 1.25f;
        [SerializeField] private float patrolDistance = 2.25f;
        [SerializeField] private float stompHeight = 1.05f;
        [SerializeField] private int contactDamage = 34;
        [SerializeField] private float bounceVelocity = 10f;

        private Vector3 spawnPosition;
        private Vector3 patrolAxis;
        private bool movingForward = true;
        private bool defeated;

        private void Awake()
        {
            spawnPosition = transform.position;
            patrolAxis = transform.forward.sqrMagnitude > 0.001f ? transform.forward.normalized : Vector3.forward;
        }

        private void Update()
        {
            if (defeated)
            {
                return;
            }

            float direction = movingForward ? 1f : -1f;
            transform.position += patrolAxis * (direction * patrolSpeed * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(patrolAxis * direction, Vector3.up);

            if (Vector3.Distance(spawnPosition, transform.position) >= patrolDistance)
            {
                movingForward = !movingForward;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (defeated)
            {
                return;
            }

            PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
            if (health == null)
            {
                return;
            }

            // A stomp requires the player's collider bottom to be above the enemy's head zone.
            if (other.bounds.min.y >= transform.position.y + stompHeight)
            {
                Defeat(other.transform.root);
                return;
            }

            health.TakeHit(contactDamage);
        }

        private void Defeat(Transform playerRoot)
        {
            if (defeated)
            {
                return;
            }

            defeated = true;
            PlatformerSfx.PlayBlop(transform.position);

            AdiPrototypeController controller = playerRoot.GetComponent<AdiPrototypeController>();
            controller?.Bounce(bounceVelocity);

            foreach (Collider collider in GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }

            transform.localScale *= 0.25f;
            Destroy(gameObject, 0.35f);
        }
    }
}

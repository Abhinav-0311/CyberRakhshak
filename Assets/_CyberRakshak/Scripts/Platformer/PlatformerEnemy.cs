using CyberRakshak.Runtime;
using StarterAssets;
using UnityEngine;
namespace CyberRakshak.Platformer
{
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
    public sealed class PlatformerEnemy : MonoBehaviour
    {
        [SerializeField] float roamSpeed = 2.25f, roamRadius = 6f, stompFootClearance = .2f, bounceVelocity = 10f;
        [SerializeField] int contactDamage = 34;
        Vector3 spawnPosition, roamTarget; Rigidbody body; CapsuleCollider hitbox; float nextTargetTime; bool defeated;

        public bool IsDefeated => defeated;
        void Awake()
        {
            hitbox = GetComponent<CapsuleCollider>();
            spawnPosition = ClampToGuardCorridor(transform.position);
            transform.position = spawnPosition;
            body = GetComponent<Rigidbody>();
            PickTarget();
        }
        void FixedUpdate()
        {
            if (defeated) return;
            if (transform.position.y < -8f) { Destroy(gameObject); return; }
            if (Time.time >= nextTargetTime || Vector3.Distance(transform.position, roamTarget) < .35f) PickTarget();
            Vector3 direction = roamTarget - transform.position; direction.y = 0f;
            if (direction.sqrMagnitude < .01f) return;
            direction.Normalize();
            Vector3 nextPosition = body.position + direction * roamSpeed * Time.fixedDeltaTime;
            nextPosition.y = spawnPosition.y;
            if (!HasSolidGroundBelow(nextPosition))
            {
                // Never allow a patrol route to carry an enemy beyond the authored
                // platform mesh. Choose a new route on the next physics tick.
                PickTarget();
                return;
            }
            body.MovePosition(nextPosition);
            body.MoveRotation(Quaternion.Slerp(body.rotation, Quaternion.LookRotation(direction, Vector3.up), 10f * Time.fixedDeltaTime));
        }
        void OnTriggerStay(Collider other)
        {
            TryResolvePlayerContact(other);
        }

        /// <summary>Resolves one player/enemy overlap. Returns true only for a successful stomp.</summary>
        public bool TryResolvePlayerContact(Collider other)
        {
            if (defeated) return false;
            var health = other.GetComponentInParent<PlayerHealth>(); if (health == null) return false;
            var controller = other.GetComponentInParent<CharacterController>();
            // PlayerArmature uses Starter Assets' CharacterController. Do not depend
            // on the unused AdiPrototypeController: that made every overlap damage
            // the player even when landing directly on a SpaceMan.
            float stompPlane = transform.position.y + stompFootClearance;
            float playerFeet = controller != null ? controller.bounds.min.y : health.transform.position.y;
            if (playerFeet >= stompPlane)
            {
                Defeat(health.transform);
                return true;
            }
            health.TakeHit(contactDamage);
            return false;
        }
        void PickTarget()
        {
            Vector2 point = Random.insideUnitCircle * roamRadius;
            roamTarget = spawnPosition + new Vector3(point.x, 0f, point.y);
            // The Level 1 barrel spawns frame the main route at Z 38-62.
            // Keep patrols inset from every platform edge while making the
            // route feel guarded rather than leaving the enemies stationary.
            roamTarget = ClampToGuardCorridor(roamTarget);
            roamTarget.y = spawnPosition.y;
            nextTargetTime = Time.time + Random.Range(1.1f, 2.4f);
        }
        private static Vector3 ClampToGuardCorridor(Vector3 position)
        {
            position.x = Mathf.Clamp(position.x, -7.5f, 7.5f);
            position.z = Mathf.Clamp(position.z, 35f, 65f);
            return position;
        }
        bool HasSolidGroundBelow(Vector3 position)
        {
            RaycastHit[] hits = Physics.RaycastAll(position + Vector3.up * 2f, Vector3.down, 5f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit hit in hits)
            {
                if (!hit.collider.transform.IsChildOf(transform)) return true;
            }
            return false;
        }
        void Defeat(Transform root)
        {
            if (defeated) return;
            defeated = true;
            PlatformerSfx.PlayBlop(transform.position);
            root.GetComponent<ThirdPersonController>()?.ApplyVerticalImpulse(bounceVelocity);
            foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
            transform.localScale *= .25f;
            Destroy(gameObject, .35f);
        }
    }
}

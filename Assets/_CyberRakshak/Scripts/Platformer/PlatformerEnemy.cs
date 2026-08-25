using CyberRakshak.Runtime;
using UnityEngine;
namespace CyberRakshak.Platformer
{
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
    public sealed class PlatformerEnemy : MonoBehaviour
    {
        [SerializeField] float roamSpeed = 2.25f, roamRadius = 6f, stompHeight = 1.55f, bounceVelocity = 10f;
        [SerializeField] int contactDamage = 34;
        Vector3 spawnPosition, roamTarget; Rigidbody body; CapsuleCollider hitbox; float nextTargetTime; bool defeated;
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
            direction.Normalize(); body.MovePosition(body.position + direction * roamSpeed * Time.fixedDeltaTime);
            body.MoveRotation(Quaternion.Slerp(body.rotation, Quaternion.LookRotation(direction, Vector3.up), 10f * Time.fixedDeltaTime));
        }
        void OnTriggerStay(Collider other)
        {
            if (defeated) return;
            var health = other.GetComponentInParent<PlayerHealth>(); if (health == null) return;
            var player = other.GetComponentInParent<AdiPrototypeController>();
            // CharacterController can report grounded before its velocity updates,
            // so a descending-velocity check makes legitimate head landings miss.
            // The controller root is at the player's feet; it must be above the
            // enemy's upper body for a platformer-style stomp.
            float headHeight = hitbox != null ? hitbox.bounds.center.y + hitbox.bounds.extents.y * 0.55f : transform.position.y + stompHeight;
            if (player != null && player.transform.position.y >= headHeight)
            {
                Defeat(other.transform.root);
                return;
            }
            health.TakeHit(contactDamage);
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
        void Defeat(Transform root) { if (defeated) return; defeated = true; PlatformerSfx.PlayBlop(transform.position); root.GetComponent<AdiPrototypeController>()?.Bounce(bounceVelocity); foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false; transform.localScale *= .25f; Destroy(gameObject, .35f); }
    }
}

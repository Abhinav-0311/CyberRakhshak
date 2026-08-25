using CyberRakshak.Runtime;
using UnityEngine;
namespace CyberRakshak.Platformer
{
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
    public sealed class PlatformerEnemy : MonoBehaviour
    {
        [SerializeField] float roamSpeed = 2.25f, roamRadius = 6f, stompHeight = 1.55f, bounceVelocity = 10f;
        [SerializeField] int contactDamage = 34;
        Vector3 spawnPosition, roamTarget; Rigidbody body; float nextTargetTime; bool defeated;
        void Awake() { spawnPosition = transform.position; body = GetComponent<Rigidbody>(); PickTarget(); }
        void FixedUpdate()
        {
            if (defeated) return;
            if (transform.position.y < -8f) { Destroy(gameObject); return; }
            // The shared Scale 5 model starts above the runway; use a grounded range that
            // reaches the plane without allowing edge-of-runway enemies to hover.
            if (!Physics.Raycast(transform.position + Vector3.up * .5f, Vector3.down, 4f, ~0, QueryTriggerInteraction.Ignore)) return;
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
            if (player != null && player.IsDescending && other.bounds.min.y >= transform.position.y + stompHeight) { Defeat(other.transform.root); return; }
            health.TakeHit(contactDamage);
        }
        void PickTarget() { Vector2 point = Random.insideUnitCircle * roamRadius; roamTarget = spawnPosition + new Vector3(point.x, 0f, point.y); nextTargetTime = Time.time + Random.Range(1.1f, 2.4f); }
        void Defeat(Transform root) { if (defeated) return; defeated = true; PlatformerSfx.PlayBlop(transform.position); root.GetComponent<AdiPrototypeController>()?.Bounce(bounceVelocity); foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false; transform.localScale *= .25f; Destroy(gameObject, .35f); }
    }
}

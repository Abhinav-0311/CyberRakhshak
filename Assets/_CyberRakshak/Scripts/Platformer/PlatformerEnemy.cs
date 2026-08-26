using System.Collections;
using CyberRakshak.Runtime;
using UnityEngine;
namespace CyberRakshak.Platformer
{
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
    public sealed class PlatformerEnemy : MonoBehaviour
    {
        [SerializeField] float roamSpeed = 2.25f, roamRadius = 6f, stompFootClearance = .12f, bounceVelocity = 10f, enemyScale = 4f;
        [SerializeField] int contactDamage = 34;
        Vector3 spawnPosition, roamTarget; Rigidbody body; CapsuleCollider hitbox, contactTrigger; float nextTargetTime; bool defeated;

        public bool IsDefeated => defeated;
        void Awake()
        {
            hitbox = GetComponent<CapsuleCollider>();
            // The source prefab's mesh pivot is not at its feet. Normalising the
            // instance scale and grounding its rendered bounds keeps every SpaceMan
            // visibly on the Level 1 runway instead of hovering above it.
            transform.localScale = Vector3.one * enemyScale;
            spawnPosition = ClampToGuardCorridor(transform.position);
            transform.position = spawnPosition;
            SnapVisualToGround();
            ConfigureColliders();
            spawnPosition = transform.position;
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
            // Deliberately forgiving platformer stomp zone. Ground-level side
            // contact stays below this line, while any ordinary jump over the
            // SpaceMan's lower upper-body registers as a defeat.
            float stompPlane = hitbox.bounds.min.y + Mathf.Min(.38f, hitbox.bounds.size.y * .25f) + stompFootClearance;
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
            for (int attempt = 0; attempt < 8; attempt++)
            {
                Vector2 point = Random.insideUnitCircle * roamRadius;
                roamTarget = spawnPosition + new Vector3(point.x, 0f, point.y);
                // The Level 1 barrel spawns frame the main route at Z 38-62.
                // Keep patrols inset from every platform edge while making the
                // route feel guarded rather than leaving the enemies stationary.
                roamTarget = ClampToGuardCorridor(roamTarget);
                roamTarget.y = spawnPosition.y;
                if (HasSolidGroundBelow(roamTarget)) break;
            }
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
            RaycastHit[] hits = Physics.RaycastAll(position + Vector3.up * 8f, Vector3.down, 16f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit hit in hits)
            {
                if (!hit.collider.transform.IsChildOf(transform)) return true;
            }
            return false;
        }
        void SnapVisualToGround()
        {
            if (!TryGetVisualBounds(out Bounds visualBounds)) return;
            RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up * 12f, Vector3.down, 24f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            bool foundGround = false;
            float highestGround = float.NegativeInfinity;
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.transform.IsChildOf(transform) || hit.point.y <= highestGround) continue;
                highestGround = hit.point.y;
                foundGround = true;
            }
            if (!foundGround) return;

            transform.position += Vector3.up * (highestGround + .02f - visualBounds.min.y);
            if (!TryGetVisualBounds(out visualBounds)) return;

            // Keep the trigger where the player sees the SpaceMan rather than at
            // the imported asset's arbitrary root pivot.
            hitbox.center = transform.InverseTransformPoint(visualBounds.center);
            float scaleY = Mathf.Max(.001f, Mathf.Abs(transform.lossyScale.y));
            float scaleXZ = Mathf.Max(.001f, Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z)));
            float worldRadius = Mathf.Max(.12f, Mathf.Min(visualBounds.extents.x, visualBounds.extents.z) * .55f);
            hitbox.radius = worldRadius / scaleXZ;
            hitbox.height = Mathf.Max(visualBounds.size.y * .85f / scaleY, hitbox.radius * 2.01f);
        }
        bool TryGetVisualBounds(out Bounds bounds)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            bool found = false;
            bounds = default;
            foreach (Renderer renderer in renderers)
            {
                if (!renderer.enabled) continue;
                if (!found) { bounds = renderer.bounds; found = true; }
                else bounds.Encapsulate(renderer.bounds);
            }
            return found;
        }
        void ConfigureColliders()
        {
            // A trigger alone lets a CharacterController walk through the enemy.
            // Keep the aligned root capsule solid, then use a slightly larger
            // child trigger to report reliable side/stomp contacts.
            hitbox.isTrigger = false;
            Transform triggerTransform = transform.Find("ContactTrigger");
            if (triggerTransform == null)
            {
                GameObject triggerObject = new GameObject("ContactTrigger");
                triggerTransform = triggerObject.transform;
                triggerTransform.SetParent(transform, false);
                contactTrigger = triggerObject.AddComponent<CapsuleCollider>();
            }
            else contactTrigger = triggerTransform.GetComponent<CapsuleCollider>() ?? triggerTransform.gameObject.AddComponent<CapsuleCollider>();

            contactTrigger.isTrigger = true;
            contactTrigger.center = hitbox.center;
            contactTrigger.radius = hitbox.radius * 1.12f;
            contactTrigger.height = Mathf.Max(hitbox.height * 1.08f, contactTrigger.radius * 2.01f);
            contactTrigger.direction = hitbox.direction;
            PlatformerEnemyContact contact = triggerTransform.GetComponent<PlatformerEnemyContact>() ?? triggerTransform.gameObject.AddComponent<PlatformerEnemyContact>();
            contact.Configure(this);
        }
        void Defeat(Transform root)
        {
            if (defeated) return;
            defeated = true;
            PlatformerSfx.PlayBlop(transform.position);
            StartCoroutine(BouncePlayer(root));
            foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
            transform.localScale *= .25f;
            Destroy(gameObject, .35f);
        }
        IEnumerator BouncePlayer(Transform root)
        {
            CharacterController controller = root.GetComponent<CharacterController>();
            if (controller == null) yield break;
            float velocity = bounceVelocity;
            const float gravity = -25f;
            const float duration = .22f;
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                controller.Move(Vector3.up * velocity * Time.deltaTime);
                velocity += gravity * Time.deltaTime;
                yield return null;
            }
        }
    }
}

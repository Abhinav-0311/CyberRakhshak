using UnityEngine;
using System.Collections;
using CyberRakshak.Platformer;

/// <summary>Launches the player along the pad's forward axis without bypassing CharacterController collision.</summary>
public sealed class JumpBoosterPad : MonoBehaviour
{
    [Header("Launch Settings")]
    [Min(0f)] public float bounceHeight = 6f;
    [Min(0.05f)] public float bounceDuration = 0.8f;
    [Min(0f)] public float forwardDistance = 10f;
    [Min(0f)] public float topTolerance = 0.55f;
    [Min(0f)] public float landingClearance = 0.12f;
    [Tooltip("Optional scene object name. When assigned, the pad launches toward that object instead of only forward.")]
    public string launchTargetName;

    private CharacterController playerController;
    private bool hasLaunched;
    private bool isLaunching;

    private void Update()
    {
        if (!TryGetPlayerController(out CharacterController controller))
        {
            return;
        }

        Bounds padBounds = GetPadBounds();
        Bounds playerBounds = controller.bounds;
        Vector3 playerCenter = playerBounds.center;
        bool insidePad = playerCenter.x >= padBounds.min.x && playerCenter.x <= padBounds.max.x &&
                         playerCenter.z >= padBounds.min.z && playerCenter.z <= padBounds.max.z;
        bool standingOnPad = playerBounds.min.y >= padBounds.max.y - topTolerance &&
                             playerBounds.min.y <= padBounds.max.y + topTolerance;

        if (!isLaunching && insidePad && standingOnPad && controller.isGrounded && !hasLaunched)
        {
            hasLaunched = true;
            StartCoroutine(Launch(controller));
        }

        if (!insidePad && !isLaunching)
        {
            hasLaunched = false;
        }
    }

    private bool TryGetPlayerController(out CharacterController controller)
    {
        if (playerController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerController = player.GetComponent<CharacterController>();
            }
        }

        controller = playerController;
        return controller != null;
    }

    private Bounds GetPadBounds()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        Bounds bounds = default;
        bool hasBounds = false;

        foreach (Collider collider in colliders)
        {
            if (!collider.enabled || collider.isTrigger)
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = collider.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(collider.bounds);
            }
        }

        return hasBounds ? bounds : new Bounds(transform.position, Vector3.one);
    }

    private IEnumerator Launch(CharacterController controller)
    {
        isLaunching = true;
        PlatformerMotionAdapter.BeginTraversalOverride(controller);
        Vector3 startPosition = controller.transform.position;
        Vector3 direction = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        if (direction.sqrMagnitude < 0.01f)
        {
            direction = Vector3.forward;
        }

        Transform target = FindLaunchTarget();
        Vector3 destination = target != null
            ? GetLandingPosition(target, startPosition)
            : startPosition + direction * forwardDistance;

        float elapsed = 0f;
        while (elapsed < bounceDuration && controller != null)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / bounceDuration);
            // Smoothstep removes the sudden start/stop that made the old launch feel mechanical.
            float horizontalProgress = progress * progress * (3f - 2f * progress);
            float arc = Mathf.Sin(progress * Mathf.PI);
            Vector3 flightPosition = Vector3.Lerp(startPosition, destination, horizontalProgress) + Vector3.up * (bounceHeight * arc);
            PlatformerMotionAdapter.MoveTraversalOverride(controller, flightPosition - controller.transform.position);
            yield return null;
        }

        if (controller != null)
        {
            PlatformerMotionAdapter.EndTraversalOverride(controller);
        }
        isLaunching = false;
    }

    private void OnDisable()
    {
        if (isLaunching && playerController != null)
        {
            PlatformerMotionAdapter.EndTraversalOverride(playerController);
        }

        isLaunching = false;
    }

    private Vector3 GetLandingPosition(Transform target, Vector3 startPosition)
    {
        Collider landingSurface = FindLandingSurface(target);
        if (landingSurface == null)
        {
            // A lever is not a landing surface: preserve its horizontal destination, then let the arc land naturally.
            return new Vector3(target.position.x, startPosition.y, target.position.z);
        }

        Bounds landingBounds = landingSurface.bounds;
        return new Vector3(landingBounds.center.x, landingBounds.max.y + landingClearance, landingBounds.center.z);
    }

    private static Collider FindLandingSurface(Transform target)
    {
        // Most authored pads place their real collider on a child mesh, not on the named root.
        foreach (Collider candidate in target.GetComponentsInChildren<Collider>())
        {
            if (candidate.enabled && !candidate.isTrigger)
            {
                return candidate;
            }
        }

        Collider ownCollider = target.GetComponent<Collider>();
        return ownCollider != null && ownCollider.enabled && !ownCollider.isTrigger
            ? ownCollider
            : target.GetComponentInParent<Collider>();
    }

    private Transform FindLaunchTarget()
    {
        return string.IsNullOrWhiteSpace(launchTargetName)
            ? null
            : GameObject.Find(launchTargetName)?.transform;
    }
}

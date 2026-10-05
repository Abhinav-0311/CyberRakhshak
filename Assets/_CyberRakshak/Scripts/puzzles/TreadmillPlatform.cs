using UnityEngine;
using CyberRakshak.Platformer;
using CyberRakshak.PATCH;

public class TreadmillPlatform : MonoBehaviour
{
    [Header("Treadmill Settings")]
    public Vector3 backwardDirection = Vector3.back; // belt direction
    public float resistanceSpeed = 1.25f;            // enough pressure to feel like a belt, not enough to eject the player
    [Min(0f)] public float edgeSafetyMargin = 0.55f;

    private GameObject player;
    private CharacterController cc;
    private PatchDialoguePresenter presenter;
    private Collider beltCollider;
    private LineRenderer[] directionMarkers;
    private Material markerMaterial;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            cc = player.GetComponent<CharacterController>() ??
                 player.GetComponentInChildren<CharacterController>();
        }

        backwardDirection = backwardDirection.normalized;
        beltCollider = GetComponent<Collider>();
        markerMaterial = new Material(Shader.Find("Sprites/Default"));
        directionMarkers = new LineRenderer[3];
        for (int i = 0; i < directionMarkers.Length; i++)
        {
            var marker = new GameObject("BeltDirection_" + i, typeof(LineRenderer));
            marker.transform.SetParent(transform, false);
            marker.transform.rotation = Quaternion.LookRotation(Vector3.up);
            var line = marker.GetComponent<LineRenderer>();
            line.sharedMaterial = markerMaterial;
            line.useWorldSpace = true;
            line.alignment = LineAlignment.TransformZ;
            line.positionCount = 3;
            line.startWidth = line.endWidth = .18f;
            line.startColor = line.endColor = new Color(.15f, .9f, 1f);
            directionMarkers[i] = line;
        }
    }

    void Update()
    {
        UpdateDirectionMarkers();
        if (player == null || cc == null || PlatformerMotionAdapter.IsTraversalOverrideActive(cc)) return;

        Bounds b = beltCollider.bounds;
        Bounds playerBounds = cc.bounds;
        Vector3 p = playerBounds.center;

        bool insideXZ =
            p.x > b.min.x && p.x < b.max.x &&
            p.z > b.min.z && p.z < b.max.z;

        bool onTop = playerBounds.min.y >= b.max.y - 0.12f &&
                     playerBounds.min.y <= b.max.y + 0.65f;

        bool onPlatform = insideXZ && onTop && cc.isGrounded;

        if (onPlatform)
        {
            if (presenter == null) presenter = PatchDialoguePresenter.Ensure();
            if (presenter.ShowOnce("level1-treadmill", "PATCH",
                "The belt is pushing back. Keep moving forward and time your final jumps.", 4f))
            {
                PlatformerHud.Instance?.SetObjective("REACH THE EXIT PLATFORM");
            }

            // A conveyor should add pressure, not push the player off a tile with no recovery.
            Vector3 beltMove = backwardDirection * resistanceSpeed * Time.deltaTime;
            Vector3 nextCenter = p + beltMove;
            bool staysOnBelt = nextCenter.x >= b.min.x + edgeSafetyMargin &&
                               nextCenter.x <= b.max.x - edgeSafetyMargin &&
                               nextCenter.z >= b.min.z + edgeSafetyMargin &&
                               nextCenter.z <= b.max.z - edgeSafetyMargin;
            if (staysOnBelt)
            {
                PlatformerMotionAdapter.ApplyEnvironmentalDisplacement(cc, beltMove);
            }
        }
    }

    private void UpdateDirectionMarkers()
    {
        Bounds bounds = beltCollider.bounds;
        Vector3 direction = Vector3.ProjectOnPlane(backwardDirection, Vector3.up).normalized;
        Vector3 side = Vector3.Cross(Vector3.up, direction);
        float length = 2f * (Mathf.Abs(direction.x) * bounds.extents.x + Mathf.Abs(direction.z) * bounds.extents.z) - 2f;
        if (length <= 0f) return;
        float halfWidth = .75f * (Mathf.Abs(side.x) * bounds.extents.x + Mathf.Abs(side.z) * bounds.extents.z);
        for (int i = 0; i < directionMarkers.Length; i++)
        {
            float offset = Mathf.Repeat(Time.time * resistanceSpeed + i * length / directionMarkers.Length, length) - length * .5f;
            Vector3 center = new Vector3(bounds.center.x, bounds.max.y + .04f, bounds.center.z) + direction * offset;
            directionMarkers[i].SetPosition(0, center - direction * .6f - side * halfWidth);
            directionMarkers[i].SetPosition(1, center + direction * .6f);
            directionMarkers[i].SetPosition(2, center - direction * .6f + side * halfWidth);
        }
    }

    private void OnDestroy()
    {
        if (markerMaterial != null) Destroy(markerMaterial);
    }
}

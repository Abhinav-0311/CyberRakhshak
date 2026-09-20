using UnityEngine;
using CyberRakshak.Platformer;

public class TreadmillPlatform : MonoBehaviour
{
    [Header("Treadmill Settings")]
    public Vector3 backwardDirection = Vector3.back; // belt direction
    public float resistanceSpeed = 1.25f;            // enough pressure to feel like a belt, not enough to eject the player
    [Min(0f)] public float edgeSafetyMargin = 0.55f;

    private GameObject player;
    private CharacterController cc;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            cc = player.GetComponent<CharacterController>();
        }

        backwardDirection = backwardDirection.normalized;
    }

    void Update()
    {
        if (player == null || cc == null || PlatformerMotionAdapter.IsTraversalOverrideActive(cc)) return;

        Bounds b = GetComponent<Collider>().bounds;
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
}

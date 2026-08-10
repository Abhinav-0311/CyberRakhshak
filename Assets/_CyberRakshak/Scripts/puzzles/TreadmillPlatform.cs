using UnityEngine;

public class TreadmillPlatform : MonoBehaviour
{
    [Header("Treadmill Settings")]
    public Vector3 backwardDirection = Vector3.back; // belt direction
    public float resistanceSpeed = 2f;               // how much it slows the player

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
        if (player == null || cc == null) return;

        Bounds b = GetComponent<Collider>().bounds;
        Vector3 p = player.transform.position;

        bool insideXZ =
            p.x > b.min.x && p.x < b.max.x &&
            p.z > b.min.z && p.z < b.max.z;

        bool onTop = Mathf.Abs(p.y - b.max.y) < 1.2f;

        bool onPlatform = insideXZ && onTop && cc.isGrounded;

        if (onPlatform)
        {
            // Apply only a small backward movement
            cc.Move(backwardDirection * resistanceSpeed * Time.deltaTime);
        }
    }
}

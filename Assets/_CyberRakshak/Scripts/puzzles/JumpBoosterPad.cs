using UnityEngine;
using System.Collections;

public class JumpBoosterPad : MonoBehaviour
{
    [Header("Bounce Settings")]
    public float bounceHeight = 4f;
    public float bounceDuration = 0.6f;

    private bool hasBounced = false;

    void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc == null) return;

        Bounds b = GetComponent<Collider>().bounds;
        Vector3 p = player.transform.position;

        // Check if player is standing on the pad area
        bool insideXZ =
            p.x > b.min.x && p.x < b.max.x &&
            p.z > b.min.z && p.z < b.max.z;

        bool onTop = Mathf.Abs(p.y - b.max.y) < 1.2f;

        bool onPad = insideXZ && onTop && cc.isGrounded;

        // Bounce only once per entry
        if (onPad && !hasBounced)
        {
            hasBounced = true;
            StartCoroutine(Bounce(player.transform));
        }

        // Reset only after player walks completely off the pad
        if (!insideXZ)
        {
            hasBounced = false;
        }
    }

    IEnumerator Bounce(Transform player)
    {
        float startPos = player.position.y; //y coordinate position

        float t = 0f;

        while (t < bounceDuration)
        {
            t += Time.deltaTime;

            float normalized = t / bounceDuration;

            // Smooth parabola: 0 → 1 → 0
            float height = 4f * normalized * (1f - normalized);

            player.position = new Vector3(player.position.x,startPos,player.position.z)+ Vector3.up * (height * bounceHeight);

            yield return null;
        }

        // Ensure player lands exactly back where the bounce started
        player.position = new Vector3(player.position.x, startPos, player.position.z);
    }
}

using UnityEngine;

public class LeverSwitch : MonoBehaviour
{
    [Header("Assign the waterfall object here")]
    public GameObject waterfall;

    [Header("Lever rotation after activation")]
    public Vector3 activatedRotation = new Vector3(0, 0, -45);

    public float interactionDistance = 3f;

    private bool activated = false;
    private Transform player;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
            player = p.transform;

        if (waterfall != null)
            waterfall.SetActive(false);
    }

    void Update()
    {
        if (activated || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= interactionDistance && Input.GetKeyDown(KeyCode.E))
        {
            ActivateLever();
        }
    }

    void ActivateLever()
    {
        activated = true;

        // Turn on waterfall
        if (waterfall != null)
            waterfall.SetActive(true);

        // Rotate lever
        transform.localRotation = Quaternion.Euler(activatedRotation);

        Debug.Log("Waterfall activated!");
    }
}

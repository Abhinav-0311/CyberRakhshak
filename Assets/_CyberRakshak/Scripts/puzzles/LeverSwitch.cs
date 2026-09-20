using UnityEngine;

public class LeverSwitch : MonoBehaviour
{
    [Header("Assign the waterfall object here")]
    public GameObject waterfall;

    [Header("Optional fire gate cleared by this water source")]
    public CyberRakshak.FirewallHazard firewall;

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
        if (Time.timeScale == 0f || activated || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= interactionDistance && Input.GetKeyDown(KeyCode.E))
        {
            ActivateLever();
        }
    }

    void ActivateLever()
    {
        if (waterfall == null || firewall == null || firewall.GetComponent<Collider>() == null)
        {
            Debug.LogWarning("Water lever was not activated because its waterfall or Firewall_Gate reference is missing.");
            return;
        }

        PlaceWaterfallOverFirewall();

        // Turn on waterfall only after it has been positioned above the fire gate.
        if (waterfall != null)
            waterfall.SetActive(true);

        firewall.BeginExtinguish();

        // Rotate lever
        transform.localRotation = Quaternion.Euler(activatedRotation);
        activated = true;

        Debug.Log("Waterfall activated. Firewall is being cleared.");
    }

    private void PlaceWaterfallOverFirewall()
    {
        if (waterfall == null || firewall == null)
        {
            return;
        }

        Collider gateCollider = firewall.GetComponent<Collider>();
        if (gateCollider == null)
        {
            return;
        }

        // The authored waterfall emits along local -Y at X=90 degrees.
        waterfall.transform.position = gateCollider.bounds.center + Vector3.up * 8f;
        waterfall.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}

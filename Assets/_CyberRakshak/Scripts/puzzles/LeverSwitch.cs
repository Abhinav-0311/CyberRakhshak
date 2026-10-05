using CyberRakshak.PATCH;
using CyberRakshak.Platformer;
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
    private bool promptShown;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
        {
            CharacterController controller = p.GetComponentInChildren<CharacterController>();
            player = controller != null ? controller.transform : p.transform;
        }

        if (waterfall != null)
            waterfall.SetActive(false);
    }

    void Update()
    {
        if (Time.timeScale == 0f || activated || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= interactionDistance && !promptShown)
        {
            promptShown = true;
            PlatformerHud.Instance?.SetObjective("PRESS E TO ACTIVATE WATER CONTROL");
            PatchDialoguePresenter.Ensure().Show("PATCH", "Press E to open the water control. This training simulation uses fire as a visual barrier; a real network firewall filters traffic using rules.", 8f);
        }
        else if (distance > interactionDistance)
        {
            promptShown = false;
        }

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
        PlatformerHud.Instance?.SetObjective("CROSS THE OPEN FIREWALL");
        PatchDialoguePresenter.Ensure().Show("PATCH", "Water flow active. The firewall is clearing—move through once the path opens.", 4f);

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

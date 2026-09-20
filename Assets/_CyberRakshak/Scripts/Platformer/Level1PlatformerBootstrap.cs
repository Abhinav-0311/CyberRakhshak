using UnityEngine;
using UnityEngine.SceneManagement;

namespace CyberRakshak.Platformer
{
    /// <summary>Connects the authored Level 1 enemies and player health when the firewall scene starts.</summary>
    public sealed class Level1PlatformerBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            if (SceneManager.GetActiveScene().name != "Game_Level01")
            {
                enabled = false;
                return;
            }

            GameObject player = GameObject.Find("PlayerArmature");
            if (player == null)
            {
                Debug.LogWarning("Platformer setup: PlayerArmature was not found.");
                return;
            }

            if (player.GetComponent<PlayerHealth>() == null)
            {
                player.AddComponent<PlayerHealth>();
            }
            PlatformerHud.EnsureCreated();
            ConfigurePlatformerTraversal(player);
            ConfigureJumpPadRoute();
            ConfigureWaterFirewallSequence();
        }

        private static void ConfigurePlatformerTraversal(GameObject player)
        {
            // CyberRakshak.asmdef intentionally does not reference StarterAssets.
            // Tune its public values by name so Level 1 keeps a simple, arcade platformer jump.
            Component controller = player.GetComponent("ThirdPersonController");
            if (controller != null)
            {
                SetPublicFloat(controller, "JumpHeight", 2f);
                SetPublicFloat(controller, "JumpTimeout", .2f);
                SetPublicFloat(controller, "FallTimeout", .08f);
                SetPublicFloat(controller, "LeapDistance", 6.2f);
                SetPublicFloat(controller, "LeapHeight", 1.8f);
                SetPublicFloat(controller, "LeapDuration", .9f);
            }

            foreach (TreadmillPlatform treadmill in FindObjectsByType<TreadmillPlatform>(FindObjectsSortMode.None))
            {
                treadmill.resistanceSpeed = 1.25f;
                treadmill.edgeSafetyMargin = .55f;
            }
        }

        private static void SetPublicFloat(Component component, string fieldName, float value)
        {
            var field = component.GetType().GetField(fieldName);
            if (field != null && field.FieldType == typeof(float))
            {
                field.SetValue(component, value);
            }
        }

        private static void ConfigureJumpPadRoute()
        {
            JumpBoosterPad startPad = GameObject.Find("LaunchPad_Start")?.GetComponent<JumpBoosterPad>();
            JumpBoosterPad upperPad = GameObject.Find("LaunchPad_Upper")?.GetComponent<JumpBoosterPad>();

            if (startPad != null)
            {
                startPad.launchTargetName = "LaunchPad_Upper";
                startPad.bounceHeight = 4.2f;
                startPad.bounceDuration = 1.05f;
            }

            if (upperPad != null)
            {
                upperPad.launchTargetName = "WaterControl_Lever";
                upperPad.bounceHeight = 4.8f;
                upperPad.bounceDuration = 1.2f;
            }
        }

        private static void ConfigureWaterFirewallSequence()
        {
            LeverSwitch lever = FindFirstObjectByType<LeverSwitch>(FindObjectsInactive.Include);
            if (lever == null)
            {
                return;
            }

            GameObject gate = GameObject.Find("Firewall_Gate");
            Collider gateCollider = gate != null ? gate.GetComponent<Collider>() : null;
            if (gateCollider == null)
            {
                Debug.LogWarning("Middle section: Firewall_Gate is missing its required collider.");
                return;
            }

            CyberRakshak.FirewallHazard firewall = gateCollider.GetComponent<CyberRakshak.FirewallHazard>();
            if (firewall == null)
            {
                firewall = gateCollider.gameObject.AddComponent<CyberRakshak.FirewallHazard>();
            }

            lever.firewall = firewall;
        }
    }
}

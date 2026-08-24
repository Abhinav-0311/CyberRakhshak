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
        }
    }
}

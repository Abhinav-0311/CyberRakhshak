using UnityEngine;

namespace CyberRakshak.Prototype
{
    public sealed class CheckpointRespawn : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Transform checkpoint;

        private void Awake()
        {
            if (player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                player = playerObject != null ? playerObject.transform : null;
            }
        }

        public void Respawn()
        {
            if (player == null || checkpoint == null)
            {
                return;
            }

            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            player.SetPositionAndRotation(checkpoint.position, checkpoint.rotation);

            if (controller != null)
            {
                controller.enabled = true;
            }
        }
    }
}


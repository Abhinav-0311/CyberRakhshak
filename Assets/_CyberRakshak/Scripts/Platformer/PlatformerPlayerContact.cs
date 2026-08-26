using UnityEngine;

namespace CyberRakshak.Platformer
{
    /// <summary>Uses the CharacterController's reliable collision callback for SpaceMan contact.</summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlatformerPlayerContact : MonoBehaviour
    {
        CharacterController characterController;

        void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.collider == null) return;
            hit.collider.GetComponentInParent<PlatformerEnemy>()?.TryResolvePlayerContact(characterController);
        }
    }
}

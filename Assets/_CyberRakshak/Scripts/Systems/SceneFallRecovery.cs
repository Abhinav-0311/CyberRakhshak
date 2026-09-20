using System.Reflection;
using UnityEngine;

namespace CyberRakshak.Runtime
{
    /// <summary>Returns the player to the scene start after an out-of-bounds fall without reloading the module.</summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class SceneFallRecovery : MonoBehaviour
    {
        [SerializeField] private float fallY = -8f;

        private CharacterController controller;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private bool recovering;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        private void Update()
        {
            if (!recovering && transform.position.y < fallY)
            {
                Recover();
            }
        }

        private void Recover()
        {
            recovering = true;
            controller.enabled = false;
            transform.SetPositionAndRotation(startPosition, startRotation);
            controller.enabled = true;
            ResetStarterAssetsMotion();
            recovering = false;
        }

        private void ResetStarterAssetsMotion()
        {
            Component controllerComponent = GetComponent("ThirdPersonController");
            if (controllerComponent == null)
            {
                return;
            }

            System.Type type = controllerComponent.GetType();
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
            type.GetField("_verticalVelocity", fields)?.SetValue(controllerComponent, -2f);
            type.GetField("_isLeaping", fields)?.SetValue(controllerComponent, false);
        }
    }
}

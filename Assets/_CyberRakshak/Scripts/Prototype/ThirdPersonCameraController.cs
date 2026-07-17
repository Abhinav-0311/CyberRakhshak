using UnityEngine;

namespace CyberRakshak.Runtime
{
    [RequireComponent(typeof(Camera))]
    public sealed class ThirdPersonCameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float distance = 6f;
        [SerializeField] private float lookHeight = 1.25f;
        [SerializeField] private float mouseSensitivity = 180f;
        [SerializeField] private float minPitch = -20f;
        [SerializeField] private float maxPitch = 55f;
        [SerializeField] private float positionSmoothTime = 0.06f;

        private float yaw;
        private float pitch;
        private Vector3 positionVelocity;

private void Awake()
        {
            if (target == null)
            {
                GameObject player = GameObject.Find("AdiPlayer");
                target = player != null ? player.transform : null;
            }

            if (target == null)
            {
                enabled = false;
                return;
            }

            Vector3 focus = target.position + Vector3.up * lookHeight;
            Vector3 offset = transform.position - focus;
            distance = Mathf.Max(2f, offset.magnitude);
            yaw = Mathf.Atan2(-offset.x, -offset.z) * Mathf.Rad2Deg;
            pitch = Mathf.Asin(offset.y / distance) * Mathf.Rad2Deg;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            yaw += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focus = target.position + Vector3.up * lookHeight;
            Vector3 desiredPosition = focus - rotation * Vector3.forward * distance;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref positionVelocity,
                positionSmoothTime);

            transform.rotation = rotation;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
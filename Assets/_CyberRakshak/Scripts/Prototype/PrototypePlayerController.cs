using UnityEngine;

namespace CyberRakshak.Prototype
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PrototypePlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5.5f;
        [SerializeField] private float sprintMultiplier = 1.45f;
        [SerializeField] private float jumpHeight = 1.4f;
        [SerializeField] private float gravity = -24f;
        [SerializeField] private Transform cameraTransform;

        private CharacterController controller;
        private Vector3 verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            Move();
        }

        private void Move()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 input = new(horizontal, 0f, vertical);
            input = Vector3.ClampMagnitude(input, 1f);

            Vector3 moveDirection = GetCameraRelativeDirection(input);
            float speed = Input.GetKey(KeyCode.LeftShift) ? moveSpeed * sprintMultiplier : moveSpeed;
            controller.Move(speed * Time.deltaTime * moveDirection);

            if (controller.isGrounded && verticalVelocity.y < 0f)
            {
                verticalVelocity.y = -2f;
            }

            if (controller.isGrounded && Input.GetButtonDown("Jump"))
            {
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalVelocity.y += gravity * Time.deltaTime;
            controller.Move(verticalVelocity * Time.deltaTime);

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(moveDirection, Vector3.up),
                    12f * Time.deltaTime);
            }
        }

        private Vector3 GetCameraRelativeDirection(Vector3 input)
        {
            if (cameraTransform == null)
            {
                return input;
            }

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return forward * input.z + right * input.x;
        }
    }
}


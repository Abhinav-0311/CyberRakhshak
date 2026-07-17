using UnityEngine;

namespace CyberRakshak.Runtime
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class AdiPrototypeController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform followCamera;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 4.5f;
        [SerializeField] private float runSpeed = 7.2f;
        [SerializeField] private float jumpHeight = 1.45f;
        [SerializeField] private float gravity = -24f;
        [SerializeField] private float turnSpeed = 14f;


        private CharacterController controller;
        private Vector3 verticalVelocity;
        private Vector3 visualStartPosition;
        private Quaternion visualStartRotation;

        private Animator animator;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (visualRoot == null && transform.childCount > 0)
            {
                visualRoot = transform.GetChild(0);
            }

            if (visualRoot != null)
            {
                visualStartPosition = visualRoot.localPosition;
                visualStartRotation = visualRoot.localRotation;
                animator = visualRoot.GetComponent<Animator>();
            }

            if (followCamera == null && Camera.main != null)
            {
                followCamera = Camera.main.transform;
            }
        }

private void Update()
        {
            Vector3 input = ReadInput();
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float speed = isRunning ? runSpeed : walkSpeed;
            Vector3 moveDirection = CameraRelativeDirection(input);

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    turnSpeed * Time.deltaTime);
            }

            if (controller.isGrounded && verticalVelocity.y < 0f)
            {
                verticalVelocity.y = -2f;
            }

            if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
            {
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

                if (animator != null)
                {
                    animator.speed = 2.55f;
                    animator.CrossFadeInFixedTime("Base Layer.Jump", 0.05f);
                }
            }

            verticalVelocity.y += gravity * Time.deltaTime;
            Vector3 velocity = moveDirection * speed + verticalVelocity;
            controller.Move(velocity * Time.deltaTime);

            UpdateAnimator(input.magnitude, isRunning);
            AnimateVisual();
        }

private void LateUpdate()
        {
        }

        private static Vector3 ReadInput()
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
            if (Input.GetKey(KeyCode.D)) horizontal += 1f;
            if (Input.GetKey(KeyCode.S)) vertical -= 1f;
            if (Input.GetKey(KeyCode.W)) vertical += 1f;

            return Vector3.ClampMagnitude(new Vector3(horizontal, 0f, vertical), 1f);
        }

private Vector3 CameraRelativeDirection(Vector3 input)
        {
            if (input.sqrMagnitude <= 0.001f)
            {
                return Vector3.zero;
            }

            Transform cameraTransform = followCamera != null ? followCamera : Camera.main?.transform;
            if (cameraTransform == null)
            {
                return input;
            }

            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            return Vector3.ClampMagnitude(forward * input.z + right * input.x, 1f);
        }


private void UpdateAnimator(float inputAmount, bool isRunning)
        {
            if (animator == null)
            {
                return;
            }

            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(0);
            bool jumpIsActive = currentState.IsName("Base Layer.Jump")
                || (animator.IsInTransition(0) && nextState.IsName("Base Layer.Jump"));

            if (jumpIsActive)
            {
                return;
            }

            string targetState = inputAmount <= 0.01f
                ? "Base Layer.Idle"
                : isRunning
                    ? "Base Layer.Run"
                    : "Base Layer.Walk";

            bool targetIsActive = currentState.IsName(targetState)
                || (animator.IsInTransition(0) && nextState.IsName(targetState));

            animator.speed = targetState == "Base Layer.Walk" ? 0.75f : 1f;

            if (!targetIsActive)
            {
                animator.CrossFadeInFixedTime(targetState, 0.12f);
            }
        }


private void AnimateVisual()
        {
            if (visualRoot == null)
            {
                return;
            }

            visualRoot.localPosition = visualStartPosition;
            visualRoot.localRotation = visualStartRotation;
        }
    }
}

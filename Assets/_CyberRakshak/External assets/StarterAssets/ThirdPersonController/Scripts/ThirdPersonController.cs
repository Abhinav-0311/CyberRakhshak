using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioSource AudioFootsteps;
        public AudioSource LandingAudio;
        public AudioSource AudioFoley;
        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;

        [Range(0, 1)]
        public float FootstepAudioVolume = 0.5f;

        // ============================================================
        // NORMAL JUMP
        // ============================================================

        [Space(10)]
        [Header("Jump")]

        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value")]
        public float Gravity = -15.0f;

        [Space(10)]

        [Tooltip("Time required to pass before being able to jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required before entering the fall state")]
        public float FallTimeout = 0.15f;

        // ============================================================
        // LEAP
        // ============================================================

        [Space(10)]
        [Header("Leap")]

        [Tooltip("Distance the player will travel during a leap")]
        public float LeapDistance = 5.0f;

        [Tooltip("Maximum height of the leap")]
        public float LeapHeight = 1.5f;

        [Tooltip("Time taken to complete the leap")]
        public float LeapDuration = 0.8f;

        [Tooltip("Minimum movement input required to perform a leap")]
        [Range(0.0f, 1.0f)]
        public float LeapInputThreshold = 0.1f;

        // ============================================================
        // GROUNDED
        // ============================================================

        [Header("Player Grounded")]

        [Tooltip("If the character is grounded or not")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        // ============================================================
        // CAMERA
        // ============================================================

        [Header("Cinemachine")]

        [Tooltip("The follow target set in the Cinemachine Virtual Camera")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degrees to override the camera")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // ============================================================
        // CINEMACHINE
        // ============================================================

        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // ============================================================
        // PLAYER
        // ============================================================

        private float _speed;
        private float _animationBlend;

        private float _targetRotation = 0.0f;
        private float _rotationVelocity;

        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // ============================================================
        // NORMAL JUMP TIMERS
        // ============================================================

        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // ============================================================
        // LEAP VARIABLES
        // ============================================================

        private bool _isLeaping = false;

        private float _leapTimer = 0.0f;

        private Vector3 _leapStartPosition;
        private Vector3 _leapDirection;

        // ============================================================
        // ANIMATION IDS
        // ============================================================

        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif

        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        /// <summary>Applies a one-shot upward velocity from Level 1 platformer interactions.</summary>
        public void ApplyVerticalImpulse(float upwardVelocity)
        {
            _verticalVelocity = Mathf.Max(_verticalVelocity, upwardVelocity);
            Grounded = false;
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, true);
                _animator.SetBool(_animIDFreeFall, false);
            }
        }

        // ============================================================
        // INPUT DEVICE
        // ============================================================

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        // ============================================================
        // AWAKE
        // ============================================================

        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        // ============================================================
        // START
        // ============================================================

        private void Start()
        {
            _cinemachineTargetYaw =
                CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);

            _controller = GetComponent<CharacterController>();

            _input = GetComponent<StarterAssetsInputs>();

#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError(
                "Starter Assets package is missing dependencies. " +
                "Please use Tools/Starter Assets/Reinstall Dependencies to fix it"
            );
#endif

            AssignAnimationIDs();

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        // ============================================================
        // UPDATE
        // ============================================================

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            GroundedCheck();

            // If currently leaping, handle leap movement separately
            if (_isLeaping)
            {
                HandleLeap();
            }
            else
            {
                JumpAndGravity();
                Move();
                CheckForLeap();
            }
        }

        // ============================================================
        // LATE UPDATE
        // ============================================================

        private void LateUpdate()
        {
            CameraRotation();
        }

        // ============================================================
        // ANIMATION IDS
        // ============================================================

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        // ============================================================
        // GROUNDED CHECK
        // ============================================================

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(
                transform.position.x,
                transform.position.y - GroundedOffset,
                transform.position.z
            );

            Grounded = Physics.CheckSphere(
                spherePosition,
                GroundedRadius,
                GroundLayers,
                QueryTriggerInteraction.Ignore
            );

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        // ============================================================
        // CAMERA ROTATION
        // ============================================================

        private void CameraRotation()
        {
            if (_input.look.sqrMagnitude >= _threshold &&
                !LockCameraPosition)
            {
                float deltaTimeMultiplier =
                    IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                float sensitivity =
                    Mathf.Lerp(
                        0.1f,
                        3.0f,
                        PlayerPrefs.GetFloat(
                            "CyberRakshak.Sensitivity",
                            0.5f
                        )
                    );

                _cinemachineTargetYaw +=
                    _input.look.x *
                    deltaTimeMultiplier *
                    sensitivity;

                _cinemachineTargetPitch +=
                    _input.look.y *
                    deltaTimeMultiplier *
                    sensitivity;
            }

            _cinemachineTargetYaw =
                ClampAngle(
                    _cinemachineTargetYaw,
                    float.MinValue,
                    float.MaxValue
                );

            _cinemachineTargetPitch =
                ClampAngle(
                    _cinemachineTargetPitch,
                    BottomClamp,
                    TopClamp
                );

            CinemachineCameraTarget.transform.rotation =
                Quaternion.Euler(
                    _cinemachineTargetPitch + CameraAngleOverride,
                    _cinemachineTargetYaw,
                    0.0f
                );
        }

        // ============================================================
        // NORMAL MOVEMENT
        // ============================================================

        private void Move()
        {
            float targetSpeed =
                _input.sprint ? SprintSpeed : MoveSpeed;

            if (_input.move == Vector2.zero)
            {
                targetSpeed = 0.0f;
            }

            float currentHorizontalSpeed =
                new Vector3(
                    _controller.velocity.x,
                    0.0f,
                    _controller.velocity.z
                ).magnitude;

            float speedOffset = 0.1f;

            float inputMagnitude =
                _input.analogMovement
                    ? _input.move.magnitude
                    : 1f;

            if (
                currentHorizontalSpeed <
                    targetSpeed - speedOffset ||

                currentHorizontalSpeed >
                    targetSpeed + speedOffset
            )
            {
                _speed = Mathf.Lerp(
                    currentHorizontalSpeed,
                    targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate
                );

                _speed =
                    Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend =
                Mathf.Lerp(
                    _animationBlend,
                    targetSpeed,
                    Time.deltaTime * SpeedChangeRate
                );

            if (_animationBlend < 0.01f)
            {
                _animationBlend = 0f;
            }

            Vector3 inputDirection =
                new Vector3(
                    _input.move.x,
                    0.0f,
                    _input.move.y
                ).normalized;

            if (_input.move != Vector2.zero)
            {
                _targetRotation =
                    Mathf.Atan2(
                        inputDirection.x,
                        inputDirection.z
                    ) *
                    Mathf.Rad2Deg +
                    _mainCamera.transform.eulerAngles.y;

                float rotation =
                    Mathf.SmoothDampAngle(
                        transform.eulerAngles.y,
                        _targetRotation,
                        ref _rotationVelocity,
                        RotationSmoothTime
                    );

                transform.rotation =
                    Quaternion.Euler(
                        0.0f,
                        rotation,
                        0.0f
                    );
            }

            Vector3 targetDirection =
                Quaternion.Euler(
                    0.0f,
                    _targetRotation,
                    0.0f
                ) *
                Vector3.forward;

            _controller.Move(
                targetDirection.normalized *
                (_speed * Time.deltaTime) +

                new Vector3(
                    0.0f,
                    _verticalVelocity,
                    0.0f
                ) *
                Time.deltaTime
            );

            if (_hasAnimator)
            {
                _animator.SetFloat(
                    _animIDSpeed,
                    _animationBlend
                );

                _animator.SetFloat(
                    _animIDMotionSpeed,
                    inputMagnitude
                );
            }
        }

        // ============================================================
        // NORMAL JUMP + GRAVITY
        // ============================================================

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (_hasAnimator)
                {
                    _animator.SetBool(
                        _animIDJump,
                        false
                    );

                    _animator.SetBool(
                        _animIDFreeFall,
                        false
                    );
                }

                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                if (
                    _input.jump &&
                    _jumpTimeoutDelta <= 0.0f
                )
                {
                    _verticalVelocity =
                        Mathf.Sqrt(
                            JumpHeight *
                            -2f *
                            Gravity
                        );

                    if (_hasAnimator)
                    {
                        _animator.SetBool(
                            _animIDJump,
                            true
                        );
                    }
                }

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -=
                        Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -=
                        Time.deltaTime;
                }
                else
                {
                    if (_hasAnimator)
                    {
                        _animator.SetBool(
                            _animIDFreeFall,
                            true
                        );
                    }
                }

                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity +=
                    Gravity *
                    Time.deltaTime;
            }
        }

        // ============================================================
        // CHECK FOR LEAP
        // ============================================================

        private void CheckForLeap()
        {
            // Player must be grounded
            if (!Grounded)
                return;

            // Player must press jump
            if (!_input.jump)
                return;

            // Player must be moving
            if (_input.move.magnitude < LeapInputThreshold)
                return;

            StartLeap();
        }

        // ============================================================
        // START LEAP
        // ============================================================

        private void StartLeap()
        {
            _isLeaping = true;

            _leapTimer = 0.0f;

            _leapStartPosition = transform.position;

            // Leap in player's current forward direction
            _leapDirection = transform.forward;

            // Remove normal vertical velocity
            _verticalVelocity = 0.0f;

            // Trigger jump animation
            if (_hasAnimator)
            {
                _animator.SetBool(
                    _animIDJump,
                    true
                );

                _animator.SetBool(
                    _animIDGrounded,
                    false
                );
            }

            // Consume jump input
            _input.jump = false;
        }

        // ============================================================
        // HANDLE LEAP
        // ============================================================

        private void HandleLeap()
        {
            _leapTimer += Time.deltaTime;

            float normalizedTime =
                Mathf.Clamp01(
                    _leapTimer /
                    LeapDuration
                );

            // --------------------------------------------------------
            // Horizontal movement
            // --------------------------------------------------------

            Vector3 horizontalMovement =
                _leapDirection *
                (LeapDistance / LeapDuration) *
                Time.deltaTime;

            // --------------------------------------------------------
            // Parabolic vertical movement
            //
            // At t = 0      -> ground
            // At t = 0.5    -> maximum height
            // At t = 1      -> ground
            // --------------------------------------------------------

            float previousHeight =
                CalculateLeapHeight(
                    normalizedTime -
                    Time.deltaTime /
                    LeapDuration
                );

            float currentHeight =
                CalculateLeapHeight(
                    normalizedTime
                );

            float verticalMovement =
                currentHeight -
                previousHeight;

            _controller.Move(
                horizontalMovement +
                Vector3.up *
                verticalMovement
            );

            // --------------------------------------------------------
            // End leap
            // --------------------------------------------------------

            if (normalizedTime >= 1.0f)
            {
                _isLeaping = false;

                _verticalVelocity = -2.0f;

                if (_hasAnimator)
                {
                    _animator.SetBool(
                        _animIDJump,
                        false
                    );
                }
            }
        }

        // ============================================================
        // LEAP HEIGHT CURVE
        // ============================================================

        private float CalculateLeapHeight(
            float normalizedTime
        )
        {
            normalizedTime =
                Mathf.Clamp01(
                    normalizedTime
                );

            // Parabola:
            // 0 -> 1 -> 0
            return
                4.0f *
                LeapHeight *
                normalizedTime *
                (1.0f - normalizedTime);
        }

        // ============================================================
        // CLAMP ANGLE
        // ============================================================

        private static float ClampAngle(
            float lfAngle,
            float lfMin,
            float lfMax
        )
        {
            if (lfAngle < -360f)
                lfAngle += 360f;

            if (lfAngle > 360f)
                lfAngle -= 360f;

            return Mathf.Clamp(
                lfAngle,
                lfMin,
                lfMax
            );
        }

        // ============================================================
        // GIZMOS
        // ============================================================

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen =
                new Color(
                    0.0f,
                    1.0f,
                    0.0f,
                    0.35f
                );

            Color transparentRed =
                new Color(
                    1.0f,
                    0.0f,
                    0.0f,
                    0.35f
                );

            if (Grounded)
                Gizmos.color =
                    transparentGreen;
            else
                Gizmos.color =
                    transparentRed;

            Gizmos.DrawSphere(
                new Vector3(
                    transform.position.x,
                    transform.position.y -
                    GroundedOffset,
                    transform.position.z
                ),
                GroundedRadius
            );

            // Draw approximate leap path
            Gizmos.color = Color.yellow;

            Vector3 start =
                transform.position;

            Vector3 direction =
                transform.forward;

            Vector3 previousPoint =
                start;

            for (int i = 1; i <= 20; i++)
            {
                float t =
                    i / 20.0f;

                Vector3 point =
                    start +
                    direction *
                    (LeapDistance * t);

                point.y +=
                    CalculateLeapHeight(t);

                Gizmos.DrawLine(
                    previousPoint,
                    point
                );

                previousPoint = point;
            }
        }

        // ============================================================
        // FOOTSTEP
        // ============================================================

        private void OnFootstep(
            AnimationEvent animationEvent
        )
        {
            if (
                animationEvent.animatorClipInfo.weight
                > 0.5f
            )
            {
                float sfxVol =
                    PlayerPrefs.GetFloat(
                        "CyberRakshak.Sfx",
                        1f
                    );

                if (AudioFootsteps != null)
                {
                    AudioFootsteps.volume =
                        FootstepAudioVolume *
                        sfxVol;

                    AudioFootsteps.Play();
                }

                if (AudioFoley != null)
                {
                    AudioFoley.volume =
                        FootstepAudioVolume *
                        sfxVol;

                    AudioFoley.Play();
                }
            }
        }

        // ============================================================
        // LAND
        // ============================================================

        private void OnLand(
            AnimationEvent animationEvent
        )
        {
            if (
                animationEvent.animatorClipInfo.weight
                > 0.5f
            )
            {
                if (LandingAudio != null)
                {
                    float sfxVol =
                        PlayerPrefs.GetFloat(
                            "CyberRakshak.Sfx",
                            1f
                        );

                    LandingAudio.volume =
                        FootstepAudioVolume *
                        sfxVol;

                    LandingAudio.Play();
                }
            }
        }
    }
}

using UnityEngine;

namespace CyberRakshak.PATCH
{
    public sealed class PatchCompanion : MonoBehaviour
    {
        [Header("Follow")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 followOffset = new(1.25f, 1.8f, -1.1f);
        [SerializeField] private float followSmoothTime = 0.18f;

        [Header("Idle Motion")]
        [SerializeField] private float bobAmplitude = 0.12f;
        [SerializeField] private float bobFrequency = 2.4f;
        [SerializeField] private float turnSpeed = 10f;

        private Vector3 velocity;
        private Vector3 baseLocalPosition;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void Awake()
        {
            baseLocalPosition = transform.localPosition;
        }

        private void LateUpdate()
        {
            if (target != null)
            {
                FollowTarget();
            }
            else
            {
                ApplyIdleBob();
            }

            FaceCamera();
        }

        private void FollowTarget()
        {
            float bob = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            Vector3 desiredPosition = target.TransformPoint(followOffset) + Vector3.up * bob;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, followSmoothTime);
        }

        private void ApplyIdleBob()
        {
            float bob = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;

            transform.localPosition = baseLocalPosition + Vector3.up * bob;
        }

        private void FaceCamera()
        {
            Camera activeCamera = Camera.main;
            if (activeCamera == null)
            {
                return;
            }

            Vector3 direction = transform.position - activeCamera.transform.position;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }
}

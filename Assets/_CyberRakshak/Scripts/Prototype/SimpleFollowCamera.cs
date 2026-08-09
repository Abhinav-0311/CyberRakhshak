using UnityEngine;

namespace CyberRakshak.Prototype
{
    public sealed class SimpleFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new(0f, 4.2f, -6.4f);
        [SerializeField] private float followSmooth = 8f;
        [SerializeField] private float lookHeight = 1.2f;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.position + target.TransformDirection(offset);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmooth * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * lookHeight);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}


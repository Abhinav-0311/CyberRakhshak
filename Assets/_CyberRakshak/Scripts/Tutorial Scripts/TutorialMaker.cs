using UnityEngine;

public class TutorialMarker : MonoBehaviour
{
    public Transform player;

    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        gameObject.SetActive(true);
    }

    public void HideMarker()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (target == null || player == null)
            return;

        Vector3 direction = target.position - player.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }
    }
}

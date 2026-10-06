using UnityEngine;

public class TutorialMovementTrigger : MonoBehaviour
{
    public float requiredDistance = 3f;

    private Vector3 startPosition;
    private bool completed = false;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (completed)
            return;

        float distance =
            Vector3.Distance(transform.position, startPosition);

        if (distance >= requiredDistance)
        {
            completed = true;

            TutorialManager.Instance.CompleteCurrentStep();
        }
    }
}

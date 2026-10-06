using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Tutorial UI")]
    public TextMeshProUGUI instructionText;

    [Header("Directional Marker")]
    public TutorialMarker marker;

    [Header("Tutorial Steps")]
    public TutorialStep[] steps;

    private int currentStep = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartStep(0);
    }

    public void StartStep(int stepIndex)
    {
        if (stepIndex >= steps.Length)
        {
            CompleteTutorial();
            return;
        }

        currentStep = stepIndex;

        TutorialStep step = steps[currentStep];

        instructionText.text = step.instruction;

        if (marker != null)
        {
            marker.SetTarget(step.target);
        }
    }

    public void CompleteCurrentStep()
    {
        StartStep(currentStep + 1);
    }

    private void CompleteTutorial()
    {
        instructionText.text = "Tutorial Complete!";

        if (marker != null)
            marker.HideMarker();

        Debug.Log("Tutorial Completed!");
    }

    public int GetCurrentStep()
    {
        return currentStep;
    }
}

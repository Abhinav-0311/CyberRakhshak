using UnityEngine;

[System.Serializable]
public class TutorialStep
{
    [TextArea]
    public string instruction;

    public Transform target;
}

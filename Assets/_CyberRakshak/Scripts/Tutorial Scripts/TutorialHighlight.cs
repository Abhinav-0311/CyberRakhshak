using UnityEngine;
public class TutorialHighlight :MonoBehaviour
{
    public GameObject highlightObject;

    public void EnableHighlight()
    {
        highlightObject.SetActive(true);
    }

    public void DisableHighlight()
    {
        highlightObject.SetActive(false);
    }
}

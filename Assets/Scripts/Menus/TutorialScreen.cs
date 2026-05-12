using UnityEngine;

public class TutorialScreen : MonoBehaviour
{
    public void OpenTutorialMenu()
    {
        gameObject.SetActive(true);
    }

    public void CloseTutorialMenu()
    {
        gameObject.SetActive(false);
    }
}

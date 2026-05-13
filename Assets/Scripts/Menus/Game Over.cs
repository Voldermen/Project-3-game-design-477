using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public GameObject GameOverUI;
    public GameObject ButtonCanvas;
    public GameObject TimelineUI;
    public GameObject PileStuff;

    private void Start()
    {
        GameOverUI.SetActive(false);
        inputedActions.Instance.EnableInput();
    }

    public void ShowGameOver()
    {
        GameOverUI.SetActive(true);
        inputedActions.Instance.DisableInput();
        ButtonCanvas.SetActive(false);
        PileStuff.SetActive(false);
        TimelineUI.SetActive(false);
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        ButtonCanvas.SetActive(true);
        PileStuff.SetActive(true);
        TimelineUI.SetActive(true);
    }
    public void Title()
    {
        SceneManager.LoadScene("Title");
    }
}

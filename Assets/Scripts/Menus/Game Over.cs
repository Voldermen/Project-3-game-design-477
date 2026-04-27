using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public GameObject GameOverUI;

    private void Start()
    {
        GameOverUI.SetActive(false);
        inputedActions.Instance.EnableInput();
    }

    public void ShowGameOver()
    {
        GameOverUI.SetActive(true);
        inputedActions.Instance.DisableInput();
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void Title()
    {
        SceneManager.LoadScene("Title");
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
public class StartGame : MonoBehaviour
{
  public void Startgame()
    {
        Time.timeScale=1f;
        SceneManager.LoadScene("BoardScene");
    }
    public void exit()
    {
        Application.Quit();
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
public class StartGame : MonoBehaviour
{
  public void Startgame()
    {
        SceneManager.LoadScene("SampleScene");
    }
    public void exit()
    {
        Application.Quit();
    }
}

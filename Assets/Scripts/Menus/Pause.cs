using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    private bool isPaused= false;
    public GameObject PauseUI;

    private void Start()
    {
        PauseUI.SetActive(false);
        inputedActions.Instance.EnableInput();
    }

    public void GamePause()
    {
        isPaused=true;
        Time.timeScale=0f;
        PauseUI.SetActive(true);
        inputedActions.Instance.DisableInput();
    }

    public void Resume()
    {
        isPaused=false;
        Time.timeScale=1f;
        PauseUI.SetActive(false);
        inputedActions.Instance.EnableInput();
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale=1f;
        
    }
    public void Title()
    {
        Time.timeScale=1f;
        SceneManager.LoadScene("Title");
    }
private void Update(){
    if (inputedActions.Instance.input.Pause.IsPressed()){
        Debug.Log("pause");
        GamePause();
    }
}
} 

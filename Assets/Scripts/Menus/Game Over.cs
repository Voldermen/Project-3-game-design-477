using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class GameOver : MonoBehaviour
{
    public GameObject GameOverUI;
    public GameObject ButtonCanvas;
    public GameObject TimelineUI;
    public GameObject PileStuff;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private ScoreUI scoreUI;
   

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
        if (finalScoreText != null && ScoreManager.Instance != null)
        {
            finalScoreText.text= $"Final Score: {ScoreManager.Instance.FinalScore}";
        }
        if (scoreUI != null)
        {
           scoreUI.RefreshScore(); 
        }
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

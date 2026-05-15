using UnityEngine;
using HighScore;
using TMPro;

public class ScoreUI : MonoBehaviour // this will allow the player to submit their score if they beat the game.
{

    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_Text finalScoreText;

    private int finalScore;
    private void OnEnable()
    {
        RefreshScore();
    }
    public void RefreshScore()
    {
       if (ScoreManager.Instance != null)
        {
            finalScore= ScoreManager.Instance.FinalScore;
        }
        else
        {
            finalScore= 0;
        }

        if (finalScoreText != null)
        {
            finalScoreText.text= $"Final Score {finalScore}";
        }
    }

    public void submitScore()
    {
        RefreshScore();
        string playerName= "Player";

        if (playerNameInput !=null && !string.IsNullOrWhiteSpace(playerNameInput.text))
        {
            playerName= playerNameInput.text;
        }

        HS.SubmitHighScore(this, playerName, finalScore);
        Debug.Log($"Submitted Score {finalScore} for {playerName}");
    }
}

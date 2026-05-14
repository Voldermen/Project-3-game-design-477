using UnityEngine;
using HighScore;
using TMPro;

public class ScoreUI : MonoBehaviour // this will allow the player to submit their score if they beat the game.
{

    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_Text finalScoreText;

    private int finalScore;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
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
        string playerName= "Player";

        if (playerNameInput !=null && !string.IsNullOrWhiteSpace(playerNameInput.text))
        {
            playerName= playerNameInput.text;
        }

        HS.SubmitHighScore(this, playerName, finalScore);
        Debug.Log($"Submitted Score {finalScore} for {playerName}");
    }
}

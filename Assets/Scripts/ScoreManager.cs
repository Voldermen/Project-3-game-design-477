using UnityEngine;

public class ScoreManager : MonoBehaviour
{
   public static ScoreManager Instance {get; private set;}

   [Header("Using Card Score")]
   [SerializeField] private int startingScore=1000;
   [SerializeField] private int cardUsePenalty= 25;
   [SerializeField] private int minCardScore=0;
   [SerializeField] private int gameOverPenalty= 1000;

   [Header("Base HP Average Score ")]
   [SerializeField] private int HPMultiplier= 10; // makes the score look larger.


   public int CardScore { get; private set;}
   public int FinalScore {get; private set;}
   public int CollectibleScore{get; private set;}

   private void Awake()
    {
        if (Instance != null && Instance != this) // this should keep the score manager alive between scenes.
        {
            Destroy(gameObject);
            return;
        }

        Instance=this;
        DontDestroyOnLoad(gameObject);
        ResetScore();
    }

    public void ResetScore()
    {
        CardScore= startingScore;
        FinalScore= 0;
        CollectibleScore=0;
    }


    public void CardUsed()
    {
        CardScore -= cardUsePenalty;

        if (CardScore< minCardScore)
        {
            CardScore = minCardScore;
        }
        Debug.Log($"Card used. CardScore is now {CardScore}");
    }

    public int CalculateBaseHPScore(GameManager gameManager)
    {
        if (gameManager == null)
        {
            return 0;
        }

        float averageBaseHP= gameManager.GetAverageBaseHP();
        int baseHPScore= Mathf.RoundToInt(averageBaseHP*HPMultiplier);
        Debug.Log($"Average Base HP= {averageBaseHP}, Base HP Score= {baseHPScore}");
        return baseHPScore;
    }

    public int CalculateFinalScore(GameManager gameManager)
    {
        int baseHPScore= CalculateBaseHPScore(gameManager);
        FinalScore= baseHPScore + CardScore+ CollectibleScore;
        Debug.Log($"Final Score= BaseHPScore({baseHPScore})+ CardScore({CardScore}) + CollectibleScore({CollectibleScore})= {FinalScore}");
        return FinalScore;
    }
    public int CalculateScoreGameOver()
    {
        int penalizedCardScore= Mathf.Max(0,CardScore- gameOverPenalty);

        FinalScore= penalizedCardScore+ CollectibleScore;

        Debug.Log($"Game Over your score= BaseHPScore(0)+ CardScore({penalizedCardScore})+ CollectibleScore({CollectibleScore})={FinalScore}");

        return FinalScore;
    }

    public void AddCollectibleScore(int amount)
    {
        CollectibleScore += Mathf.Max(0, amount);
        Debug.Log($"CollectibleScore is now {CollectibleScore}");
    }
}

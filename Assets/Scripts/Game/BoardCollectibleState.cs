using UnityEngine;
[System.Serializable]
public class BoardCollectibleState 
{
   public int CollectibleId;
   public Vector2Int Position;
   public int ScoreValue=100;

   public BoardCollectibleState Clone()
   {
      return new BoardCollectibleState
      {
         CollectibleId=CollectibleId,
         Position=Position,
         ScoreValue= ScoreValue
      };
   }
}

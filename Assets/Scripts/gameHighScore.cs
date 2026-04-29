using HighScore;
using TMPro;
using UnityEngine;

public class gameHighScore : MonoBehaviour {
  public TMP_InputField inputGameName;
  public TMP_InputField inputPlayerName;
  public TMP_InputField inputScore;

  public void InitGame() {
    // Read the comment for this HS.Init method .
    // This method call should be put into the Start method of some script that runs
    // in the title menu. Call it once when the game launches and you don't have to
    // call it again. If you want to launch the game from another scene, you can 
    // call this method again in the Start method of any script in that scene, since
    // calling this method multiple times doesn't hurt anything.
    HS.Init(this, inputGameName.text); // you can hard code your game's name
  }

  public void SubmitScore() {
    // Read the comment for this HS.SubmitHighScore method.
    // This method call should be made once the player has earned a score (ANY score).
    // Ask the player to input their name, then call this method. The method will determine
    // whether this is actually a "HIGH" score or not, so you don't have to worry about that.
    // The first argument should be the "this" pointer. Just make sure this method is called
    // in a script that inherits from MonoBehaviour (i.e. is a normal script that you attach
    // to a game object)
    HS.SubmitHighScore(this, inputPlayerName.text, int.Parse(inputScore.text));
  }

  public void ClearScores() {
    HS.Clear(this);
  }
}

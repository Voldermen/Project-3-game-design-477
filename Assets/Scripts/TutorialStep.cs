using UnityEngine;
using UnityEngine.Video;
[System.Serializable]
public class TutorialStep
{
   public VideoClip videoClip;

   [TextArea(3,8)]
   public string tutorialText;
}

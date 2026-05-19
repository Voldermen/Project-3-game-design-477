using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialSequenceUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextButtonText;

    [Header("Tutorial Steps")]
    [SerializeField] private TutorialStep[] tutorialSteps;

    [Header("UI To Hide During Tutorial")]
    [SerializeField] private GameObject[] uiToHideDuringTutorial;

    private int currentIndex;
    private System.Action onFinished;

    private void Awake()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextStep);
        }

        if (root != null)
        {
            root.SetActive(false);
        }
    }

    public void Open(System.Action finishedCallback)
    {
        onFinished = finishedCallback;
        currentIndex = 0;

        SetOtherUIVisible(false);

        if (root != null)
        {
            root.SetActive(true);
        }

        ShowCurrentStep();
    }

    private void ShowCurrentStep()
    {
        if (tutorialSteps == null || tutorialSteps.Length == 0)
        {
            FinishTutorial();
            return;
        }

        if (currentIndex < 0 || currentIndex >= tutorialSteps.Length)
        {
            FinishTutorial();
            return;
        }

        TutorialStep step = tutorialSteps[currentIndex];

        if (tutorialText != null)
        {
            tutorialText.text = step.tutorialText;
        }

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.clip = step.videoClip;
            videoPlayer.isLooping = true;
            videoPlayer.Play();
        }

        if (nextButtonText != null)
        {
            bool lastStep = currentIndex == tutorialSteps.Length - 1;
            nextButtonText.text = lastStep ? "Start Game" : "Next";
        }
    }

    private void NextStep()
    {
        currentIndex++;

        if (currentIndex >= tutorialSteps.Length)
        {
            FinishTutorial();
            return;
        }

        ShowCurrentStep();
    }

    private void FinishTutorial()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        if (root != null)
        {
            root.SetActive(false);
        }

        SetOtherUIVisible(true);

        onFinished?.Invoke();
        onFinished = null;
    }

    private void SetOtherUIVisible(bool visible)
    {
        if (uiToHideDuringTutorial == null)
        {
            return;
        }

        for (int i = 0; i < uiToHideDuringTutorial.Length; i++)
        {
            if (uiToHideDuringTutorial[i] != null)
            {
                uiToHideDuringTutorial[i].SetActive(visible);
            }
        }
    }
}
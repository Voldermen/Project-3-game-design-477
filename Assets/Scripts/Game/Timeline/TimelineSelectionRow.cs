using TMPro;
using UnityEngine;

public class TimelineSelectionRow : MonoBehaviour
{
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private Transform stateButtonRoot;
    [SerializeField] private TimelineStateButton stateButtonPrefab;

    public void Render(TimelineSelectionWidget owner, Timeline timeline)
    {
        if (labelText != null)
        {
            labelText.text = $"Timeline {timeline.TimelineId}";
        }

        for (int i = stateButtonRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(stateButtonRoot.GetChild(i).gameObject);
        }

        for (int i = 0; i < timeline.StateCount; i++)
        {
            BoardState state = timeline.GetStateAtIndex(i);
            TimelineStateButton button = Instantiate(stateButtonPrefab, stateButtonRoot, false);
            button.Initialize(owner, timeline.TimelineId, i, state);
        }
    }
}
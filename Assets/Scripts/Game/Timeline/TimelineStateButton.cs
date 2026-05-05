using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimelineStateButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text labelText;

    private TimelineSelectionWidget owner;
    private int timelineId;
    private int stateIndex;

    public void Initialize(TimelineSelectionWidget newOwner, int newTimelineId, int newStateIndex, BoardState state)
    {
        owner = newOwner;
        timelineId = newTimelineId;
        stateIndex = newStateIndex;

        if (labelText != null)
        {
            labelText.text = state == null ? "NULL" : $"T{state.TurnCount}";
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(Click);
        }
    }

    private void Click()
    {
        if (owner != null)
        {
            owner.SelectState(timelineId, stateIndex);
        }
    }
}
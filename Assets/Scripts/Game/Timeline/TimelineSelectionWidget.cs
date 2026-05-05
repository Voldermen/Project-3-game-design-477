using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimelineSelectionWidget : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject graphRoot;
    [SerializeField] private Transform rowRoot;
    [SerializeField] private TimelineSelectionRow rowPrefab;
    [SerializeField] private GameObject previewRoot;
    [SerializeField] private Button startTurnButton;
    [SerializeField] private Button backButton;

    private GameManager gameManager;
    private readonly List<TimelineSelectionRow> activeRows = new();
    private int selectedTimelineId = -1;
    private int selectedStateIndex = -1;

    private void Awake()
    {
        if (startTurnButton != null)
        {
            startTurnButton.onClick.RemoveAllListeners();
            startTurnButton.onClick.AddListener(StartTurn);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(BackToGraph);
        }

        Hide();
    }

    public void Open(GameManager newGameManager)
    {
        gameManager = newGameManager;
        selectedTimelineId = -1;
        selectedStateIndex = -1;

        if (root != null)
        {
            root.SetActive(true);
        }

        ShowGraph();
        RenderGraph();
    }

    public void Hide()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
    }

    private void RenderGraph()
    {
        ClearRows();

        List<Timeline> timelines = gameManager.GetTimelines();

        for (int i = 0; i < timelines.Count; i++)
        {
            TimelineSelectionRow row = Instantiate(rowPrefab, rowRoot, false);
            row.Render(this, timelines[i]);
            activeRows.Add(row);
        }
    }

    public void SelectState(int timelineId, int stateIndex)
    {
        Debug.Log($"Timeline UI selected timeline {timelineId}, state index {stateIndex}");

        selectedTimelineId = timelineId;
        selectedStateIndex = stateIndex;

        BoardState state = gameManager.GetTimelineState(timelineId, stateIndex);

        if (state == null)
        {
            Debug.LogError("Selected timeline state was null.");
            return;
        }

        Debug.Log($"Previewing state: turn={state.TurnCount}, timeline={state.TimelineId}, units={state.UnitsById.Count}");

        gameManager.PreviewTimelineState(state);

        graphRoot.SetActive(false);
        previewRoot.SetActive(true);
        startTurnButton.gameObject.SetActive(gameManager.IsRightmostTimelineState(timelineId, stateIndex));
    }

    private void StartTurn()
    {
        if (selectedTimelineId < 0)
        {
            return;
        }

        if (!gameManager.IsRightmostTimelineState(selectedTimelineId, selectedStateIndex))
        {
            return;
        }

        Hide();
        gameManager.SelectTimelineForTurn(selectedTimelineId);
    }

    private void BackToGraph()
    {
        ShowGraph();
        gameManager.RenderCommittedBoard();
    }

    private void ShowGraph()
    {
        if (graphRoot != null)
        {
            graphRoot.SetActive(true);
        }

        if (previewRoot != null)
        {
            previewRoot.SetActive(false);
        }

        if (startTurnButton != null)
        {
            startTurnButton.gameObject.SetActive(false);
        }
    }

    private void ClearRows()
    {
        for (int i = activeRows.Count - 1; i >= 0; i--)
        {
            if (activeRows[i] != null)
            {
                Destroy(activeRows[i].gameObject);
            }
        }

        activeRows.Clear();
    }
}
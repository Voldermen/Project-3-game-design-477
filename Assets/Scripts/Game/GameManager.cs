using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BoardRepresentative boardRepresentative;
    [SerializeField] private CardManager cardManager;

    [SerializeField] private int boardWidth = 8;
    [SerializeField] private int boardHeight = 8;

    private readonly List<Timeline> timelines = new();
    private readonly CardResolver cardResolver = new();

    private Timeline activeTimeline;
    private BoardState committedBoardState;
    private BoardState workingBoardState;

    private int nextTimelineId;
    private int nextUnitId;

    public TurnPhase CurrentPhase { get; private set; }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartMatch();
    }

    // Called when a GameManager is made, creates a startingstate and sets up the timelines
    public void StartMatch()
    {
        CurrentPhase = TurnPhase.Setup;

        timelines.Clear();

        nextTimelineId = 0;
        nextUnitId = 0;

        BoardState startingState = new BoardState(boardWidth, boardHeight, nextTimelineId, 0);

        ResolveOpeningEnemyTurn(startingState);

        Timeline timeline = new Timeline(nextTimelineId, 0);
        timeline.AddState(startingState);

        timelines.Add(timeline);

        activeTimeline = timeline;
        committedBoardState = startingState;
        workingBoardState = null;

        boardRepresentative.Render(committedBoardState);

        StartPlayerTimelineSelection();
        print("StartMatch cleared");
    }

    // Draws cards, renders the current committed board, and waits for the player to choose a timeline
    public void StartPlayerTimelineSelection()
    {
        CurrentPhase = TurnPhase.TimelineSelection;

        committedBoardState = activeTimeline.GetLatestState();
        workingBoardState = null;

        cardManager.DrawHand(5);

        boardRepresentative.Render(committedBoardState);
    }

    // Set the active timeline to the given timeLineId, and updates relevant fields
    public bool SelectTimelineForTurn(int timelineId)
    {
        if (CurrentPhase != TurnPhase.TimelineSelection) return false;

        Timeline timeline = GetTimeline(timelineId);
        if (timeline == null) return false;

        activeTimeline = timeline;
        committedBoardState = activeTimeline.GetLatestState();
        if (committedBoardState == null) return false;

        workingBoardState = committedBoardState.CloneForNextTurn();
        workingBoardState.RefreshEnergyFromFriendlyUnits();

        CurrentPhase = TurnPhase.PlayerTurn;

        boardRepresentative.Render(workingBoardState);
        return true;
    }

    // Commits the working state, checks for a match end condition, and starts enemy turn
    public void EndPlayerTurn()
    {
        if (CurrentPhase != TurnPhase.PlayerTurn) return;
        if (workingBoardState == null) return;

        if (ShouldLose(workingBoardState))
        {
            LoseMatch();
            return;
        }

        if (ShouldWin(workingBoardState))
        {
            CommitWorkingBoardState();
            WinMatch();
            return;
        }

        StartEnemyTurn();
    }

    private void StartEnemyTurn()
    {
        CurrentPhase = TurnPhase.EnemyResolution;

        ResolveEnemyTurn(workingBoardState);

        boardRepresentative.Render(workingBoardState);

        if (ShouldLose(workingBoardState))
        {
            LoseMatch();
            return;
        }

        if (ShouldWin(workingBoardState))
        {
            CommitWorkingBoardState();
            WinMatch();
            return;
        }

        CommitWorkingBoardState();
        StartPlayerTimelineSelection();
    }

    // We resolve the tail of the previous turn, then move and set up the next one
    private void ResolveEnemyTurn(BoardState state)
    {
        ExecuteTelegraphedEnemyAttacks(state);
        MoveEnemies(state);
        TelegraphEnemyAttacks(state);
    }

    // Called only once on match start
    private void ResolveOpeningEnemyTurn(BoardState state)
    {
        MoveEnemies(state);
        TelegraphEnemyAttacks(state);
    }

    private void ExecuteTelegraphedEnemyAttacks(BoardState state)
    {
    }

    private void MoveEnemies(BoardState state)
    {
    }

    private void TelegraphEnemyAttacks(BoardState state)
    {
    }

    // Adds the working board state to the timeline as a real state, and renders it
    private void CommitWorkingBoardState()
    {
        activeTimeline.AddState(workingBoardState);
        committedBoardState = workingBoardState;
        workingBoardState = null;

        boardRepresentative.Render(committedBoardState);
    }

    public BoardState GetWorkingBoardState()
    {
        return workingBoardState;
    }

    public BoardState GetVisibleBoardState()
    {
        return workingBoardState ?? committedBoardState;
    }

    public bool TryPlayCard(CardDefinition card, int actingUnitId, Vector2Int targetPosition)
    {
        if (CurrentPhase != TurnPhase.PlayerTurn) return false;
        if (workingBoardState == null || card == null) return false;

        if (!workingBoardState.EnergyState.TrySpend(card.Cost)) return false;

        bool resolved = cardResolver.ResolveCard(card, workingBoardState, actingUnitId, targetPosition);

        if (!resolved)
        {
            workingBoardState.EnergyState.Refund(card.Cost);
            return false;
        }

        boardRepresentative.Render(workingBoardState);
        return true;
    }

    public bool TryMoveUnit(int unitId, int x, int y)
    {
        if (CurrentPhase != TurnPhase.PlayerTurn) return false;
        if (workingBoardState == null) return false;

        bool moved = workingBoardState.MoveUnit(unitId, x, y);

        if (moved) boardRepresentative.Render(workingBoardState);
        return moved;
    }

    public bool TryDamageUnit(int unitId, int damage)
    {
        if (CurrentPhase != TurnPhase.PlayerTurn) return false;
        if (workingBoardState == null) return false;

        // Do nothing if we can't find the UnitState by id
        if (!workingBoardState.UnitsById.TryGetValue(unitId, out BoardUnitState unit)) return false;

        unit.Health -= Mathf.Max(0, damage);

        if (unit.Health <= 0)
        {
            workingBoardState.RemoveUnit(unitId);
        }

        boardRepresentative.Render(workingBoardState);
        return true;
    }

    public Timeline BranchTimelineFromTurn(int sourceTimelineId, int sourceTurn)
    {
        // First see if we can get the timeline and boardstate
        Timeline sourceTimeline = GetTimeline(sourceTimelineId);
        if (sourceTimeline == null) return null;
        BoardState sourceState = sourceTimeline.GetStateAtTurn(sourceTurn);
        if (sourceState == null) return null;

        // Setup the new timeline id
        int timelineId = ++nextTimelineId;

        // Make the branchedState and set it's timeline to match
        BoardState branchedState = sourceState.Clone();
        branchedState.TimelineId = timelineId;

        // Make the new timeline using the id and sourceTurn
        Timeline newTimeline = new Timeline(timelineId, sourceTurn);
        newTimeline.AddState(branchedState);

        // Keep track of the new timeline
        timelines.Add(newTimeline);
        return newTimeline;
    }

    // We store units as just a bunch of data rather than literal actors for ease of duplication
    // This function just assembles a unit's data and returns it
    public BoardUnitState CreateUnit(UnitTeam team, int maxHealth, bool isBase, Vector2Int position)
    {
        BoardUnitState unit = new BoardUnitState
        {
            UnitId = nextUnitId++,
            Team = team,
            MaxHealth = maxHealth,
            Health = maxHealth,
            IsBase = isBase,
            Position = position
        };

        return unit;
    }

    // Dynamically adds a unit to specically the WORKING state
    public void AddUnitToWorkingState(BoardUnitState unit)
    {
        if (CurrentPhase != TurnPhase.PlayerTurn) return;
        if (workingBoardState == null) return;

        workingBoardState.AddUnit(unit, unit.Position.x, unit.Position.y);
        boardRepresentative.Render(workingBoardState);
    }

    // Finds the TimeLine of a given id
    public Timeline GetTimeline(int timelineId)
    {
        for (int i = 0; i < timelines.Count; i++)
        {
            if (timelines[i].TimelineId == timelineId)
            {
                return timelines[i];
            }
        }

        return null;
    }

    public List<Timeline> GetTimelines()
    {
        return timelines;
    }

    private bool ShouldLose(BoardState activeTimelineOverride)
    {
        for (int i = 0; i < timelines.Count; i++)
        {
            BoardState state = timelines[i] == activeTimeline && activeTimelineOverride != null
                ? activeTimelineOverride
                : timelines[i].GetLatestState();

            if (state == null)
            {
                continue;
            }

            if (!state.HasLivingBase(UnitTeam.Friendly))
            {
                return true;
            }
        }

        return false;
    }

    private bool ShouldWin(BoardState activeTimelineOverride)
    {
        for (int i = 0; i < timelines.Count; i++)
        {
            BoardState state = timelines[i] == activeTimeline && activeTimelineOverride != null
                ? activeTimelineOverride
                : timelines[i].GetLatestState();

            if (state == null)
            {
                continue;
            }

            if (state.HasLivingEnemies())
            {
                return false;
            }
        }

        return true;
    }

    private void WinMatch()
    {
        CurrentPhase = TurnPhase.GameOver;
        Debug.Log("Victory");
    }

    private void LoseMatch()
    {
        CurrentPhase = TurnPhase.GameOver;
        Debug.Log("Defeat");
    }
}
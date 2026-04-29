using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BoardRepresentative boardRepresentative;
    [SerializeField] private CardManager cardManager;

    [SerializeField] private int boardWidth = 8;
    [SerializeField] private int boardHeight = 8;

    private readonly List<Timeline> timelines = new();

    private Timeline activeTimeline;
    private BoardState committedBoardState;
    private BoardState workingBoardState;

    private int nextTimelineId;
    private int nextUnitId;

    private void Start()
    {
        StartMatch();
    }

    // Called when a GameManager is made, creates a startingstate and sets up the timelines
    public void StartMatch()
    {
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

        StartPlayerTurn();
    }

    // Submits the "current" state of the active timeline, then clones that state to form the working state
    // Draws cards and renders the working state
    public void StartPlayerTurn()
    {
        committedBoardState = activeTimeline.GetLatestState();
        workingBoardState = committedBoardState.CloneForNextTurn();

        cardManager.DrawHand(5);

        boardRepresentative.Render(workingBoardState);
    }

    // Set the active timeline to the given timeLineId, and updates relevant fields
    public void SelectTimeline(int timelineId)
    {
        Timeline timeline = GetTimeline(timelineId);

        if (timeline == null)
        {
            return;
        }

        activeTimeline = timeline;
        committedBoardState = activeTimeline.GetLatestState();
        workingBoardState = committedBoardState.CloneForNextTurn();

        boardRepresentative.Render(workingBoardState);
    }

    // Commits the working state, checks for a match end condition, and starts enemy turn
    public void EndPlayerTurn()
    {
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
        StartPlayerTurn();
    }

    // TODO: Figure out how we're gonna do this
    private void ResolveEnemyTurn(BoardState state)
    {
        ExecuteTelegraphedEnemyAttacks(state);
        MoveEnemies(state);
        TelegraphEnemyAttacks(state);
    }

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

    public bool TryMoveUnit(int unitId, int x, int y)
    {
        if (workingBoardState == null)
        {
            return false;
        }

        bool moved = workingBoardState.MoveUnit(unitId, x, y);

        if (moved)
        {
            boardRepresentative.Render(workingBoardState);
        }

        return moved;
    }

    public bool TryDamageUnit(int unitId, int damage)
    {
        if (workingBoardState == null)
        {
            return false;
        }

        if (!workingBoardState.UnitsById.TryGetValue(unitId, out BoardUnitState unit))
        {
            return false;
        }

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
        Timeline sourceTimeline = GetTimeline(sourceTimelineId);

        if (sourceTimeline == null)
        {
            return null;
        }

        BoardState sourceState = sourceTimeline.GetStateAtTurn(sourceTurn);

        if (sourceState == null)
        {
            return null;
        }

        int timelineId = ++nextTimelineId;

        BoardState branchedState = sourceState.Clone();
        branchedState.TimelineId = timelineId;

        Timeline newTimeline = new Timeline(timelineId, sourceTurn);
        newTimeline.AddState(branchedState);

        timelines.Add(newTimeline);

        return newTimeline;
    }

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

    public void AddUnitToWorkingState(BoardUnitState unit)
    {
        if (workingBoardState == null)
        {
            return;
        }

        workingBoardState.AddUnit(unit, unit.Position.x, unit.Position.y);
        boardRepresentative.Render(workingBoardState);
    }

    private Timeline GetTimeline(int timelineId)
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

    private bool ShouldLose()
    {
        return ShouldLose(null);
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

    private bool ShouldWin()
    {
        return ShouldWin(null);
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
        Debug.Log("Victory");
    }

    private void LoseMatch()
    {
        Debug.Log("Defeat");
    }
}
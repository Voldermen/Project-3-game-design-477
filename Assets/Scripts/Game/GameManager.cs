using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BoardRepresentative boardRepresentative;
    [SerializeField] private CardManager cardManager;
    [SerializeField] private EnergyWidget energyWidget;
    [SerializeField] private UnitDefinition friendlyUnitDefinition;
    [SerializeField] private UnitDefinition friendlyBaseDefinition;
    [SerializeField] private List<UnitDefinition> enemyDefinitions;
    [SerializeField] private UnitDatabase unitDatabase;

    [SerializeField] private int boardWidth = 8;
    [SerializeField] private int boardHeight = 8;

    [SerializeField] private int enemiesToSpawn = 3;
    [SerializeField] private int friendlyUnitsToPlace = 3;
    [SerializeField] private bool randomlyPlaceBase = true;
    [SerializeField] private float enemyActionDelay = 0.35f;

    private int placedFriendlyUnits;
    private BoardState setupBoardState;

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
        placedFriendlyUnits = 0;

        setupBoardState = new BoardState(boardWidth, boardHeight, nextTimelineId, 0);

        PlaceRandomEnemies(setupBoardState, enemiesToSpawn);
        if (randomlyPlaceBase) PlaceRandomBase(setupBoardState);
        

        CurrentPhase = TurnPhase.PlayerPlacement;

        committedBoardState = setupBoardState;
        workingBoardState = setupBoardState;

        boardRepresentative.Render(setupBoardState);
        if (energyWidget != null) energyWidget.Hide();

    }

    private void PlaceRandomEnemies(BoardState boardState, int count)
    {
        int placed = 0;
        int safety = 0;

        while (placed < count && safety < 1000)
        {
            safety++;

            int x = Random.Range(0, boardState.Width);
            int y = Random.Range(0, boardState.Height);

            if (boardState.GetUnitAtTile(x, y) != null)
            {
                continue;
            }

            UnitDefinition def = enemyDefinitions[Random.Range(0, enemyDefinitions.Count)];
            Vector2Int position = new Vector2Int(x, y);
            BoardUnitState unit = CreateUnit(def, position);
            boardState.AddUnit(unit, x, y);

            placed++;
        }
    }

    private void PlaceRandomBase(BoardState boardState)
    {
        int safety = 0;

        while (safety < 1000)
        {
            safety++;

            int x = Random.Range(0, boardState.Width);
            int y = Random.Range(0, boardState.Height);

            if (boardState.GetUnitAtTile(x, y) != null)
            {
                continue;
            }

            Vector2Int position = new Vector2Int(x, y);
            BoardUnitState baseUnit = CreateUnit(friendlyBaseDefinition, position);
            boardState.AddUnit(baseUnit, x, y);
            return;
        }

        Debug.LogWarning("Failed to place base.");
    }

    public bool TryPlaceFriendlyUnit(int x, int y)
    {
        if (CurrentPhase != TurnPhase.PlayerPlacement)
        {
            return false;
        }

        if (setupBoardState == null)
        {
            return false;
        }

        if (!setupBoardState.IsInsideBoard(x, y))
        {
            return false;
        }

        if (setupBoardState.GetUnitAtTile(x, y) != null)
        {
            return false;
        }

        Vector2Int position = new Vector2Int(x, y);
        BoardUnitState unit = CreateUnit(friendlyUnitDefinition, position);
        setupBoardState.AddUnit(unit, x, y);
        placedFriendlyUnits++;

        boardRepresentative.Render(setupBoardState);

        if (placedFriendlyUnits >= friendlyUnitsToPlace)
        {
            BeginCombatFromSetup();
        }

        return true;
    }

    private void BeginCombatFromSetup()
    {
        ResolveOpeningEnemyTurn(setupBoardState);

        Timeline timeline = new Timeline(nextTimelineId, 0);
        timeline.AddState(setupBoardState);

        timelines.Add(timeline);

        activeTimeline = timeline;
        committedBoardState = setupBoardState;
        workingBoardState = null;
        setupBoardState = null;

        boardRepresentative.Render(committedBoardState);

        StartPlayerTimelineSelection();
    }

    // Draws cards, renders the current committed board, and waits for the player to choose a timeline
    public void StartPlayerTimelineSelection()
    {
        CurrentPhase = TurnPhase.TimelineSelection;

        committedBoardState = activeTimeline.GetLatestState();
        workingBoardState = null;

        cardManager.DrawHand(5);

        if (energyWidget != null)
        {
            energyWidget.Hide();
        }

        boardRepresentative.Render(committedBoardState);

        if (timelines.Count == 1)
        {
            SelectTimelineForTurn(timelines[0].TimelineId);
            return;
        }
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
        LogAllUnitHealth(workingBoardState, "Start of Player Turn");
        workingBoardState.RefreshEnergyFromFriendlyUnits();
        if (energyWidget != null) energyWidget.Show(workingBoardState.EnergyState);

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
        StartCoroutine(RunEnemyTurn());
    }

    private IEnumerator RunEnemyTurn()
    {
        CurrentPhase = TurnPhase.EnemyResolution;

        yield return new WaitForSeconds(enemyActionDelay);

        ExecuteTelegraphedEnemyAttacks(workingBoardState);
        boardRepresentative.Render(workingBoardState);

        yield return new WaitForSeconds(enemyActionDelay);

        MoveEnemies(workingBoardState);
        boardRepresentative.Render(workingBoardState);

        yield return new WaitForSeconds(enemyActionDelay);

        TelegraphEnemyAttacks(workingBoardState);
        boardRepresentative.Render(workingBoardState);

        yield return new WaitForSeconds(enemyActionDelay);

        if (ShouldLose(workingBoardState))
        {
            LoseMatch();
            yield break;
        }

        if (ShouldWin(workingBoardState))
        {
            CommitWorkingBoardState();
            WinMatch();
            yield break;
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
        for (int i = 0; i < state.EnemyIntents.Count; i++)
        {
            EnemyIntentState intent = state.EnemyIntents[i];

            if (!state.UnitsById.TryGetValue(intent.EnemyUnitId, out BoardUnitState enemy))
            {
                continue;
            }

            if (enemy.Team != UnitTeam.Enemy || enemy.Health <= 0)
            {
                continue;
            }

            if (intent.IntentType != EnemyIntentType.Damage)
            {
                continue;
            }

            for (int j = 0; j < intent.TargetTiles.Count; j++)
            {
                Vector2Int position = intent.TargetTiles[j];
                BoardUnitState target = state.GetUnitAtTile(position.x, position.y);

                if (target == null || target.Team != UnitTeam.Friendly)
                {
                    continue;
                }

                target.Health -= intent.Damage;

                if (target.Health <= 0)
                {
                    state.RemoveUnit(target.UnitId);
                }
            }
        }

        state.EnemyIntents.Clear();
    }

    private void MoveEnemies(BoardState state)
    {
        List<BoardUnitState> enemies = new();

        foreach (var pair in state.UnitsById)
        {
            BoardUnitState unit = pair.Value;

            if (unit.Team == UnitTeam.Enemy && unit.Health > 0)
            {
                enemies.Add(unit);
            }
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            BoardUnitState enemy = enemies[i];
            UnitDefinition definition = unitDatabase.GetDefinition(enemy.UnitDefinitionId);

            if (definition == null || definition.EnemyBehavior == null)
            {
                continue;
            }

            definition.EnemyBehavior.Move(state, enemy);
        }
    }

    private void TelegraphEnemyAttacks(BoardState state)
    {
        state.EnemyIntents.Clear();

        Debug.Log("TelegraphEnemyAttacks called");

        foreach (var pair in state.UnitsById)
        {
            BoardUnitState enemy = pair.Value;

            Debug.Log($"Checking unit {enemy.UnitId}: team={enemy.Team}, health={enemy.Health}, def={enemy.UnitDefinitionId}");

            if (enemy.Team != UnitTeam.Enemy || enemy.Health <= 0)
            {
                continue;
            }

            UnitDefinition definition = unitDatabase.GetDefinition(enemy.UnitDefinitionId);

            if (definition == null)
            {
                Debug.LogError($"No UnitDefinition found for {enemy.UnitDefinitionId}");
                continue;
            }

            if (definition.EnemyBehavior == null)
            {
                Debug.LogError($"Enemy definition {definition.DisplayName} has no EnemyBehavior assigned.");
                continue;
            }

            EnemyIntentState intent = definition.EnemyBehavior.CreateIntent(state, enemy);

            if (intent == null)
            {
                Debug.LogError($"Enemy behavior returned null intent for {enemy.UnitId}");
                continue;
            }

            Debug.Log($"Created intent with {intent.TargetTiles.Count} target tiles");

            state.EnemyIntents.Add(intent);
        }

        Debug.Log($"Final intent count: {state.EnemyIntents.Count}");
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

        cardManager.DiscardCard(card);
        if (energyWidget != null) energyWidget.Refresh(workingBoardState.EnergyState);
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
    public BoardUnitState CreateUnit(UnitDefinition def, Vector2Int pos)
    {
        return new BoardUnitState
        {
            UnitId = nextUnitId++,
            UnitDefinitionId = def.UnitDefinitionId,
            Team = def.Team,
            MaxHealth = def.MaxHealth,
            Health = def.MaxHealth,
            IsBase = def.IsBase,
            Position = pos
        };
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

    private void LogAllUnitHealth(BoardState state, string context)
    {
        Debug.Log($"--- {context} ---");

        foreach (var pair in state.UnitsById)
        {
            BoardUnitState unit = pair.Value;

            Debug.Log(
                $"Unit {unit.UnitId} ({unit.UnitDefinitionId}) | Team={unit.Team} | HP={unit.Health}/{unit.MaxHealth}"
            );
        }
    }
}
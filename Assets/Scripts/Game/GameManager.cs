using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using  UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BoardRepresentative boardRepresentative;
    [SerializeField] private CardManager cardManager;
    [SerializeField] private EnergyWidget energyWidget;
    [SerializeField] private UnitDefinition friendlyUnitDefinition;
    [SerializeField] private UnitDefinition friendlyBaseDefinition;
    [SerializeField] private List<UnitDefinition> enemyDefinitions;
    [SerializeField] private UnitDatabase unitDatabase;
    [SerializeField] private TimelineSelectionWidget timelineSelectionWidget;
    private bool isSelectingSplitTimeTarget;
    private int pendingSplitTimeUnitId = -1;

    [SerializeField] private int boardWidth = 8;
    [SerializeField] private int boardHeight = 8;

    [SerializeField] private int enemiesToSpawn = 3;
    [SerializeField] private int friendlyUnitsToPlace = 3;
    [SerializeField] private bool randomlyPlaceBase = true;
    [SerializeField] private float enemyActionDelay = 0.35f;
    [SerializeField] private Transform boardOrigin;
    [SerializeField] private float tileSize= 1f;
    [SerializeField] private float projectileHeight=0.5f;
    [SerializeField] private int CollectibleScoreValue =100;
    [SerializeField] private int collectiblesPerTimeline= 4;
    [SerializeField] private bool spawnCollectibles=true;
    [Header("Player UI")]
    [SerializeField] private GameObject playerButtonsRoot;
    [SerializeField] private GameObject pileRoot;
    [SerializeField] private CanvasGroup handCanvasGroup;
    [SerializeField] private TutorialSequenceUI tutorialSequenceUI;
    [SerializeField] private bool showTutorialOnStart = true;
    private int nextCollectibleId;
    

    private int placedFriendlyUnits;
    private BoardState setupBoardState;

    private readonly List<Timeline> timelines = new();
    private readonly CardResolver cardResolver = new();

    private Timeline activeTimeline;
    private BoardState committedBoardState;
    private BoardState workingBoardState;
    private Dictionary<int, BoardState> workingStatesByTimelineId
    = new Dictionary<int, BoardState>();

    private int nextTimelineId;
    private int nextUnitId;

    public TurnPhase CurrentPhase { get; private set; }
    public GameOver gameOver;

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (showTutorialOnStart && tutorialSequenceUI != null)
        {
            tutorialSequenceUI.Open(StartMatch);
        }
        else
        {
            StartMatch();
        }
    }

    // Called when a GameManager is made, creates a startingstate and sets up the timelines
    public void StartMatch()
    {
        nextCollectibleId=0;
        CurrentPhase = TurnPhase.Setup;

        if (ScoreManager.Instance != null) // resets score.
        {
            ScoreManager.Instance.ResetScore();
        }

        timelines.Clear();

        nextTimelineId = 0;
        nextUnitId = 0;
        placedFriendlyUnits = 0;

        setupBoardState = new BoardState(boardWidth, boardHeight, nextTimelineId, 0);

        if (randomlyPlaceBase) PlaceRandomBase(setupBoardState);
        PlaceRandomEnemies(setupBoardState, enemiesToSpawn);
        
        

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

            UnitDefinition def = enemyDefinitions[Random.Range(0, enemyDefinitions.Count)];

            if (def == null)
            {
                continue;
            }

            Vector2Int position;

            if (def.EnemyBehavior is BaseEaterBehavior)
            {
                position= FarFromBase(boardState);

                if (position.x <0 || position.y < 0)
                {
                    continue;
                }
            }
            else
            {
                int x = Random.Range(0, boardState.Width);
                int y = Random.Range(0, boardState.Height);

            if (boardState.GetUnitAtTile(x, y) != null)
            {
                continue;
            }
            position= new Vector2Int(x,y);
            }
            
            if (boardState.GetUnitAtTile(position.x,position.y) != null)
            {
                continue;
            }
            
            BoardUnitState unit = CreateUnit(def, position);
            boardState.AddUnit(unit, position.x, position.y);

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

    private void BeginCombatFromSetup() // new addition: warper makes its own timeline at the start of the battle.
    {
        ResolveOpeningEnemyTurn(setupBoardState);
        
        BoardState mainState= setupBoardState.Clone();
        BoardState warperState= setupBoardState.Clone();

        bool hasWarper= ThereBeAWarper(warperState);

        if (hasWarper)
        {
            RemoveWarpers(mainState);
            mainState.EnemyIntents.Clear();
            TelegraphEnemyAttacks(mainState);
            warperState.EnemyIntents.Clear();
            TelegraphEnemyAttacks(warperState);
        }

        Timeline mainTimeline = new Timeline(nextTimelineId, 0);
        nextTimelineId++;
        
        mainTimeline.AddState(mainState);
        timelines.Add(mainTimeline);

        if (hasWarper)
        {
            Timeline warperTimeline=new Timeline(nextTimelineId,0);
            nextTimelineId++;
            warperState.TimelineId= warperTimeline.TimelineId;
            

            warperTimeline.AddState(warperState);
            timelines.Add(warperTimeline);
        }

        activeTimeline = mainTimeline;
        workingBoardState = null;
        setupBoardState = null;
         if (spawnCollectibles)
        {
            SpawnCollectiblesOnAllTimelines();
        }

        committedBoardState= mainTimeline.GetLatestState();
        boardRepresentative.Render(committedBoardState);
       

        StartPlayerTimelineSelection();
    }

    

    // Draws cards, renders the current committed board, and waits for the player to choose a timeline
    public void StartPlayerTimelineSelection()
    {
        CurrentPhase = TurnPhase.TimelineSelection;

        workingBoardState = null;

        cardManager.DrawHand(5);
        SetPlayerUIVisible(false, false);

        if (energyWidget != null)
        {
            energyWidget.Hide();
        }

        if (committedBoardState != null)
        {
            boardRepresentative.Render(committedBoardState);
        }

        if (timelines.Count == 1)
        {
            SelectTimelineForTurn(timelines[0].TimelineId);
            return;
        }

        if (timelineSelectionWidget != null)
        {
            timelineSelectionWidget.Open(this);
        }
    }

    public Vector3 WorldPosition(Vector2Int tilePosition) // this is used for the projectile movement. where it is in the game world.
    {
        Vector3 originPoint= boardOrigin != null ? boardOrigin.position : Vector3.zero;

        return originPoint+new Vector3(tilePosition.x* tileSize, projectileHeight, tilePosition.y* tileSize);
    }



    private Timeline GetRandomPlayableTimeline()
    {
        if (timelines.Count == 0)
        {
            return null;
        }

        return timelines[Random.Range(0, timelines.Count)];
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

        BoardState clonedState = committedBoardState.CloneForNextTurn();

        clonedState.FriendlyBuffCountdown(); // starts buff turn countdown.
        workingStatesByTimelineId[activeTimeline.TimelineId] = clonedState;
        workingBoardState = clonedState;
        workingBoardState.RefreshEnergyFromFriendlyUnits();
        if (energyWidget != null) energyWidget.Show(workingBoardState.EnergyState);

        CurrentPhase = TurnPhase.PlayerTurn;
        SetPlayerUIVisible(true, true);

        
        RenderBoardWithWarperGhosts(workingBoardState);
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

        CommitWorkingBoardState();
        StartEnemyTurn();
    }

    private void StartEnemyTurn()
    {
        StartCoroutine(RunEnemyTurn());
    }

    private IEnumerator RunEnemyTurn()
    {
        CurrentPhase = TurnPhase.EnemyResolution;

        Timeline enemyTimeline = GetRandomPlayableTimeline();

        if (enemyTimeline == null)
        {
            yield break;
        }

        activeTimeline = enemyTimeline;
        committedBoardState = activeTimeline.GetLatestState();
        workingBoardState = committedBoardState.CloneForNextTurn();

        boardRepresentative.Render(workingBoardState);

        LogAllUnitHealth(workingBoardState, "Start of Enemy Turn");

        yield return new WaitForSeconds(enemyActionDelay);

       // ExecuteTelegraphedEnemyAttacks(workingBoardState);
        //boardRepresentative.Render(workingBoardState);
        yield return EnemyProjectileTelegrphed(workingBoardState);
        boardRepresentative.Render(workingBoardState);

        WarperAttackActiveTimeline(workingBoardState);
        boardRepresentative.Render(workingBoardState);


        yield return new WaitForSeconds(enemyActionDelay);

        MoveEnemies(workingBoardState);
        boardRepresentative.Render(workingBoardState);


        yield return new WaitForSeconds(enemyActionDelay);

        TelegraphEnemyAttacks(workingBoardState);
        boardRepresentative.Render(workingBoardState);

        yield return new WaitForSeconds(enemyActionDelay);

        CommitWorkingBoardState();
        if (ShouldLose(workingBoardState))
        {
            LoseMatch();
            yield break;
        }
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

                if (target == null)
                {
                    continue;
                }

                if (target.Team != UnitTeam.Friendly) // prevents enemies from attacking other enemies.
                {
                    continue;
                }

                target.Health -= intent.Damage;

                if (target.Health <= 0)
                {
                    state.RemoveUnit(target.UnitId);
                }

                boardRepresentative.Render(state);
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
        if (workingBoardState == null)
        {
            return;
        }

        Timeline timeline = GetTimeline(workingBoardState.TimelineId);

        if (timeline == null)
        {
            Debug.LogError($"No timeline found for board state timeline id {workingBoardState.TimelineId}");
            return;
        }

        timeline.AddState(workingBoardState);

        Debug.Log(
            $"Committed state turn={workingBoardState.TurnCount} " +
            $"to timeline={workingBoardState.TimelineId}"
        );
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
        int timelineCountBeforeCard= timelines.Count;
        bool resolved = cardResolver.ResolveCard(card, workingBoardState, actingUnitId, targetPosition, this);

        if (!resolved)
        {
            workingBoardState.EnergyState.Refund(card.Cost);
            return false;
        }

        CheckCollectiblePickup(workingBoardState);
        
        
        if (ScoreManager.Instance != null) // this decreases the max card score everytime a card is played.
        {
            ScoreManager.Instance.CardUsed();
        }

        TelegraphEnemyAttacks(workingBoardState);

        cardManager.DiscardCard(card);
        boardRepresentative.Render(workingBoardState);

        if (energyWidget != null)
        {
            energyWidget.Refresh(workingBoardState.EnergyState);
        }

        if (timelines.Count > timelineCountBeforeCard)
        {
            CommitWorkingBoardState();
            TimelineSelectionWhenSplit();
            return true;
            
        }

        if (CheckWin())
        {
            return true;
        }
        RenderBoardWithWarperGhosts(workingBoardState);
        return true;
    }

    public bool TryMoveUnit(int unitId, int x, int y)
    {
        if (CurrentPhase != TurnPhase.PlayerTurn) return false;
        if (workingBoardState == null) return false;

        bool moved = workingBoardState.MoveUnit(unitId, x, y);

        if (moved)
        {
            if(workingBoardState.UnitsById.TryGetValue(unitId, out BoardUnitState unit))
            {
                CollectiblePickup(workingBoardState, unit);
            }
            boardRepresentative.Render(workingBoardState);
        }
        RenderBoardWithWarperGhosts(workingBoardState);
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

    public bool SendUnitToTurn(int unitId, int targetTurn)
    {
        if (workingBoardState == null) return false;

        if (!workingBoardState.UnitsById.TryGetValue(unitId, out BoardUnitState unit))
        {
            return false;
        }

        if (targetTurn < 0 || targetTurn >= workingBoardState.TurnCount)
        {
            return false;
        }

        BoardState targetState = activeTimeline.GetStateAtTurn(targetTurn);

        if (targetState == null)
        {
            return false;
        }

        if (unit.IsBase)
        {
            return false;
        }

        int newTimelineId = nextTimelineId;
        nextTimelineId++;

        BoardUnitState copiedUnit = unit.Clone();
        copiedUnit.UnitId = nextUnitId++;
        copiedUnit.Position = unit.Position;

        BoardState branchedState = targetState.Clone();
        branchedState.TimelineId = newTimelineId;
        branchedState.EnemyIntents.Clear();

        Vector2Int spawnPosition = copiedUnit.Position;

        if (!branchedState.IsInsideBoard(spawnPosition.x, spawnPosition.y))
        {
            return false;
        }

        if (branchedState.GetUnitAtTile(spawnPosition.x, spawnPosition.y) != null)
        {
            spawnPosition = FindNearestEmptyTile(branchedState, spawnPosition);

            if (spawnPosition.x < 0)
            {
                return false;
            }
        }

        workingBoardState.RemoveUnit(unitId);
        workingBoardState.EnemyIntents.Clear();
        TelegraphEnemyAttacks(workingBoardState);

        branchedState.AddUnit(copiedUnit, spawnPosition.x, spawnPosition.y);
        TelegraphEnemyAttacks(branchedState);

        Timeline newTimeline = new Timeline(newTimelineId, targetTurn);
        newTimeline.AddState(branchedState);

        timelines.Add(newTimeline);

        DebugTimelines();

        return true;
    }

    public bool BeginSplitTimeSelection(int unitId)
    {
        if (CurrentPhase != TurnPhase.PlayerTurn) return false;
        if (workingBoardState == null) return false;

        if (!workingBoardState.UnitsById.TryGetValue(unitId, out BoardUnitState unit))
        {
            return false;
        }

        if (unit.IsBase)
        {
            return false;
        }

        isSelectingSplitTimeTarget = true;
        pendingSplitTimeUnitId = unitId;
        SetPlayerUIVisible(false, false); // Hides cards and buttons when choosing a timeline split.
        if (energyWidget!= null)
        {
            energyWidget.Hide();
        }
        if (timelineSelectionWidget != null)
        {
            timelineSelectionWidget.OpenForSplitTime(this);
            return true;
        }

        return false;
    }

    public void RefreshBoardVisuals()
    {
        if (workingBoardState != null)
        {
            boardRepresentative.Render(workingBoardState);
        }

        if (energyWidget != null && workingBoardState != null)
        {
            energyWidget.Refresh(workingBoardState.EnergyState);
        }
    }

    public bool ResolveSplitTimeSelection(int targetTimelineId, int targetStateIndex)
    {
        if (!isSelectingSplitTimeTarget) return false;
        if (workingBoardState == null) return false;

        if (!workingBoardState.UnitsById.TryGetValue(pendingSplitTimeUnitId, out BoardUnitState unit))
        {
            ClearSplitTimeSelection();
            return false;
        }

        Timeline targetTimeline = GetTimeline(targetTimelineId);

        if (targetTimeline == null)
        {
            return false;
        }

        BoardState selectedState = targetTimeline.GetStateAtIndex(targetStateIndex);

        if (selectedState == null)
        {
            return false;
        }

        if (unit.IsBase)
        {
            return false;
        }

        BoardUnitState copiedUnit = unit.Clone();
        copiedUnit.UnitId = nextUnitId++;
        copiedUnit.Position = unit.Position;

        bool targetIsRightmost = IsRightmostTimelineState(targetTimelineId, targetStateIndex);

        workingBoardState.RemoveUnit(unit.UnitId);
        workingBoardState.EnemyIntents.Clear();
        TelegraphEnemyAttacks(workingBoardState);

        if (targetIsRightmost)
        {
            BoardState continuedState = selectedState.CloneForNextTurn();
            continuedState.TimelineId = targetTimeline.TimelineId;

            if (!PlaceCopiedUnitIntoState(continuedState, copiedUnit))
            {
                ClearSplitTimeSelection();
                return false;
            }

            TelegraphEnemyAttacks(continuedState);
            targetTimeline.AddState(continuedState);

            if (activeTimeline != null && activeTimeline.TimelineId == targetTimeline.TimelineId)
            {
                committedBoardState = targetTimeline.GetLatestState();
            }

            Debug.Log($"Split Time continued Timeline={targetTimeline.TimelineId} from StateIndex={targetStateIndex}");
        }
        else
        {
            int newTimelineId = nextTimelineId;
            nextTimelineId++;

            BoardState branchedState = selectedState.Clone();
            branchedState.TimelineId = newTimelineId;
            branchedState.EnemyIntents.Clear();

            if (!PlaceCopiedUnitIntoState(branchedState, copiedUnit))
            {
                ClearSplitTimeSelection();
                return false;
            }

            TelegraphEnemyAttacks(branchedState);

            Timeline newTimeline = new Timeline(newTimelineId, branchedState.TurnCount);
            newTimeline.AddState(branchedState);
            timelines.Add(newTimeline);

            Debug.Log($"Split Time created Timeline={newTimelineId} from Timeline={targetTimelineId}, StateIndex={targetStateIndex}");
        }

        ClearSplitTimeSelection();

        if (timelineSelectionWidget != null)
        {
            timelineSelectionWidget.Hide();
        }

        CurrentPhase= TurnPhase.PlayerTurn;

        SetPlayerUIVisible(true,true);

        if(energyWidget != null && workingBoardState != null)
        {
            energyWidget.Show(workingBoardState.EnergyState);
            energyWidget.Refresh(workingBoardState.EnergyState);
        }

        boardRepresentative.Render(workingBoardState);
        DebugTimelines();

        return true;
    }

    private bool PlaceCopiedUnitIntoState(BoardState state, BoardUnitState copiedUnit)
    {
        Vector2Int spawnPosition = copiedUnit.Position;

        if (!state.IsInsideBoard(spawnPosition.x, spawnPosition.y))
        {
            return false;
        }

        if (state.GetUnitAtTile(spawnPosition.x, spawnPosition.y) != null)
        {
            spawnPosition = FindNearestEmptyTile(state, spawnPosition);

            if (spawnPosition.x < 0)
            {
                return false;
            }
        }

        state.AddUnit(copiedUnit, spawnPosition.x, spawnPosition.y);
        return true;
    }

    private void ClearSplitTimeSelection()
    {
        isSelectingSplitTimeTarget = false;
        pendingSplitTimeUnitId = -1;
    }

    private Vector2Int FindNearestEmptyTile(BoardState state, Vector2Int origin)
    {
        if (state.IsInsideBoard(origin.x, origin.y) && state.GetUnitAtTile(origin.x, origin.y) == null)
        {
            return origin;
        }

        for (int radius = 1; radius <= Mathf.Max(state.Width, state.Height); radius++)
        {
            for (int x = origin.x - radius; x <= origin.x + radius; x++)
            {
                for (int y = origin.y - radius; y <= origin.y + radius; y++)
                {
                    if (!state.IsInsideBoard(x, y))
                    {
                        continue;
                    }

                    if (Mathf.Abs(origin.x - x) + Mathf.Abs(origin.y - y) > radius)
                    {
                        continue;
                    }

                    if (state.GetUnitAtTile(x, y) == null)
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }
        }

        return new Vector2Int(-1, -1);
    }

    public void DebugTimelines()
    {
        Debug.Log("===== TIMELINE DEBUG =====");

        for (int i = 0; i < timelines.Count; i++)
        {
            Timeline timeline = timelines[i];

            Debug.Log($"Timeline {timeline.TimelineId} | CreatedOnTurn={timeline.CreatedOnTurn} | States={timeline.StateCount}");

            for (int j = 0; j < timeline.StateCount; j++)
            {
                BoardState state = timeline.GetStateAtIndex(j);

                if (state == null)
                {
                    Debug.Log($"    StateIndex {j}: NULL");
                    continue;
                }

                Debug.Log(
                    $"    StateIndex {j} | Turn={state.TurnCount} | Timeline={state.TimelineId} | Units={state.UnitsById.Count} | Intents={state.EnemyIntents.Count}"
                );
            }
        }
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
        if (ScoreManager.Instance != null) // basaclly sends the final score to the end scene.
        {
            ScoreManager.Instance.CalculateFinalScore(this);
        }
        Debug.Log("Victory");
        SceneManager.LoadScene("Ending");
    }

    private void LoseMatch()
    {
        CurrentPhase = TurnPhase.GameOver;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.CalculateScoreGameOver();
        }
        Debug.Log("Defeat");
        if (gameOver != null){
        gameOver.ShowGameOver();
        }
    }

    private void LogAllUnitHealth(BoardState state, string context) // unit hp indicator 
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

    // Helpers with timeline rendering
    public BoardState GetTimelineState(int timelineId, int stateIndex)
    {
        Timeline timeline = GetTimeline(timelineId);

        if (timeline == null)
        {
            return null;
        }

        return timeline.GetStateAtIndex(stateIndex);
    }

    public bool IsRightmostTimelineState(int timelineId, int stateIndex)
    {
        Timeline timeline = GetTimeline(timelineId);

        if (timeline == null)
        {
            return false;
        }

        return stateIndex == timeline.StateCount - 1;
    }

    public void PreviewTimelineState(BoardState state)
    {
        if (state == null)
        {
            return;
        }
        Debug.Log($"GameManager preview state: turn={state.TurnCount}, timeline={state.TimelineId}, units={state.UnitsById.Count}");
        RenderBoardWithWarperGhosts(workingBoardState);
        
    }

    public void RenderCommittedBoard()
    {
        if (committedBoardState != null)
        {
            boardRepresentative.Render(committedBoardState);
        }
    }

    private Vector2Int FarFromBase(BoardState boardState){// finds the farthest empty tile from the base.
        BoardUnitState baseUnit= null;

        foreach (var pair in boardState.UnitsById)
        {
            BoardUnitState unit= pair.Value;

            if (unit.Team== UnitTeam.Friendly && unit.IsBase && unit.Health > 0)
            {
                baseUnit=unit;
                break;
            }
        }
        if (baseUnit == null)
        {
            return new Vector2Int(-1,-1);
        }

        Vector2Int bestTile= new Vector2Int(-1,-1);
        int bestDistance=-1;

        for (int x=0; x<boardState.Width; x++)
        {
            for(int y=0; y<boardState.Height; y++)
            {
                if (boardState.GetUnitAtTile(x,y) != null)
                {
                    continue;
                }
                int distance= Mathf.Abs(x-baseUnit.Position.x) + Mathf.Abs(y-baseUnit.Position.y);

                if (distance> bestDistance)
                {
                    bestDistance= distance;
                    bestTile= new Vector2Int(x,y);
                }
            }
        }
        return bestTile;
    }

    private void SpawnBaseEater(BoardState boardState, UnitDefinition enemyDefinition)
    {
        Vector2Int spawnPosition= FarFromBase(boardState);

      BoardUnitState unit= CreateUnit(enemyDefinition, spawnPosition);
      boardState.AddUnit(unit, spawnPosition.x, spawnPosition.y);
    }

    private IEnumerator EnemyProjectileTelegrphed(BoardState state) // coroutine for enemy projectiles
    {
        for (int i=0; i< state.EnemyIntents.Count; i++)
        {
            EnemyIntentState intent= state.EnemyIntents[i];

            if(!state.UnitsById.TryGetValue(intent.EnemyUnitId, out BoardUnitState enemy))
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

            UnitDefinition enemyDefinition= unitDatabase.GetDefinition(enemy.UnitDefinitionId);

            RangedUnitBehavior rangedBehavior=null;

            if ( enemyDefinition != null)
            {
                rangedBehavior= enemyDefinition.EnemyBehavior as RangedUnitBehavior;
            }

            for (int j=0; j< intent.TargetTiles.Count; j++)
            {
                Vector2Int position= intent.TargetTiles[j];
                BoardUnitState target= state.GetUnitAtTile(position.x,position.y);

                if (target == null)
                {
                    continue;
                }

                if (target.Team != UnitTeam.Friendly)
                {
                    continue;
                }

                if (rangedBehavior !=null && rangedBehavior.ProjectilePrefab != null)
                {
                    Vector3 startPosition= WorldPosition(enemy.Position);
                    Vector3 endPosition= WorldPosition(target.Position);

                    ProjectileMovement projectile= Instantiate( rangedBehavior.ProjectilePrefab,startPosition, Quaternion.identity);

                    yield return projectile.MoveTo( startPosition, endPosition, rangedBehavior.projectileSpeed);
                    Destroy(projectile.gameObject);
                }
                if (!state.UnitsById.ContainsKey(target.UnitId)) // if player dies before the enemy attack hits them.
                {
                    continue;
                }

                target.Health -= intent.Damage;
                if (target.Health <= 0)
                {
                    state.RemoveUnit(target.UnitId);
                }
                boardRepresentative.Render(state);
            }
        }
        state.EnemyIntents.Clear();
    }

    private bool IsWarperInTimeline(BoardUnitState unit)
    {
        if(unit== null)
        {
            return false;
        }
        UnitDefinition definition= unitDatabase.GetDefinition(unit.UnitDefinitionId);

        if (definition == null)
        {
            return false;
        }

        return definition.EnemyBehavior is WarperEnemyBehavior;
    }

    private void WarperAttackActiveTimeline(BoardState activeState)
    {
        if (activeState== null)
        {
            return;
        }

        for (int i=0; i< timelines.Count; i++)
        {
            Timeline timeline= timelines[i];

            BoardState WarperState;
            if (activeTimeline != null && timeline.TimelineId== activeTimeline.TimelineId)
            {
                WarperState=workingBoardState;
            }
            else
            {
                WarperState = timeline.GetLatestState();
            }
            if (WarperState == null)
            {
                continue;
            }
            List<BoardUnitState> unitsToCheck=new();
            foreach (var pair in WarperState.UnitsById)
            {
                unitsToCheck.Add(pair.Value);
            }
                for( int j=0; j< unitsToCheck.Count; j++){
                    BoardUnitState warper= unitsToCheck[j];

                    if (warper== null)
                {
                    continue;
                }
                if (warper.Team != UnitTeam.Enemy || warper.Health <= 0)
                {
                    continue;
                }

                if (!IsWarperInTimeline(warper))
                {
                    continue;
                }

                UnitDefinition definition = unitDatabase.GetDefinition(warper.UnitDefinitionId);
                WarperEnemyBehavior behavior= definition.EnemyBehavior as WarperEnemyBehavior;

                if (definition == null)
                {
                    continue;
                }
                if (behavior == null)
                {
                    continue;
                }

                BoardUnitState target= FindNearestFriendlyInRange(activeState, warper.Position, behavior.AttackRange);

                if (target == null)
                {
                    continue;
                }

                target.Health-=behavior.Damage;
                Debug.Log($"Warper attacked unit {target.UnitId} for {behavior.Damage}");

                if (target.Health <= 0)
                {
                    activeState.RemoveUnit(target.UnitId);
                }
                }
        }
    }

    private BoardUnitState FindNearestFriendlyInRange(BoardState state, Vector2Int origin, int range)
    {
        BoardUnitState bestTarget=null;
        int bestDistance= int.MaxValue;

        foreach (var pair in state.UnitsById)
        {
            BoardUnitState unit= pair.Value;

            if(unit.Team != UnitTeam.Friendly)
            {
                continue;
            }

            if (unit.Health <= 0)
            {
                continue;
            }

            int distance=Mathf.Abs(origin.x-unit.Position.x)+Mathf.Abs(origin.y- unit.Position.y);

            if (distance > range)
            {
                continue;
            }

            if (distance < bestDistance)
            {
                bestDistance=distance;
                bestTarget=unit;
            }
        }
        return bestTarget;
    }
    private bool ThereBeAWarper(BoardState state)
    {
        foreach (var pair in state.UnitsById)
        {
            BoardUnitState unit =pair.Value;

            if (unit.Team== UnitTeam.Enemy && unit.Health> 0 && IsWarperInTimeline(unit))
            {
                return true;
            }
        }
        return false;
    }

    private void RemoveWarpers(BoardState state)
    {
        List<int> warperIds= new();

        foreach (var pair in state.UnitsById)
        {
            BoardUnitState unit = pair.Value;

            if (unit.Team== UnitTeam.Enemy && IsWarperInTimeline(unit))
            {
                warperIds.Add(unit.UnitId);
            }
        }
        for (int i=0; i< warperIds.Count; i++)
        {
            state.RemoveUnit(warperIds[i]);
        }
    }
    public float GetAverageBaseHP()
    {
       
            if( timelines== null || timelines.Count == 0)
            {
                return 0f;
            }

            int totalBaseHP= 0;
            int BaseCount=0;

            for (int i=0; i< timelines.Count; i++)
            {
                BoardState state = timelines[i].GetLatestState();

                if (state== null)
                {
                    continue;
                }

                foreach( var pair in state.UnitsById)
                {
                    BoardUnitState unit= pair.Value;

                    if (unit.Team== UnitTeam.Friendly && unit.IsBase)
                    {
                        totalBaseHP += Mathf.Max(0,unit.Health);
                        BaseCount++;
                    }
                }
            }

            if (BaseCount == 0)
            {
                return 0f;
            }
            return (float)totalBaseHP/BaseCount;
        }
        private void SpawnCollectiblesOnAllTimelines()
    {
        for (int i=0; i<timelines.Count; i++){
        Timeline timeline= timelines[i];
        BoardState state= timeline.GetLatestState();
        if (state== null)
            {
                continue;
            }
            for (int j=0; j< collectiblesPerTimeline; j++)
            {
                SpawnCollectibleOnRandomEmptyTile(state);
            }
        }
    }

    private void SpawnCollectibleOnRandomEmptyTile(BoardState state)
    {
        if (state == null)
        {
            return;
        }
        int safety=0;

        while (safety< 1000)
        {
            safety++;

            int x= Random.Range(0, state.Width);
            int y= Random.Range(0, state.Height);

            if (state.GetUnitAtTile(x,y) != null)
            {
                continue;
            }

            if(state.GetCollectibleAtTile(x,y) != null)
            {
                continue;
            }

            BoardCollectibleState collectible= new BoardCollectibleState
            {
                CollectibleId= nextCollectibleId++,
                Position= new Vector2Int(x,y),
                ScoreValue= CollectibleScoreValue
            };

            state.AddCollectible(collectible);

            Debug.Log($"Spawned collectible at {collectible.Position} on timeline {state.TimelineId}");
            return;
        }
        Debug.LogWarning("Failed to spawn collectible");
    }

    private void CollectiblePickup(BoardState state, BoardUnitState unit)
    {
        if (state == null || unit == null)
        {
            return;
        }
        if (unit.Team != UnitTeam.Friendly)
        {
            return;
        }
        BoardCollectibleState collectible= state.GetCollectibleAtTile(unit.Position.x,unit.Position.y);

        if(collectible == null)
        {
            return;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddCollectibleScore(collectible.ScoreValue);
        }

        state.RemoveCollectible(collectible.CollectibleId);

        Debug.Log($"Friendly unit {unit.UnitId} picked up collectible for {collectible.ScoreValue} points");
    }
    public void CheckCollectiblePickup(BoardState state) // checks collectibles after cards are played.
    {
       if (state == null)
        {
            return;
        } 
        List<BoardUnitState> friendlyUnits= new();

        foreach(var pair in state.UnitsById)
        {
            BoardUnitState unit = pair.Value;

            if (unit.Team== UnitTeam.Friendly && unit.Health > 0)
            {
                friendlyUnits.Add(unit);
            }
        }
        for (int i= 0; i< friendlyUnits.Count; i++)
        {
            CollectiblePickup(state,friendlyUnits[i]);
        }
    } 

    public void RefreshBoard(BoardState state)
    {
        if (state==null || boardRepresentative== null)
        {
            return;
        }
        RenderBoardWithWarperGhosts(workingBoardState);
        
    }

    private void SetPlayerUIVisible(bool visible, bool cardsInteractable)
    {
        if (playerButtonsRoot != null)
        {
            playerButtonsRoot.SetActive(visible && cardsInteractable);
        }

        if (pileRoot != null)
        {
            pileRoot.SetActive(visible);
        }

        if (handCanvasGroup != null)
        {
            handCanvasGroup.alpha= visible ? 1f : 0f;
            handCanvasGroup.interactable= visible && cardsInteractable;
            handCanvasGroup.blocksRaycasts= visible && cardsInteractable;
        }
    }

    public void ShowTimelinePreviewUI()
    {
        SetPlayerUIVisible(true,false);
    }

    public void HideTimelinePreviewUI()
    {
        SetPlayerUIVisible(false, false);
    }

    private bool CheckWin() // if player wins they will not have to press end turn to win.
    {
        if ( workingBoardState== null)
        {
            return false;
        }

        if (ShouldWin(workingBoardState))
        {
            CommitWorkingBoardState();
            WinMatch();
            return true;
        }
        return false;
    }

    private void TimelineSelectionWhenSplit()
    {
        CurrentPhase= TurnPhase.TimelineSelection;
        workingBoardState=null;

        if(energyWidget != null)
        {
            energyWidget.Hide();
        }
        SetPlayerUIVisible(false, false);

        if (timelineSelectionWidget != null)
        {
            timelineSelectionWidget.Open(this);
        }
    }

        private List<WarperGhostVisualState> GetWarperGhostsForVisibleTimeline(BoardState visibleState)
{
    List<WarperGhostVisualState> ghosts = new();

    if (visibleState == null)
    {
        return ghosts;
    }

    for (int i = 0; i < timelines.Count; i++)
    {
        Timeline timeline = timelines[i];

        
        if (timeline.TimelineId == visibleState.TimelineId)// doesn't make a ghost for Warpers already in the visible timeline.
        {
            continue;
        }

        BoardState warperState = timeline.GetLatestState();

        if (warperState == null)
        {
            continue;
        }

        foreach (var pair in warperState.UnitsById)
        {
            BoardUnitState possibleWarper = pair.Value;

            if (possibleWarper.Team != UnitTeam.Enemy || possibleWarper.Health <= 0)
            {
                continue;
            }

            if (!IsWarperInTimeline(possibleWarper))
            {
                continue;
            }

            UnitDefinition definition = unitDatabase.GetDefinition(possibleWarper.UnitDefinitionId);

            if (definition == null)
            {
                continue;
            }

            WarperEnemyBehavior behavior = definition.EnemyBehavior as WarperEnemyBehavior;

            if (behavior == null)
            {
                continue;
            }

            WarperGhostVisualState ghost = new WarperGhostVisualState
            {
                UnitDefinitionId = possibleWarper.UnitDefinitionId,
                Position = possibleWarper.Position
            };

            BoardUnitState target = FindNearestFriendlyInRange(
                visibleState,
                possibleWarper.Position,
                behavior.AttackRange
            );

            if (target != null)
            {
                ghost.TargetTiles.Add(target.Position);
            }

            ghosts.Add(ghost);
        }
    }

    return ghosts;
}
private void RenderBoardWithWarperGhosts(BoardState state)
{
    if (boardRepresentative == null || state==null)
    {
        return;
    }

    boardRepresentative.Render(state);

    List<WarperGhostVisualState> ghosts = GetWarperGhostsForVisibleTimeline(state);
    boardRepresentative.RenderWarperGhosts(ghosts, unitDatabase);
}
    }


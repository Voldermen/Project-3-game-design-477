using UnityEngine;

// This is the main class that the player interacts with all the game logic through

public class SelectionController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    public int SelectedUnitId = -1;
    public CardDefinition SelectedCard;
    public Vector2Int HoveredTile;
    public bool HasHoveredTile;

    private readonly TargetingValidator targetingValidator = new();

    public void SelectTimeline(int timelineId)
    {
        gameManager.SelectTimelineForTurn(timelineId);
    }

    public void SelectUnit(int unitId)
    {
        SelectedUnitId = unitId;
    }

    public void SelectCard(CardDefinition card)
    {
        SelectedCard = card;
    }

    public void ClearSelection()
    {
        SelectedUnitId = -1;
        SelectedCard = null;
    }

    public void HoverTile(int x, int y)
    {
        HoveredTile = new Vector2Int(x, y);
        HasHoveredTile = true;
    }

    public void ClickTile(int x, int y)
    {
        BoardState boardState = gameManager.GetWorkingBoardState();

        if (boardState == null) return;

        Vector2Int targetPosition = new Vector2Int(x, y);

        if (SelectedCard != null)
        {
            if (!targetingValidator.CanTarget(SelectedCard, boardState, SelectedUnitId, targetPosition))
            {
                return;
            }

            if (gameManager.TryPlayCard(SelectedCard, SelectedUnitId, targetPosition))
            {
                SelectedCard = null;
            }

            return;
        }

        BoardUnitState clickedUnit = boardState.GetUnitAtTile(x, y);

        if (clickedUnit != null && clickedUnit.Team == UnitTeam.Friendly)
        {
            SelectUnit(clickedUnit.UnitId);
            return;
        }

        if (SelectedUnitId != -1)
        {
            gameManager.TryMoveUnit(SelectedUnitId, x, y);
        }
    }
}
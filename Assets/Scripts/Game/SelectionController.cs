using UnityEngine;

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

    public bool TryBeginCard(CardDefinition card)
    {
        BoardState boardState = gameManager.GetWorkingBoardState();

        if (boardState == null) return false;
        if (!targetingValidator.CanBeginCard(card, boardState, SelectedUnitId)) return false;

        if (card.TargetType == CardTargetType.None)
        {
            bool played = gameManager.TryPlayCard(card, SelectedUnitId, Vector2Int.zero);

            if (played)
            {
                SelectedCard = null;
            }

            return played;
        }

        SelectedCard = card;
        return true;
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
        Debug.Log($"Clicked tile {x}, {y} during phase {gameManager.CurrentPhase}");
        if (gameManager.CurrentPhase == TurnPhase.PlayerPlacement)
        {
            gameManager.TryPlaceFriendlyUnit(x, y);
            return;
        }

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
        }
    }
}
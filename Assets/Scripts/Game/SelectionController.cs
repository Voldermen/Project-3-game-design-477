using UnityEngine;

public class SelectionController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CanvasGroup cancelCardButton;

    public int SelectedUnitId = -1;
    public CardDefinition SelectedCard;
    public Vector2Int HoveredTile;
    public bool HasHoveredTile;

    private readonly TargetingValidator targetingValidator = new();
    private CardInHandView pendingCardView;

    private void Start()
    {
        RefreshCancelButton();
    }

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
        pendingCardView=null;
        RefreshCancelButton();
    }

    public void HoverTile(int x, int y)
    {
        HoveredTile = new Vector2Int(x, y);
        HasHoveredTile = true;
    }

    public void ClickTile(int x, int y)
    {
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
                SelectedUnitId= -1;
                pendingCardView=null;
                RefreshCancelButton();
            }

            return;
        }

        BoardUnitState clickedUnit = boardState.GetUnitAtTile(x, y);

        if (clickedUnit != null && clickedUnit.Team == UnitTeam.Friendly)
        {
            SelectUnit(clickedUnit.UnitId);
        }
    }

    public bool TryPlayCardOnTile(CardDefinition card, int x, int y)
    {
        BoardState boardState= gameManager.GetWorkingBoardState();

        if (boardState== null)
        {
            return false;
        }

        Vector2Int targetPosition= new Vector2Int(x,y);

        int actingUnitId= card.PlayType == CardPlayType.Global ? -1 : SelectedUnitId;

        if (!targetingValidator.CanBeginCard(card, boardState, actingUnitId))
        {
            return false;
        }

        if (!targetingValidator.CanTarget(card, boardState, actingUnitId, targetPosition))
        {
            return false;
        }
        return gameManager.TryPlayCard(card,actingUnitId, targetPosition);
    }

    public BoardState GetWorkingBoardState()
    {
        return gameManager.GetWorkingBoardState();
    }

    public bool TryBeginCardFromUnitDrop(CardDefinition card, int unitId, CardInHandView cardView)
    {
        BoardState boardState= gameManager.GetWorkingBoardState();

        if (boardState== null || card== null)
        {
            return false;
        }

       

        if (!boardState.UnitsById.TryGetValue(unitId, out BoardUnitState unit))
        {
            return false;
        }

        if (unit.Team != UnitTeam.Friendly)
        {
            return false;
        }

        SelectedUnitId= unitId;

        if (!targetingValidator.CanBeginCard(card, boardState, SelectedUnitId))
        {
            return false;
        }

        if (card.TargetType== CardTargetType.None)
        {
            bool played= gameManager.TryPlayCard(card, SelectedUnitId, Vector2Int.zero);
            if (played)
            {
                SelectedCard= null;
                pendingCardView=null;
            }

            return played;
        }
        SelectedCard= card;
        pendingCardView=cardView;
        RefreshCancelButton();
        return true;
    }

    public void CancelCardPlay()
    {
        if (SelectedCard== null)
        {
            return;
        }

        SelectedCard=null;
        SelectedUnitId=-1;
        HasHoveredTile=false;
        if (pendingCardView != null)
        {
            pendingCardView.ReturnToHand();
            pendingCardView=null;
        }
        RefreshCancelButton();
        Debug.Log("Canceled play");
    }

    private void RefreshCancelButton()
    {
        if (cancelCardButton == null)
        {
            return;
        }

        bool show= SelectedCard != null;

        cancelCardButton.alpha= show ? 1f : 0f;
        cancelCardButton.interactable= show;
        cancelCardButton.blocksRaycasts= show;
    }
}
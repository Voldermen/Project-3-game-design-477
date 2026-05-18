using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardInHandView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image cardImage;
    [SerializeField] private Camera boardCamera;

    private CardDefinition card;
    private SelectionController selectionController;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 startAnchoredPosition;

    public void Initialize(CardDefinition newCard, SelectionController newSelectionController)
    {
        card = newCard;
        selectionController = newSelectionController;
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (cardNameText != null)
        {
            cardNameText.text= card != null ? card.DisplayName : "";
        }
    if (costText!= null)
        {
            costText.text= card != null ? card.Cost.ToString() : "";
        }

        if (descriptionText != null)
        {
            descriptionText.text= card != null ? card.Description : "";
        }

        if(cardImage != null)
        {
            cardImage.sprite= card !=null ? card.CardImage : null;
            cardImage.enabled = card != null && card.CardImage != null;
        }

        //if (cardNameText != null) cardNameText.text = card.DisplayName;
       // if (costText != null) costText.text = card.Cost.ToString();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startAnchoredPosition = rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
       TileRepresentative tile= GetTileUnderMouse(eventData.position);

       if (tile != null && selectionController != null)
        {
            BoardState state= selectionController.GetWorkingBoardState();
        

        
            if (state != null)
            {
                BoardUnitState unit= state.GetUnitAtTile(tile.X, tile.Y);

                if (card.PlayType== CardPlayType.Global)
                {
                    bool played= selectionController.TryPlayCardOnTile(card, tile.X, tile.Y);

                    if (played)
                    {
                        gameObject.SetActive(false);
                        return;
                    }
                }
                if (unit != null && unit.Team== UnitTeam.Friendly)
                {
                    bool beganCard= selectionController.TryBeginCardFromUnitDrop(card, unit.UnitId, this);
                    if (beganCard)
                    {
                        gameObject.SetActive(false);
                        return;
                    }
                }
            }
        
        }
        ReturnToHand();
    }

    public void ReturnToHand()
    {
        rectTransform.anchoredPosition = startAnchoredPosition;
        gameObject.SetActive(true);
    }

    private TileRepresentative GetTileUnderMouse(Vector2 screenPosition)
    {
        Camera cameraToUse= boardCamera !=null ? boardCamera : Camera.main;

        if (cameraToUse== null)
        {
            return null;
        }

        Ray ray =cameraToUse.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.GetComponentInParent<TileRepresentative>();
        }
        return null;
    }

    
}
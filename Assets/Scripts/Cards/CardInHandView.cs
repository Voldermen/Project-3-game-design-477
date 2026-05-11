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
        bool draggedOutOfHand = eventData.position.y > Screen.height * 0.5f;

        if (!draggedOutOfHand)
        {
            ReturnToHand();
            return;
        }

        if (!selectionController.TryBeginCard(card))
        {
            ReturnToHand();
            return;
        }

        gameObject.SetActive(false);
    }

    private void ReturnToHand()
    {
        rectTransform.anchoredPosition = startAnchoredPosition;
    }
}
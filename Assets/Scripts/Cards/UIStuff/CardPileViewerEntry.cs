using TMPro;
using UnityEngine;

public class CardPileViewerEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    public void Render(CardDefinition card)
    {
        if (nameText != null)
        {
            nameText.text = card != null ? card.DisplayName : "";
        }

        if (costText != null)
        {
            costText.text = card != null ? card.Cost.ToString() : "";
        }
    }
}
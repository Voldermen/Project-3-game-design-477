using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PileWidget : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text countText;

    private CardPileViewer pileViewer;
    private CardManager cardManager;
    private CardPileType pileType;

    public void Initialize(CardPileViewer newPileViewer, CardManager newCardManager, CardPileType newPileType)
    {
        pileViewer = newPileViewer;
        cardManager = newCardManager;
        pileType = newPileType;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OpenPile);
        }

        Refresh();
        transform.SetAsLastSibling();
        Debug.Log("PileWidget initialized");
    }

    public void Refresh()
    {
        if (titleText != null)
        {
            titleText.text = pileType == CardPileType.Draw ? "Draw" : "Discard";
        }

        if (countText != null)
        {
            countText.text = GetCount().ToString();
        }
    }

    private int GetCount()
    {
        if (cardManager == null)
        {
            return 0;
        }

        return pileType == CardPileType.Draw ? cardManager.DrawPile.Count : cardManager.DiscardPile.Count;
    }

    public void OpenPile()
    {
        Debug.Log("OpenPile called");
        if (pileViewer == null || cardManager == null)
        {
            return;
        }

        pileViewer.Open(cardManager, pileType);
    }
}
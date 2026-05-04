using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardPileViewer : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button backButton;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private CardPileViewerEntry entryPrefab;

    private readonly List<CardPileViewerEntry> activeEntries = new();

    private void Awake()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(Close);
        }

        Close();
    }

    public void Open(CardManager cardManager, CardPileType pileType)
    {
        if (root != null)
        {
            root.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = pileType == CardPileType.Draw ? "Draw Pile" : "Discard Pile";
        }

        List<CardDefinition> cards = pileType == CardPileType.Draw ? cardManager.DrawPile : cardManager.DiscardPile;
        Render(cards);
    }

    public void Close()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
    }

    private void Render(List<CardDefinition> cards)
    {
        ClearEntries();

        for (int i = 0; i < cards.Count; i++)
        {
            CardPileViewerEntry entry = Instantiate(entryPrefab, contentRoot, false);
            entry.Render(cards[i]);
            activeEntries.Add(entry);
        }
    }

    private void ClearEntries()
    {
        for (int i = activeEntries.Count - 1; i >= 0; i--)
        {
            if (activeEntries[i] != null)
            {
                Destroy(activeEntries[i].gameObject);
            }
        }

        activeEntries.Clear();
    }
}
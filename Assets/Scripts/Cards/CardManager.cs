using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [SerializeField] private List<CardDefinition> startingDeck = new();
    [SerializeField] private CardInHandView cardViewPrefab;
    [SerializeField] private Transform handRoot;
    [SerializeField] private SelectionController selectionController;
    [SerializeField] private PileWidget drawPileWidget;
    [SerializeField] private PileWidget discardPileWidget;
    [SerializeField] private CardPileViewer pileViewer;

    public List<CardDefinition> DrawPile = new();
    public List<CardDefinition> Hand = new();
    public List<CardDefinition> DiscardPile = new();

    private readonly List<CardInHandView> activeCardViews = new();

    private void Awake()
    {
        BuildStartingDeck();
        InitializePileWidgets();
    }

    private void InitializePileWidgets()
    {
        print("Initialize widgets called");
        if (drawPileWidget != null)
        {
            drawPileWidget.Initialize(pileViewer, this, CardPileType.Draw);
        }

        if (discardPileWidget != null)
        {
            discardPileWidget.Initialize(pileViewer, this, CardPileType.Discard);
        }

        RefreshPileWidgets();
        print("Initialize widgets finished");
    }

    private void RefreshPileWidgets()
    {
        if (drawPileWidget != null)
        {
            drawPileWidget.Refresh();
        }

        if (discardPileWidget != null)
        {
            discardPileWidget.Refresh();
        }
    }

    public void BuildStartingDeck()
    {
        DrawPile.Clear();
        Hand.Clear();
        DiscardPile.Clear();

        for (int i = 0; i < startingDeck.Count; i++)
        {
            DrawPile.Add(startingDeck[i]);
        }
        RefreshPileWidgets();
    }

    public void DrawHand(int count)
    {
        DiscardHand();

        for (int i = 0; i < count; i++)
        {
            DrawCard();
        }

        RenderHand();
        RefreshPileWidgets();
    }

    public CardDefinition DrawCard()
    {
        if (DrawPile.Count == 0)
        {
            RefillDrawPile();
        }

        if (DrawPile.Count == 0)
        {
            return null;
        }

        CardDefinition card = DrawPile[0];
        DrawPile.RemoveAt(0);
        Hand.Add(card);
        RefreshPileWidgets();
        return card;
    }

    public void DiscardCard(CardDefinition card)
    {
        if (card == null) return;

        if (Hand.Remove(card))
        {
            DiscardPile.Add(card);
            RenderHand();
        }
        RefreshPileWidgets();
    }

    public void DiscardHand()
    {
        DiscardPile.AddRange(Hand);
        Hand.Clear();
        RenderHand();
        RefreshPileWidgets();
    }

    private void RefillDrawPile()
    {
        DrawPile.AddRange(DiscardPile);
        DiscardPile.Clear();
        RefreshPileWidgets();
    }

    private void RenderHand()
    {
        for (int i = activeCardViews.Count - 1; i >= 0; i--)
        {
            if (activeCardViews[i] != null)
            {
                Destroy(activeCardViews[i].gameObject);
            }
        }

        activeCardViews.Clear();

        for (int i = 0; i < Hand.Count; i++)
        {
            CardInHandView view = Instantiate(cardViewPrefab, handRoot, false);
            RectTransform rect = view.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;

            float spacing = 120f;
            float startX = -((Hand.Count - 1) * spacing * 0.5f);

            rect.anchoredPosition = new Vector2(startX + i * spacing, 0f);

            view.Initialize(Hand[i], selectionController);
            activeCardViews.Add(view);
        }
    }
}
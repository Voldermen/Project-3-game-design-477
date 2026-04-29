using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public List<CardDefinition> DrawPile = new();
    public List<CardDefinition> Hand = new();
    public List<CardDefinition> DiscardPile = new();

    public void DrawHand(int count)
    {
        Hand.Clear();

        for (int i = 0; i < count; i++)
        {
            DrawCard();
        }
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
        return card;
    }

    public void DiscardHand()
    {
        DiscardPile.AddRange(Hand);
        Hand.Clear();
    }

    private void RefillDrawPile()
    {
        DrawPile.AddRange(DiscardPile);
        DiscardPile.Clear();
    }
}
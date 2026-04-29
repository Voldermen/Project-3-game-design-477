using UnityEngine;

public class CardResolver
{
    public bool ResolveCard(CardDefinition card, BoardState boardState, int actingUnitId, Vector2Int targetPosition)
    {
        if (card == null || card.CardEffect == null || boardState == null)
        {
            return false;
        }

        CardEffectContext context = new CardEffectContext
        {
            BoardState = boardState,
            Card = card,
            ActingUnitId = actingUnitId,
            TargetPosition = targetPosition
        };

        return card.CardEffect.Resolve(context);
    }
}
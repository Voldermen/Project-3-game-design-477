using UnityEngine;

[CreateAssetMenu(menuName = "Strategy Game/Card Effects/Send Unit Back")]
public class SendUnitBackCardEffect : CardEffect
{
    public override bool Resolve(CardEffectContext context)
    {
        if (context.ActingUnit == null || context.GameManager == null)
        {
            return false;
        }

        int targetTurn = context.BoardState.TurnCount - 1;

        return context.GameManager.SendUnitToTurn(context.ActingUnit.UnitId, targetTurn);
    }
}
using UnityEngine;

[CreateAssetMenu(menuName = "Strategy Game/Card Effects/Move Unit")]
public class MoveCardEffect : CardEffect
{
    public override bool Resolve(CardEffectContext context)
    {
        BoardUnitState actingUnit = context.ActingUnit;

        if (actingUnit == null)
        {
            return false;
        }

        if (!context.BoardState.IsInsideBoard(context.TargetPosition.x, context.TargetPosition.y))
        {
            return false;
        }

        if (context.BoardState.GetUnitAtTile(context.TargetPosition.x, context.TargetPosition.y) != null)
        {
            return false;
        }

        return context.BoardState.MoveUnit(actingUnit.UnitId, context.TargetPosition.x, context.TargetPosition.y);
    }
}
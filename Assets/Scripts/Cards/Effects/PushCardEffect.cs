using UnityEngine;

[CreateAssetMenu(menuName = "Strategy Game/Card Effects/Push Unit")]
public class PushCardEffect : CardEffect
{
    public int PushDistance = 1;

    public override bool Resolve(CardEffectContext context)
    {
        BoardUnitState actingUnit = context.ActingUnit;
        BoardUnitState targetUnit = context.TargetUnit;

        if (actingUnit == null || targetUnit == null)
        {
            return false;
        }

        Vector2Int direction = targetUnit.Position - actingUnit.Position;

        if (Mathf.Abs(direction.x) + Mathf.Abs(direction.y) != 1)
        {
            return false;
        }

        Vector2Int pushDirection = new Vector2Int(
            Mathf.Clamp(direction.x, -1, 1),
            Mathf.Clamp(direction.y, -1, 1)
        );

        Vector2Int destination = targetUnit.Position + pushDirection * PushDistance;

        if (!context.BoardState.IsInsideBoard(destination.x, destination.y))
        {
            return false;
        }

        if (context.BoardState.GetUnitAtTile(destination.x, destination.y) != null)
        {
            return false;
        }

        return context.BoardState.MoveUnit(targetUnit.UnitId, destination.x, destination.y);
    }
}
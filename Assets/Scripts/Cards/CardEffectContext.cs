using UnityEngine;

public class CardEffectContext
{
    public BoardState BoardState;
    public CardDefinition Card;
    public int ActingUnitId;
    public Vector2Int TargetPosition;

    public BoardUnitState ActingUnit => BoardState.UnitsById.TryGetValue(ActingUnitId, out BoardUnitState unit) ? unit : null;
    public BoardUnitState TargetUnit => BoardState.GetUnitAtTile(TargetPosition.x, TargetPosition.y);

    public bool DoDamage(BoardUnitState target, int damage)
    {
        if (target == null)
        {
            return false;
        }

        target.Health -= Mathf.Max(0, damage);

        if (target.Health <= 0)
        {
            BoardState.RemoveUnit(target.UnitId);
        }

        return true;
    }

    public bool DoHeal(BoardUnitState target, int healing)
    {
        if (target == null)
        {
            return false;
        }

        target.Health = Mathf.Min(target.MaxHealth, target.Health + Mathf.Max(0, healing));
        return true;
    }

    public bool DoMove(BoardUnitState target, Vector2Int destination)
    {
        if (target == null)
        {
            return false;
        }

        return BoardState.MoveUnit(target.UnitId, destination.x, destination.y);
    }

    public bool DoDamageAt(Vector2Int position, int damage)
    {
        return DoDamage(BoardState.GetUnitAtTile(position.x, position.y), damage);
    }

    public bool DoHealAt(Vector2Int position, int healing)
    {
        return DoHeal(BoardState.GetUnitAtTile(position.x, position.y), healing);
    }

    public bool DoMoveActingUnit(Vector2Int destination)
    {
        return DoMove(ActingUnit, destination);
    }
}
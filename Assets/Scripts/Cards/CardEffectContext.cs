using UnityEngine;

public class CardEffectContext
{
    public BoardState BoardState;
    public CardDefinition Card;
    public int ActingUnitId;
    public Vector2Int TargetPosition;
    public GameManager GameManager;

    public BoardUnitState ActingUnit => BoardState.UnitsById.TryGetValue(ActingUnitId, out BoardUnitState unit) ? unit : null;
    public BoardUnitState TargetUnit => BoardState.GetUnitAtTile(TargetPosition.x, TargetPosition.y);

    public bool DoDamage(BoardUnitState target, int damage)
    {
        if (target == null) return false;

        int finalDamage = Mathf.Max(0, damage);

        BoardUnitState actingUnit = ActingUnit;

        if (actingUnit != null)
        {
            finalDamage += Mathf.Max(0, actingUnit.strengthUp);
        }

        target.Health -= finalDamage;

        if (target.Health <= 0)
        {
            BoardState.RemoveUnit(target.UnitId);
        }

        if (GameManager != null)
        {
            GameManager.RefreshBoard(BoardState);
        }

        return true;
    }

    public bool DoHeal(BoardUnitState target, int healing)
    {
        if (target == null) return false;

        target.Health = Mathf.Min(target.MaxHealth, target.Health + Mathf.Max(0, healing));

        if (GameManager != null)
        {
            GameManager.RefreshBoard(BoardState);
        }

        return true;
    }

    public bool DoMove(BoardUnitState target, Vector2Int destination)
    {
        if (target == null) return false;

        bool moved = BoardState.MoveUnit(target.UnitId, destination.x, destination.y);

        if (moved && GameManager != null)
        {
            GameManager.RefreshBoard(BoardState);
        }

        return moved;
    }

    public bool DoDamageAt(Vector2Int position, int damage)
    {
        return DoDamage(BoardState.GetUnitAtTile(position.x, position.y), damage);
    }
}
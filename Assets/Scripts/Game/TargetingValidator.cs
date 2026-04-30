using UnityEngine;

public class TargetingValidator
{
    public bool CanTarget(CardDefinition card, BoardState boardState, int actingUnitId, Vector2Int targetPosition)
    {
        if (card == null || boardState == null)
        {
            return false;
        }

        if (!boardState.IsInsideBoard(targetPosition.x, targetPosition.y))
        {
            return false;
        }

        BoardUnitState targetUnit = boardState.GetUnitAtTile(targetPosition.x, targetPosition.y);

        switch (card.TargetType)
        {
            case CardTargetType.None:
                return true;

            case CardTargetType.Tile:
                return true;

            case CardTargetType.Unit:
                return targetUnit != null;

            case CardTargetType.FriendlyUnit:
                return targetUnit != null && targetUnit.Team == UnitTeam.Friendly;

            case CardTargetType.EnemyUnit:
                return targetUnit != null && targetUnit.Team == UnitTeam.Enemy;

            default:
                return false;
        }
    }
}
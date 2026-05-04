using UnityEngine;

public class TargetingValidator
{
    public bool CanBeginCard(CardDefinition card, BoardState boardState, int actingUnitId)
    {
        if (card == null || boardState == null) return false;

        if (card.PlayType == CardPlayType.Global)
        {
            return boardState.EnergyState.CanSpend(card.Cost);
        }

        if (!boardState.EnergyState.CanSpend(card.Cost)) return false;
        if (actingUnitId == -1) return false;
        if (!boardState.UnitsById.TryGetValue(actingUnitId, out BoardUnitState actingUnit)) return false;
        if (actingUnit.Team != card.RequiredActingUnitTeam) return false;

        return true;
    }

    public bool CanTarget(CardDefinition card, BoardState boardState, int actingUnitId, Vector2Int targetPosition)
    {
        if (card == null || boardState == null) return false;
        if (!boardState.IsInsideBoard(targetPosition.x, targetPosition.y)) return false;

        if (card.PlayType == CardPlayType.Unit)
        {
            if (!boardState.UnitsById.TryGetValue(actingUnitId, out BoardUnitState actingUnit)) return false;
            if (actingUnit.Team != card.RequiredActingUnitTeam) return false;

            if (card.TargetPattern == CardTargetPattern.CardinalAdjacentToActingUnit)
            {
                int distance = Mathf.Abs(actingUnit.Position.x - targetPosition.x) + Mathf.Abs(actingUnit.Position.y - targetPosition.y);
                if (distance != 1) return false;
            }
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
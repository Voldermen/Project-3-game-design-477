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
        if (card.PlayType == CardPlayType.Unit)
        {
            if (!boardState.UnitsById.TryGetValue(actingUnitId, out BoardUnitState actingUnit)) return false;
            if (actingUnit.Team != card.RequiredActingUnitTeam) return false;

            int distanceX= Mathf.Abs(actingUnit.Position.x - targetPosition.x);
            int distanceY= Mathf.Abs(actingUnit.Position.y - targetPosition.y);
            int distance = distanceX + distanceY;

            if (card.TargetPattern == CardTargetPattern.CardinalAdjacentToActingUnit && !((distanceX==0 || distanceY==0)&& distance> 0 && distance<=card.TargetRange))
            {
                return false;
            }

            if (card.TargetPattern == CardTargetPattern.DiagonalAdjacentToActingUnit && ! (distanceX== distanceY && distanceX <= card.TargetRange && distanceX > 0)) // makes the unit move left right for tiles above and below the unit, and make the unit move up or down for tiles that are right and left of the unit.
            {
                return false;
            }

            if (card.TargetPattern == CardTargetPattern.WithinRangeOfActingUnit && distance > card.TargetRange)
            {
                return false;
            }

            if (card.TargetPattern== CardTargetPattern.AllAroundActingUnit && !( distanceX <= card.TargetRange && distanceY <= card.TargetRange && distance> 0)) // allows for a card to be played anywhere around the player
            {
                return false;
            }
        }

        BoardUnitState targetUnit = boardState.GetUnitAtTile(targetPosition.x, targetPosition.y);

        switch (card.TargetType)
        {
            case CardTargetType.None:
                return true;

            case CardTargetType.Tile:
                return true;

            case CardTargetType.EmptyTile:
                return targetUnit == null;

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
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Strategy Game/Enemy Behaviors/Basic Chaser")]
public class BasicChaser : EnemyBehavior
{
    public int Damage = 1;
    public int MaxWalkDistance = 2;

    public override void Move(BoardState state, BoardUnitState enemy)
    {
        BoardUnitState target = FindNearestFriendlyUnit(state, enemy);

        if (target == null)
        {
            return;
        }

        Vector2Int bestPosition = enemy.Position;
        int bestDistance = GetDistance(enemy.Position, target.Position);

        Queue<Vector2Int> frontier = new();
        Dictionary<Vector2Int, int> distanceFromStart = new();

        frontier.Enqueue(enemy.Position);
        distanceFromStart[enemy.Position] = 0;

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();
            int steps = distanceFromStart[current];

            if (steps >= MaxWalkDistance)
            {
                continue;
            }

            for (int i = 0; i < directions.Length; i++)
            {
                Vector2Int next = current + directions[i];

                if (!state.IsInsideBoard(next.x, next.y))
                {
                    continue;
                }

                if (distanceFromStart.ContainsKey(next))
                {
                    continue;
                }

                BoardUnitState occupyingUnit = state.GetUnitAtTile(next.x, next.y);

                if (occupyingUnit != null && occupyingUnit.UnitId != enemy.UnitId)
                {
                    continue;
                }

                distanceFromStart[next] = steps + 1;
                frontier.Enqueue(next);

                int distanceToTarget = GetDistance(next, target.Position);

                if (distanceToTarget < bestDistance)
                {
                    bestDistance = distanceToTarget;
                    bestPosition = next;
                }
            }
        }

        if (bestPosition != enemy.Position)
        {
            state.MoveUnit(enemy.UnitId, bestPosition.x, bestPosition.y);
        }
    }

    public override EnemyIntentState CreateIntent(BoardState state, BoardUnitState enemy)
    {
        EnemyIntentState intent = new EnemyIntentState
        {
            EnemyUnitId = enemy.UnitId,
            IntentType = EnemyIntentType.Damage,
            Damage = Damage
        };

        Vector2Int[] targetTiles =
        {
            enemy.Position + Vector2Int.up,
            enemy.Position + Vector2Int.down,
            enemy.Position + Vector2Int.left,
            enemy.Position + Vector2Int.right
        };

        for (int i = 0; i < targetTiles.Length; i++)
        {
            Vector2Int tile = targetTiles[i];

            if (state.IsInsideBoard(tile.x, tile.y))
            {
                intent.TargetTiles.Add(tile);
            }
        }

        return intent;
    }

    private BoardUnitState FindNearestFriendlyUnit(BoardState state, BoardUnitState enemy)
    {
        BoardUnitState bestTarget = null;
        int bestDistance = int.MaxValue;

        foreach (var pair in state.UnitsById)
        {
            BoardUnitState unit = pair.Value;

            if (unit.Team != UnitTeam.Friendly || unit.Health <= 0)
            {
                continue;
            }

            int distance = GetDistance(enemy.Position, unit.Position);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = unit;
            }
        }

        return bestTarget;
    }

    private int GetDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
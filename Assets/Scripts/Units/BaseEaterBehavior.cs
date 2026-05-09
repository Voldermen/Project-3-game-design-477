using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Strategy Game/Enemy Behaviors/Base Eater")]
public class BaseEaterBehavior : EnemyBehavior
{
    public int Damage = 10;
    public int MaxWalkDistance = 1;

    public override void Move(BoardState state, BoardUnitState enemy)
    {
        BoardUnitState targetBase = FindBase(state);

        if (targetBase == null)
        {
            return;
        }

        Vector2Int bestPosition = enemy.Position;
        int bestDistance = GetDistance(enemy.Position, targetBase.Position);

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

                int distanceToTarget = GetDistance(next, targetBase.Position);

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

        BoardUnitState targetBase= FindBase(state);
           
           if (targetBase == null)
        {
            return intent;
        }

        int distanceToBase= GetDistance(enemy.Position,targetBase.Position);
        
        if (distanceToBase == 1) //attacks base when it gets close to it
        {
            intent.TargetTiles.Add(targetBase.Position);
        }
        return intent;
    }

    private BoardUnitState FindBase(BoardState state)
    {
      foreach (var pair in state.UnitsById)
        {
            BoardUnitState unit= pair.Value;

            if ( unit.Team== UnitTeam.Friendly && unit.IsBase && unit.Health > 0) // only looks for the player base.
            {
                return unit;
            }
        }
        return null;
    }

    private int GetDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
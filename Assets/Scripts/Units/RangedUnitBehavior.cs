using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Strategy Game/Enemy Behaviors/Ranged Unit")]
public class RangedUnitBehavior : EnemyBehavior
{
    public int Damage = 5;
    public int MaxWalkDistance = 2;
    public int rangedAttack = 5;

    [Header("Projectile")]
    public ProjectileMovement ProjectilePrefab;
    public float projectileSpeed = 6f;

    public override void Move(BoardState state, BoardUnitState enemy)
    {
        BoardUnitState targetInRange = FindNearestFriendlyUnitInRange(state, enemy);

        if (targetInRange != null)
        {
            FaceTarget(enemy, targetInRange.Position);
            return;
        }

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
        else
        {
            FaceTarget(enemy, target.Position);
        }
    }

    public override EnemyIntentState CreateIntent(BoardState state, BoardUnitState enemy)
    {
        BoardUnitState target = FindNearestFriendlyUnitInRange(state, enemy);

        if (target == null)
        {
            return null;
        }

        FaceTarget(enemy, target.Position);

        EnemyIntentState intent = new EnemyIntentState
        {
            EnemyUnitId = enemy.UnitId,
            IntentType = EnemyIntentType.Damage,
            Damage = Damage
        };

        intent.TargetTiles.Add(target.Position);

        return intent;
    }

    private BoardUnitState FindNearestFriendlyUnitInRange(BoardState state, BoardUnitState enemy)
    {
        BoardUnitState optimalTarget = null;
        int optimalDistance = int.MaxValue;

        foreach (var pair in state.UnitsById)
        {
            BoardUnitState unit = pair.Value;

            if (unit.Team != UnitTeam.Friendly)
            {
                continue;
            }

            if (unit.IsBase)
            {
                continue;
            }

            if (unit.Health <= 0)
            {
                continue;
            }

            if (!IsInCardinalRange(enemy.Position, unit.Position))
            {
                continue;
            }

            int distance = GetDistance(enemy.Position, unit.Position);

            if (distance < optimalDistance)
            {
                optimalDistance = distance;
                optimalTarget = unit;
            }
        }

        return optimalTarget;
    }

    private BoardUnitState FindNearestFriendlyUnit(BoardState state, BoardUnitState enemy)
    {
        BoardUnitState optimalTarget = null;
        int optimalDistance = int.MaxValue;

        foreach (var pair in state.UnitsById)
        {
            BoardUnitState unit = pair.Value;

            if (unit.Team != UnitTeam.Friendly)
            {
                continue;
            }

            if (unit.IsBase)
            {
                continue;
            }

            if (unit.Health <= 0)
            {
                continue;
            }

            int distance = GetDistance(enemy.Position, unit.Position);

            if (distance < optimalDistance)
            {
                optimalDistance = distance;
                optimalTarget = unit;
            }
        }

        return optimalTarget;
    }

    private bool IsInCardinalRange(Vector2Int enemyPosition, Vector2Int targetPosition)
    {
        int xDistance = Mathf.Abs(enemyPosition.x - targetPosition.x);
        int yDistance = Mathf.Abs(enemyPosition.y - targetPosition.y);
        int distance = xDistance + yDistance;

        bool column = xDistance == 0;
        bool row = yDistance == 0;

        return (column || row) && distance > 0 && distance <= rangedAttack;
    }

    private void FaceTarget(BoardUnitState enemy, Vector2Int targetPosition)
    {
        Vector2Int delta = targetPosition - enemy.Position;

        if (delta == Vector2Int.zero)
        {
            return;
        }

        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
        {
            enemy.FacingDirection = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
        }
        else
        {
            enemy.FacingDirection = delta.y > 0 ? Vector2Int.up : Vector2Int.down;
        }
    }

    private int GetDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
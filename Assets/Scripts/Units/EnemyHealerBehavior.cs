using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName= "Strategy Game/Enemy Behaviors/Enemy Healer")]
public class EnemyHealerBehavior : EnemyBehavior
{
    public int HealAmount= 4;
    public int MaxWalkDistance=2;

    public override void Move(BoardState state, BoardUnitState healer)
    {
        if (state==null || healer == null)
        {
            return;
        }

        BoardUnitState adjacentTarget= FindAdjacentDamagedEnemy(state,healer);

        if (adjacentTarget != null)
        {
            Heal(adjacentTarget);
            return;
        }

        BoardUnitState nearestDamagedEnemy= FindNearestDamagedEnemy(state,healer);

        if (nearestDamagedEnemy== null)
        {
            return;
        }
        MoveTowardTarget(state, healer, nearestDamagedEnemy);
    }
    
    public override EnemyIntentState CreateIntent(BoardState state, BoardUnitState enemy)
    {
        return new EnemyIntentState
        {
            EnemyUnitId=enemy.UnitId,
            IntentType= EnemyIntentType.Damage,
            Damage=0
        };
    }
    private BoardUnitState FindAdjacentDamagedEnemy(BoardState state, BoardUnitState healer)
    {
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
        BoardUnitState bestTarget= null;

        int lowestHealth=int.MaxValue;

        for (int i=0; i< directions.Length; i++)
        {
            Vector2Int tile= healer.Position+ directions[i];

            if(!state.IsInsideBoard(tile.x, tile.y))
            {
                continue;
            }
            BoardUnitState unit= state.GetUnitAtTile(tile.x, tile.y);

            if (unit == null)
            {
                continue;
            }
            if (unit.Team != UnitTeam.Enemy)
            {
                continue;
            }

            if(unit.UnitId== healer.UnitId)
            {
                continue;
            }

            if(unit.Health <= 0)
            {
                continue;
            }

            if( unit.Health< lowestHealth)
            {
                lowestHealth= unit.Health;
                bestTarget=unit;
            }
        }
        return bestTarget;
    }

    private BoardUnitState FindNearestDamagedEnemy(BoardState state, BoardUnitState healer)
    {
        BoardUnitState bestTarget=null;
        int bestDistance= int.MaxValue;

        foreach (var pair in state.UnitsById)
        {
            BoardUnitState unit= pair.Value;

            if(unit.Team != UnitTeam.Enemy)
            {
                continue;
            }

            if (unit.UnitId== healer.UnitId)
            {
                continue;
            }

            if( unit.Health <= 0)
            {
                continue;
            }
            if (unit.Health >= unit.MaxHealth)
            {
                continue;
            }

            int distance =GetDistance(healer.Position, unit.Position);

            if (distance < bestDistance)
            {
                bestDistance=distance;
                bestTarget=unit;
            }
        }
        return bestTarget;
    }

    private void Heal(BoardUnitState target)
    {
        target.Health= Mathf.Min(target.MaxHealth,target.Health+ Mathf.Max(0, HealAmount));
        Debug.Log($"Enemy healer healed unit {target.UnitId} for {HealAmount}. Hp is now {target.Health}/{target.MaxHealth}");
    }

    private void MoveTowardTarget(BoardState state, BoardUnitState healer, BoardUnitState target)
    {
        Vector2Int bestPosition= healer.Position;
        int bestDistance= GetDistance(healer.Position, target.Position);
        Queue<Vector2Int> frontier= new();
        Dictionary<Vector2Int, int> distanceFromStart= new();

        frontier.Enqueue(healer.Position);

        distanceFromStart[healer.Position]=0;
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        while (frontier.Count > 0)
        {
            Vector2Int current= frontier.Dequeue();
            int steps= distanceFromStart[current];

            if(steps>= MaxWalkDistance)
            {
                continue;
            }

            for (int i=0; i< directions.Length; i++)
            {
                Vector2Int next= current + directions[i];

                if(!state.IsInsideBoard(next.x, next.y))
                {
                    continue;
                }

                if (distanceFromStart.ContainsKey(next))
                {
                    continue;
                }

                BoardUnitState occupyingUnit= state.GetUnitAtTile(next.x, next.y);

                if(occupyingUnit != null && occupyingUnit.UnitId != healer.UnitId)
                {
                    continue;
                }

                distanceFromStart[next]=steps+1;
                frontier.Enqueue(next);

                int distanceToTarget= GetDistance(next, target.Position);
                if(distanceToTarget< bestDistance){
                    bestDistance=distanceToTarget;
                    bestPosition= next;
                }
            }
        }
        if (bestPosition != healer.Position)
        {
            state.MoveUnit(healer.UnitId, bestPosition.x, bestPosition.y);
        }


    }
    private int GetDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x- b.x) + Mathf.Abs(a.y-b.y);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class EnemyIntentState
{
    public int EnemyUnitId;
    public EnemyIntentType IntentType;
    public int Damage;
    public List<Vector2Int> TargetTiles = new();

    public EnemyIntentState Clone()
    {
        EnemyIntentState clone = new EnemyIntentState();

        clone.EnemyUnitId = EnemyUnitId;
        clone.IntentType = IntentType;
        clone.Damage = Damage;

        clone.TargetTiles = new List<Vector2Int>(TargetTiles);

        return clone;
    }
}
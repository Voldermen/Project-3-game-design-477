using UnityEngine;

public abstract class EnemyBehavior : ScriptableObject
{
    public abstract void Move(BoardState state, BoardUnitState enemy);
    public abstract EnemyIntentState CreateIntent(BoardState state, BoardUnitState enemy);
}
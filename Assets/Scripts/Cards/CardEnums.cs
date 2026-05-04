public enum CardPlayType
{
    Unit,
    Global
}

public enum CardTargetType
{
    None,
    Tile,
    Unit,
    FriendlyUnit,
    EnemyUnit
}

public enum CardTargetPattern
{
    Any,
    CardinalAdjacentToActingUnit
}

public enum CardEffectType
{
    None,
    Damage,
    Heal,
    Move,
    Push,
    Spawn,
    BranchTimeline
}
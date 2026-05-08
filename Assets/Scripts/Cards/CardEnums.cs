public enum CardPlayType
{
    Unit,
    Global
}

public enum CardTargetType
{
    None,
    Tile,
    EmptyTile,
    Unit,
    FriendlyUnit,
    EnemyUnit
}

public enum CardTargetPattern
{
    Any,
    CardinalAdjacentToActingUnit,
    WithinRangeOfActingUnit,
    DiagonalAdjacentToActingUnit
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
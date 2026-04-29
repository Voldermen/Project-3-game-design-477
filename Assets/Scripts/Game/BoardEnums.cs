public enum TileType
{
    Normal,
    Blocked,
    Hazard
}

public enum UnitTeam
{
    Friendly,
    Enemy
}

public enum CardTargetType
{
    None,
    Tile,
    Unit,
    FriendlyUnit,
    EnemyUnit
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
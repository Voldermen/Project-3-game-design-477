using System;
using UnityEngine;

// We'll probably build out this class a ton
// For now, it's a data-only class that contains what SHOULD be all the relevant information for any given unit
// We'll probably end up with some "modeltype" enums when it comes time to visually represent different enemy types and such

[Serializable]
public class BoardUnitState
{
    public int UnitId;
    public string UnitDefinitionId;
    public UnitTeam Team;
    public Vector2Int Position;
    public int Health;
    public int MaxHealth;
    public bool IsBase;

    public BoardUnitState Clone()
    {
        return new BoardUnitState
        {
            UnitId = UnitId,
            UnitDefinitionId = UnitDefinitionId,
            Team = Team,
            Position = Position,
            Health = Health,
            MaxHealth = MaxHealth,
            IsBase = IsBase
        };
    }
}
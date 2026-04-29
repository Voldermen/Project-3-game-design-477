using System;
using UnityEngine;

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
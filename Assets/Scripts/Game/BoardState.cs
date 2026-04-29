using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BoardState
{
    public int TurnCount;
    public int TimelineId;
    public BoardTileState[,] Tiles;
    public Dictionary<int, BoardUnitState> UnitsById = new();

    public int Width => Tiles.GetLength(0);
    public int Height => Tiles.GetLength(1);

    public BoardState(int width, int height, int timelineId, int turnCount)
    {
        TimelineId = timelineId;
        TurnCount = turnCount;
        Tiles = new BoardTileState[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tiles[x, y] = new BoardTileState(x, y);
            }
        }
    }

    public BoardState Clone()
    {
        BoardState clone = new BoardState(Width, Height, TimelineId, TurnCount);

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                clone.Tiles[x, y] = Tiles[x, y].Clone();
            }
        }

        clone.UnitsById = new Dictionary<int, BoardUnitState>();

        foreach (var pair in UnitsById)
        {
            clone.UnitsById[pair.Key] = pair.Value.Clone();
        }

        return clone;
    }

    public BoardState CloneForNextTurn()
    {
        BoardState clone = Clone();
        clone.TurnCount++;
        return clone;
    }

    public bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }

    public BoardTileState GetTile(int x, int y)
    {
        if (!IsInsideBoard(x, y))
        {
            return null;
        }

        return Tiles[x, y];
    }

    public BoardUnitState GetUnitAtTile(int x, int y)
    {
        BoardTileState tile = GetTile(x, y);

        if (tile == null || tile.OccupyingUnitId == -1)
        {
            return null;
        }

        return UnitsById.TryGetValue(tile.OccupyingUnitId, out BoardUnitState unit) ? unit : null;
    }

    public void AddUnit(BoardUnitState unit, int x, int y)
    {
        if (!IsInsideBoard(x, y))
        {
            return;
        }

        UnitsById[unit.UnitId] = unit;
        unit.Position = new Vector2Int(x, y);
        Tiles[x, y].OccupyingUnitId = unit.UnitId;
    }

    public bool MoveUnit(int unitId, int newX, int newY)
    {
        if (!UnitsById.TryGetValue(unitId, out BoardUnitState unit))
        {
            return false;
        }

        BoardTileState oldTile = GetTile(unit.Position.x, unit.Position.y);
        BoardTileState newTile = GetTile(newX, newY);

        if (oldTile == null || newTile == null)
        {
            return false;
        }

        if (newTile.OccupyingUnitId != -1)
        {
            return false;
        }

        oldTile.OccupyingUnitId = -1;
        newTile.OccupyingUnitId = unitId;
        unit.Position = new Vector2Int(newX, newY);

        return true;
    }

    public void RemoveUnit(int unitId)
    {
        if (!UnitsById.TryGetValue(unitId, out BoardUnitState unit))
        {
            return;
        }

        BoardTileState tile = GetTile(unit.Position.x, unit.Position.y);

        if (tile != null && tile.OccupyingUnitId == unitId)
        {
            tile.OccupyingUnitId = -1;
        }

        UnitsById.Remove(unitId);
    }

    public bool HasLivingBase(UnitTeam team)
    {
        foreach (var pair in UnitsById)
        {
            BoardUnitState unit = pair.Value;

            if (unit.Team == team && unit.IsBase && unit.Health > 0)
            {
                return true;
            }
        }

        return false;
    }

    public bool HasLivingEnemies()
    {
        foreach (var pair in UnitsById)
        {
            BoardUnitState unit = pair.Value;

            if (unit.Team == UnitTeam.Enemy && unit.Health > 0)
            {
                return true;
            }
        }

        return false;
    }
}
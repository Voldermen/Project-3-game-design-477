using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class BoardState
{
    // Stored information per BoardState
    public int TurnCount;
    public int TimelineId;
    public BoardTileState[,] Tiles;
    public Dictionary<int, BoardUnitState> UnitsById = new();
    public EnergyState EnergyState = new();
    public List<EnemyIntentState> EnemyIntents = new List<EnemyIntentState>();

    public int Width => Tiles.GetLength(0);
    public int Height => Tiles.GetLength(1);

    public List<BoardCollectibleState> Collectibles= new();

    public BoardState(int width, int height, int timelineId, int turnCount)
    {
        TimelineId = timelineId;
        TurnCount = turnCount;

        // Create a 2D array of tiles (and tileStates) according to the width and length
        Tiles = new BoardTileState[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tiles[x, y] = new BoardTileState(x, y);
            }
        }
    }

    // Wide spanning function that creates a deep copy of the entire BoardState and all it's data
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

        clone.EnemyIntents = new List<EnemyIntentState>();

        for (int i = 0; i < EnemyIntents.Count; i++)
        {
            clone.EnemyIntents.Add(EnemyIntents[i].Clone());
        }
        clone.Collectibles= new List<BoardCollectibleState>();
        for (int i=0; i < Collectibles.Count; i++)
        {
            clone.Collectibles.Add(Collectibles[i].Clone());
        }

        clone.EnergyState = EnergyState.Clone();

        return clone;
    }

    // Used to create the next turn's board
    public BoardState CloneForNextTurn()
    {
        BoardState clone = Clone();
        clone.TurnCount++;
        return clone;
    }

    public int GetPresentFriendlyUnitCount()
    {
        int count = 0;

        foreach (var pair in UnitsById)
        {
            BoardUnitState unit = pair.Value;


            if (unit.Team == UnitTeam.Friendly && !unit.IsBase && unit.Health > 0)
            {
                count++;
            }
        }

        return count;
    }

    public void RefreshEnergyFromFriendlyUnits()
    {
        int count = GetPresentFriendlyUnitCount();
        EnergyState.SetEnergy(GetPresentFriendlyUnitCount());
    }

    // Checks to see if a given coordinate exists within the board
    public bool IsInsideBoard(int x, int y)
    {
        // NOTE: I'm not 100% sure this boolmath is correct, break it up if bugs arise
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }

    // Gets a given TileState if it exists
    public BoardTileState GetTile(int x, int y)
    {
        if (!IsInsideBoard(x, y)) return null;

        return Tiles[x, y];
    }

    public BoardUnitState GetUnitAtTile(int x, int y)
    {
        // Check to see if the tile exists and if it contains a unit
        BoardTileState tile = GetTile(x, y);
        if (tile == null || tile.OccupyingUnitId == -1) return null;

        // If so, grabs the unit by id (or returns null if we don't have a matching id) 
        return UnitsById.TryGetValue(tile.OccupyingUnitId, out BoardUnitState unit) ? unit : null;
    }

    // Adds a unit to the board and tile
    public void AddUnit(BoardUnitState unit, int x, int y)
    {
        if (!IsInsideBoard(x, y)) return;
        if (Tiles[x, y].OccupyingUnitId != -1) return;

        UnitsById[unit.UnitId] = unit;
        unit.Position = new Vector2Int(x, y);
        Tiles[x, y].OccupyingUnitId = unit.UnitId;
    }

    // Tries to move the given unit to the given position
    public bool MoveUnit(int unitId, int x, int y)
    {
        if (!UnitsById.TryGetValue(unitId, out BoardUnitState unit))
        {
            return false;
        }

        if (!IsInsideBoard(x, y))
        {
            return false;
        }

        if (Tiles[x, y].OccupyingUnitId != -1)
        {
            return false;
        }

        Vector2Int oldPosition = unit.Position;
        Vector2Int newPosition = new Vector2Int(x, y);
        Vector2Int delta = newPosition - oldPosition;

        if (delta != Vector2Int.zero)
        {
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
            {
                unit.FacingDirection = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                unit.FacingDirection = delta.y > 0 ? Vector2Int.up : Vector2Int.down;
            }
        }

        Tiles[oldPosition.x, oldPosition.y].OccupyingUnitId = -1;
        Tiles[x, y].OccupyingUnitId = unitId;

        unit.Position = newPosition;

        return true;
    }

    public void RemoveUnit(int unitId)
    {
        // Do nothing if the unit isn't in the list
        if (!UnitsById.TryGetValue(unitId, out BoardUnitState unit)) return;

        // Remove the unit from it's occupying tile
        BoardTileState tile = GetTile(unit.Position.x, unit.Position.y);
        if (tile != null && tile.OccupyingUnitId == unitId) tile.OccupyingUnitId = -1;

        // Remove the unitId from the list
        UnitsById.Remove(unitId);
    }

    // This function is generalized to teams instead of just the player
    // In case we want bosses to count as bases
    public bool HasLivingBase(UnitTeam team)
    {
        // Just scans for a unit flagged as base that matches the given team
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
        // Scans the active units for any enemies
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

    public void FriendlyBuffCountdown() // counts down the turns for friendly buffs for buff cards.
    {
       foreach (var pair in UnitsById)
        {
            BoardUnitState unit= pair.Value;
            if(unit.Team != UnitTeam.Friendly)
            {
                continue;
            }

            if (unit.strengthTurnsRemaining > 0)
            {
                unit.strengthTurnsRemaining--;

                if (unit.strengthTurnsRemaining <= 0)
                {
                    unit.strengthUp=0;
                }
            }
        } 
    }

    public BoardCollectibleState GetCollectibleAtTile(int x, int y)
    {
        for (int i=0; i < Collectibles.Count; i++)
        {
            BoardCollectibleState collectible= Collectibles[i];

            if (collectible.Position.x== x && collectible.Position.y == y)
            {
                return collectible;
            }
        }
        return null;
    }

    public void AddCollectible(BoardCollectibleState collectible)
    {
        if (collectible== null)
        {
            return;
        }
        if (!IsInsideBoard(collectible.Position.x, collectible.Position.y))
        {
            return;
        }

        if ( GetCollectibleAtTile(collectible.Position.x, collectible.Position.y)!= null)
        {
            return;
        }
        Collectibles.Add(collectible);
    }
    public bool RemoveCollectible(int collectibleId)
    {
        for (int i=0; i< Collectibles.Count; i++)
        {
            if (Collectibles[i].CollectibleId== collectibleId)
            {
                Collectibles.RemoveAt(i);
                return true;
            }
        }
        return false;
    }
}
using System;

// This class doesn't hold too much data
// It has a position, a TileType, and the UnitID of the occupying unit
// It also possesses a Clone function used by the BoardState during it's deepcopy


[Serializable]
public class BoardTileState
{
    public int X;
    public int Y;
    public TileType TileType;
    public int OccupyingUnitId = -1;

    public BoardTileState(int x, int y)
    {
        X = x;
        Y = y;
        TileType = TileType.Normal;
    }

    public BoardTileState Clone()
    {
        return new BoardTileState(X, Y)
        {
            TileType = TileType,
            OccupyingUnitId = OccupyingUnitId
        };
    }
}
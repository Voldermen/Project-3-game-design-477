using System;

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
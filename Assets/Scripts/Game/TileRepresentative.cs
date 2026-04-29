using UnityEngine;

public class TileRepresentative : MonoBehaviour
{
    public int X { get; private set; }
    public int Y { get; private set; }

    private BoardRepresentative boardRepresentative;

    public void Initialize(int x, int y, BoardRepresentative owner)
    {
        X = x;
        Y = y;
        boardRepresentative = owner;
    }

    public void Render(BoardTileState tileState)
    {
    }

    private void OnMouseEnter()
    {
        boardRepresentative.OnTileHovered(this);
    }

    private void OnMouseDown()
    {
        boardRepresentative.OnTileClicked(this);
    }
}
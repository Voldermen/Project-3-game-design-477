using UnityEngine;

// A MonoBehaviour script, Exists just to render each TileState given by the boardRepresentative
public class TileRepresentative : MonoBehaviour
{
    public int X { get; private set; }
    public int Y { get; private set; }

    private BoardRepresentative boardRepresentative;

    private void Start()
    {
        print("test");
    }
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
        print("tile hovered");
    }

    private void OnMouseDown()
    {
        boardRepresentative.OnTileClicked(this);
        print("Mousedown on tile");
    }
}
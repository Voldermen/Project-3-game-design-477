using UnityEngine;

public class TileRepresentative : MonoBehaviour
{
    [SerializeField] private Renderer tileRenderer;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material hoveredMaterial;
    [SerializeField] public float hoverRaiseAmount = 0.15f;

    public int X { get; private set; }
    public int Y { get; private set; }

    private BoardRepresentative boardRepresentative;
    private Vector3 baseLocalPosition;
    private bool isHovered;

    public void Initialize(int x, int y, BoardRepresentative owner)
    {
        X = x;
        Y = y;
        boardRepresentative = owner;
        baseLocalPosition = transform.localPosition;
        SetHovered(false);
    }

    public void Render(BoardTileState tileState)
    {
        if (!isHovered)
        {
            tileRenderer.material = normalMaterial;
        }
    }

    public void SetHovered(bool hovered)
    {
        isHovered = hovered;

        transform.localPosition = baseLocalPosition + (hovered ? Vector3.up * hoverRaiseAmount : Vector3.zero);

        if (tileRenderer != null)
        {
            tileRenderer.material = hovered ? hoveredMaterial : normalMaterial;
        }
    }

    public void Click()
    {
        boardRepresentative.OnTileClicked(this);
    }
}
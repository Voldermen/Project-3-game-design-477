using UnityEngine;
using UnityEngine.InputSystem;

public class BoardMouseInput : MonoBehaviour
{
    [SerializeField] private Camera boardCamera;
    [SerializeField] private LayerMask tileLayerMask;

    private TileRepresentative hoveredTile;

    private void Update()
    {
        UpdateHover();
        UpdateClick();
    }

    private void UpdateHover()
    {
        TileRepresentative newHoveredTile = GetTileUnderMouse();

        if (newHoveredTile == hoveredTile)
        {
            return;
        }

        if (hoveredTile != null)
        {
            hoveredTile.SetHovered(false);
        }

        hoveredTile = newHoveredTile;

        if (hoveredTile != null)
        {
            hoveredTile.SetHovered(true);
        }
    }

    private void UpdateClick()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (hoveredTile != null)
        {
            Debug.Log($"Clicked tile {hoveredTile.X}, {hoveredTile.Y}");
            hoveredTile.Click();
        }
    }

    private TileRepresentative GetTileUnderMouse()
    {
        if (Mouse.current == null || boardCamera == null)
        {
            return null;
        }

        Ray ray = boardCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, tileLayerMask))
        {
            return null;
        }

        return hit.collider.GetComponentInParent<TileRepresentative>();
    }
}
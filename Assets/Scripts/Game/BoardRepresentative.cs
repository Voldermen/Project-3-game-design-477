using System.Collections.Generic;
using UnityEngine;

public class BoardRepresentative : MonoBehaviour
{
    [SerializeField] private TileRepresentative tilePrefab;
    [SerializeField] private BoardUnitRepresentative unitPrefab;
    [SerializeField] private Transform tileRoot;
    [SerializeField] private Transform unitRoot;
    [SerializeField] private float tileSpacing = 1f;

    private TileRepresentative[,] tileRepresentatives;
    private readonly Dictionary<int, BoardUnitRepresentative> unitRepresentativesById = new();

    public void Render(BoardState boardState)
    {
        EnsureTiles(boardState);
        RenderTiles(boardState);
        RenderUnits(boardState);
    }

    // Do nothing if our tiles already exist and match the given boardState
    // Otherwise create them
    private void EnsureTiles(BoardState boardState)
    {
        if (tileRepresentatives != null &&
            tileRepresentatives.GetLength(0) == boardState.Width &&
            tileRepresentatives.GetLength(1) == boardState.Height) return;
        
        ClearChildren(tileRoot);

        tileRepresentatives = new TileRepresentative[boardState.Width, boardState.Height];

        for (int x = 0; x < boardState.Width; x++)
        {
            for (int y = 0; y < boardState.Height; y++)
            {
                TileRepresentative tile = Instantiate(tilePrefab, tileRoot);
                tile.transform.localPosition = GridToWorld(x, y);
                tile.Initialize(x, y, this);
                tileRepresentatives[x, y] = tile;
            }
        }
    }

    // Call Render on each tileRepresentative
    private void RenderTiles(BoardState boardState)
    {
        for (int x = 0; x < boardState.Width; x++)
        {
            for (int y = 0; y < boardState.Height; y++)
            {
                tileRepresentatives[x, y].Render(boardState.Tiles[x, y]);
            }
        }
    }

    // This one is a bit tricky
    private void RenderUnits(BoardState boardState)
    {
        List<int> idsToRemove = new();

        // If a logged unitRepresentative no longer exists on the boardState (it's died or we've move timeline)
        // then we log its ID for removal
        foreach (var pair in unitRepresentativesById)
        {
            if (!boardState.UnitsById.ContainsKey(pair.Key))
            {
                idsToRemove.Add(pair.Key);
            }
        }

        // For each id we've logged to remove, destroy it's represtative gameobject and remove the id from our list
        for (int i = 0; i < idsToRemove.Count; i++)
        {
            int unitId = idsToRemove[i];
            Destroy(unitRepresentativesById[unitId].gameObject);
            unitRepresentativesById.Remove(unitId);
        }

        // For each id that remains
        foreach (var pair in boardState.UnitsById)
        {
            // Grab the corresponding state
            BoardUnitState unitState = pair.Value;

            // Create a representative if none exists
            if (!unitRepresentativesById.TryGetValue(unitState.UnitId, out BoardUnitRepresentative unitRep))
            {
                unitRep = Instantiate(unitPrefab, unitRoot);
                unitRepresentativesById[unitState.UnitId] = unitRep;
            }

            // Render the representative
            unitRep.Render(unitState);
            unitRep.transform.localPosition = GridToWorld(unitState.Position.x, unitState.Position.y);
        }
    }

    // Returns the world coordinates of a given tile
    public Vector3 GridToWorld(int x, int y)
    {
        return new Vector3(x * tileSpacing, 0f, y * tileSpacing);
    }

    // TODO: Fill out these two functions, they'll probably end up being pretty expansive
    public void OnTileClicked(TileRepresentative tile)
    {
    }

    public void OnTileHovered(TileRepresentative tile)
    {
    }

    // Destroys all children (should only be tileRepresentatives)
    private void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Destroy(root.GetChild(i).gameObject);
        }
    }
}
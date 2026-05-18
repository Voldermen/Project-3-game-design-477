using System.Collections.Generic;
using UnityEngine;

public class BoardRepresentative : MonoBehaviour
{
    [SerializeField] private TileRepresentative tilePrefab;
    [SerializeField] private BoardUnitRepresentative unitPrefab;
    [SerializeField] private Transform tileRoot;
    [SerializeField] private Transform unitRoot;
    [SerializeField] private float tileSpacing = 1f;
    [SerializeField] private SelectionController selectionController;
    [SerializeField] private UnitDatabase unitDatabase;
    [SerializeField] private DangerTileRepresentative dangerPrefab;
    [SerializeField] private Transform dangerRoot;
    [SerializeField] private UnitHealthHoverWidget healthHoverWidget;
    [SerializeField] private float healthHoverHeight = 1.5f;
    [SerializeField] private CollectibleRepresentative collectiblePrefab;
    [SerializeField] private Transform collectibleRoot;

    private bool IsHoverTile;
    private int HoverTileX;
    private int HoverTileY;
    private readonly List<CollectibleRepresentative> collectibleRepresentatives= new();

    private BoardState lastRenderedBoardState;

    private readonly List<DangerTileRepresentative> dangerTiles = new();

    private TileRepresentative[,] tileRepresentatives;
    private readonly Dictionary<int, BoardUnitRepresentative> unitRepresentativesById = new();

    public void Render(BoardState boardState)
    {
        
        lastRenderedBoardState = boardState;

        EnsureTiles(boardState);
        RenderTiles(boardState);
        RenderUnits(boardState);
        RenderDangerTiles(boardState);
        RenderCollectibles(boardState);
        RefeshHoveredHealth();
    }

    private void RenderDangerTiles(BoardState state)
    {
        ClearDangerTiles();

        if (dangerPrefab == null)
        {
            return;
        }

        if (dangerRoot == null)
        {
            return;
        }

        foreach (var intent in state.EnemyIntents)
        {
            foreach (var tile in intent.TargetTiles)
            {

                DangerTileRepresentative danger = Instantiate(dangerPrefab, dangerRoot);
                danger.transform.localPosition = GridToWorld(tile.x, tile.y);
                dangerTiles.Add(danger);
            }
        }
    }

    private void ClearDangerTiles()
    {
        for (int i = dangerTiles.Count - 1; i >= 0; i--)
        {
            if (dangerTiles[i] != null)
            {
                Destroy(dangerTiles[i].gameObject);
            }
        }

        dangerTiles.Clear();
    }

    private void EnsureTiles(BoardState boardState)
    {
        if (tileRepresentatives != null &&
            tileRepresentatives.GetLength(0) == boardState.Width &&
            tileRepresentatives.GetLength(1) == boardState.Height)
        {
            return;
        }

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

    private void RenderUnits(BoardState boardState)
    {
        List<int> idsToRemove = new();

        foreach (var pair in unitRepresentativesById)
        {
            if (!boardState.UnitsById.ContainsKey(pair.Key))
            {
                idsToRemove.Add(pair.Key);
            }
        }

        for (int i = 0; i < idsToRemove.Count; i++)
        {
            int unitId = idsToRemove[i];

            if (unitRepresentativesById[unitId] != null)
            {
                Destroy(unitRepresentativesById[unitId].gameObject);
            }

            unitRepresentativesById.Remove(unitId);
        }

        foreach (var pair in boardState.UnitsById)
        {
            BoardUnitState unitState = pair.Value;

            if (!unitRepresentativesById.TryGetValue(unitState.UnitId, out BoardUnitRepresentative unitRep))
            {
                unitRep = Instantiate(unitPrefab, unitRoot);
                unitRep.Initialize(unitDatabase);
                unitRepresentativesById[unitState.UnitId] = unitRep;
            }

            unitRep.Render(unitState);
            unitRep.transform.localPosition = GridToWorld(unitState.Position.x, unitState.Position.y);
        }
    }

    public Vector3 GridToWorld(int x, int y)
    {
        return new Vector3(x * tileSpacing, 0f, y * tileSpacing);
    }

    public void OnTileClicked(TileRepresentative tile)
    {

        if (selectionController == null)
        {
            return;
        }

        selectionController.ClickTile(tile.X, tile.Y);
    }

    public void OnTileHovered(TileRepresentative tile)
    {

        IsHoverTile=true;
        HoverTileX= tile.X;
        HoverTileY= tile.Y;

        if (selectionController != null)
        {
            selectionController.HoverTile(tile.X, tile.Y);
        }
        RefeshHoveredHealth();
        if (healthHoverWidget == null || lastRenderedBoardState == null)
        {
            return;
        }

        BoardUnitState unit = lastRenderedBoardState.GetUnitAtTile(tile.X, tile.Y);

        if (unit == null)
        {
            healthHoverWidget.Hide();
            return;
        }

        Vector3 localPos = GridToWorld(unit.Position.x, unit.Position.y) + Vector3.up * healthHoverHeight;
        Vector3 worldPos = transform.TransformPoint(localPos);

        healthHoverWidget.Show(unit, worldPos);
    }

    public void ClearHoveredTile()
    {
        IsHoverTile=false;
        if (healthHoverWidget != null)
        {
            healthHoverWidget.Hide();
        }
    }

    private void ClearChildren(Transform root)
    {
        if (root == null)
        {
            return;
        }

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Destroy(root.GetChild(i).gameObject);
        }
    }

    private void RenderCollectibles(BoardState state)
    {
        ClearCollectibles();

        if (collectiblePrefab== null || collectibleRoot== null)
        {
            return;
        }

        for (int i=0; i< state.Collectibles.Count; i++)
        {
            BoardCollectibleState collectible= state.Collectibles[i];

            CollectibleRepresentative rep= Instantiate( collectiblePrefab,collectibleRoot);
            rep.transform.localPosition= GridToWorld(collectible.Position.x, collectible.Position.y) + Vector3.up * 1f;
            rep.Render(collectible);

            collectibleRepresentatives.Add(rep);

            Debug.Log($"Rendering {state.Collectibles.Count} collectibles. Prefab= {collectiblePrefab}, Root={collectibleRoot}");
            Debug.Log($"Spawned collectible visual at {rep.transform.localPosition}");
        }
    }

    private void ClearCollectibles()
    {
        for (int i= collectibleRepresentatives.Count -1; i >=0; i--)
        {
            if (collectibleRepresentatives[i] != null)
            {
                Destroy(collectibleRepresentatives[i].gameObject);
            }
        }
        collectibleRepresentatives.Clear();
    }

    private void RefeshHoveredHealth()
    {
        if (!IsHoverTile)
        {
            return;
        }

        if (healthHoverWidget == null || lastRenderedBoardState == null)
        {
            return;
        }

        BoardUnitState unit= lastRenderedBoardState.GetUnitAtTile(HoverTileX, HoverTileY);

        if (unit == null)
        {
            healthHoverWidget.Hide();
            return;
        }

        Vector3 localPosition= GridToWorld(unit.Position.x, unit.Position.y) + Vector3.up* healthHoverHeight;
        Vector3 worldPosition= transform.TransformPoint(localPosition);
        healthHoverWidget.Show(unit, worldPosition);
    }
}
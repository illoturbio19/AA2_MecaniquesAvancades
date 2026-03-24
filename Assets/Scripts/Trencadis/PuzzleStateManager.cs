using System.Collections.Generic;
using UnityEngine;

public class PuzzleStateManager : MonoBehaviour
{
    [System.Serializable]
    public class PuzzleTargetCell
    {
        public Vector2Int gridPosition;
        public string requiredTileId;
    }

    [Header("References")]
    [SerializeField] private MosaicGrid mosaicGrid;

    [Header("Puzzle Target Layout")]
    [SerializeField] private List<PuzzleTargetCell> targetCells = new List<PuzzleTargetCell>();

    private Dictionary<Vector2Int, string> targetLookup = new Dictionary<Vector2Int, string>();

    public int TargetCellCount => targetLookup.Count;

    private void Awake()
    {
        BuildLookup();
    }

    public void BuildLookup()
    {
        targetLookup.Clear();

        for (int i = 0; i < targetCells.Count; i++)
        {
            PuzzleTargetCell cell = targetCells[i];

            if (!targetLookup.ContainsKey(cell.gridPosition))
            {
                targetLookup.Add(cell.gridPosition, cell.requiredTileId);
            }
        }
    }

    public bool HasTargetForPosition(Vector2Int gridPosition)
    {
        return targetLookup.ContainsKey(gridPosition);
    }

    public string GetRequiredTileId(Vector2Int gridPosition)
    {
        if (!targetLookup.ContainsKey(gridPosition))
            return string.Empty;

        return targetLookup[gridPosition];
    }

    public bool IsTileCorrectAtPosition(Vector2Int gridPosition)
    {
        if (mosaicGrid == null)
            return false;

        if (!targetLookup.ContainsKey(gridPosition))
            return false;

        MosaicTile placedTile = mosaicGrid.GetTileAt(gridPosition);

        if (placedTile == null || placedTile.TileData == null)
            return false;

        return placedTile.TileData.tileId == targetLookup[gridPosition];
    }

    public int GetCorrectPlacedTilesCount()
    {
        int count = 0;

        foreach (KeyValuePair<Vector2Int, string> entry in targetLookup)
        {
            if (IsTileCorrectAtPosition(entry.Key))
            {
                count++;
            }
        }

        return count;
    }

    public bool IsPuzzleComplete()
    {
        if (targetLookup.Count == 0)
            return false;

        foreach (KeyValuePair<Vector2Int, string> entry in targetLookup)
        {
            if (!IsTileCorrectAtPosition(entry.Key))
            {
                return false;
            }
        }

        return true;
    }

    public float GetCompletionRatio()
    {
        if (targetLookup.Count == 0)
            return 0f;

        return (float)GetCorrectPlacedTilesCount() / targetLookup.Count;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        for (int i = 0; i < targetCells.Count; i++)
        {
            if (mosaicGrid == null)
                continue;

            Vector3 pos = mosaicGrid.GetWorldPosition(targetCells[i].gridPosition);
            Gizmos.DrawSphere(pos, 0.15f);
        }
    }
#endif
}
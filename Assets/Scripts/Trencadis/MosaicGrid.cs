using System.Collections.Generic;
using UnityEngine;

public class MosaicGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector3 originPosition = Vector3.zero;

    private MosaicTile[,] gridArray;

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;

    private void Awake()
    {
        InitializeGrid();
    }

    public void InitializeGrid()
    {
        gridArray = new MosaicTile[width, height];
    }

    public bool IsValidGridPosition(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 &&
               gridPosition.x < width &&
               gridPosition.y >= 0 &&
               gridPosition.y < height;
    }

    public bool IsCellEmpty(Vector2Int gridPosition)
    {
        if (!IsValidGridPosition(gridPosition))
            return false;

        return gridArray[gridPosition.x, gridPosition.y] == null;
    }

    public bool PlaceTile(MosaicTile tile, Vector2Int gridPosition)
    {
        if (tile == null)
            return false;

        if (!IsValidGridPosition(gridPosition))
            return false;

        if (!IsCellEmpty(gridPosition))
            return false;

        gridArray[gridPosition.x, gridPosition.y] = tile;
        tile.transform.position = GetWorldPosition(gridPosition);
        tile.SetPlaced(true, gridPosition);

        return true;
    }

    public bool RemoveTile(Vector2Int gridPosition)
    {
        if (!IsValidGridPosition(gridPosition))
            return false;

        MosaicTile tile = gridArray[gridPosition.x, gridPosition.y];

        if (tile == null)
            return false;

        tile.SetPlaced(false, new Vector2Int(-1, -1));
        gridArray[gridPosition.x, gridPosition.y] = null;

        return true;
    }

    public MosaicTile GetTileAt(Vector2Int gridPosition)
    {
        if (!IsValidGridPosition(gridPosition))
            return null;

        return gridArray[gridPosition.x, gridPosition.y];
    }

    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return originPosition + new Vector3(gridPosition.x * cellSize, gridPosition.y * cellSize, 0f);
    }

    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - originPosition;

        int x = Mathf.RoundToInt(localPosition.x / cellSize);
        int y = Mathf.RoundToInt(localPosition.y / cellSize);

        return new Vector2Int(x, y);
    }

    public bool HasAnyTile()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gridArray[x, y] != null)
                    return true;
            }
        }

        return false;
    }

    public int GetPlacedTileCount()
    {
        int count = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gridArray[x, y] != null)
                    count++;
            }
        }

        return count;
    }

    public List<Vector2Int> GetAllOccupiedPositions()
    {
        List<Vector2Int> occupiedPositions = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gridArray[x, y] != null)
                {
                    occupiedPositions.Add(new Vector2Int(x, y));
                }
            }
        }

        return occupiedPositions;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 cellCenter = originPosition + new Vector3(x * cellSize, y * cellSize, 0f);
                Gizmos.DrawWireCube(cellCenter, Vector3.one * cellSize * 0.95f);
            }
        }
    }
#endif
}
using System.Collections.Generic;
using UnityEngine;

public class TrencadisGenerator : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private MosaicTile tilePrefab;

    [Header("Tile Data Pool")]
    [SerializeField] private List<TileShapeData> tileDatas = new List<TileShapeData>();

    [Header("Spawn Layout")]
    [SerializeField] private Transform spawnOrigin;
    [SerializeField] private int columns = 4;
    [SerializeField] private float spacingX = 1.25f;
    [SerializeField] private float spacingY = 1.25f;

    [Header("Generated Tiles")]
    [SerializeField] private List<MosaicTile> generatedTiles = new List<MosaicTile>();

    public List<MosaicTile> GeneratedTiles => generatedTiles;

    [ContextMenu("Generate Tiles")]
    public void GenerateTiles()
    {
        ClearGeneratedTiles();

        if (tilePrefab == null || spawnOrigin == null)
            return;

        for (int i = 0; i < tileDatas.Count; i++)
        {
            TileShapeData data = tileDatas[i];

            int row = i / columns;
            int column = i % columns;

            Vector3 spawnPosition = spawnOrigin.position + new Vector3(column * spacingX, -row * spacingY, 0f);

            MosaicTile newTile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity, transform);
            newTile.name = $"Tile_{data.tileId}";
            newTile.Initialize(data);

            generatedTiles.Add(newTile);
        }
    }

    [ContextMenu("Clear Generated Tiles")]
    public void ClearGeneratedTiles()
    {
        for (int i = generatedTiles.Count - 1; i >= 0; i--)
        {
            if (generatedTiles[i] != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(generatedTiles[i].gameObject);
                else
                    Destroy(generatedTiles[i].gameObject);
#else
                Destroy(generatedTiles[i].gameObject);
#endif
            }
        }

        generatedTiles.Clear();
    }
}
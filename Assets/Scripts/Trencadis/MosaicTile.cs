using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class MosaicTile : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private TileShapeData tileData;

    [Header("Runtime Data")]
    [SerializeField] private bool isPlaced;
    [SerializeField] private bool isSelected;
    [SerializeField] private Vector2Int currentGridPosition = new Vector2Int(-1, -1);

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public TileShapeData TileData => tileData;
    public bool IsPlaced => isPlaced;
    public bool IsSelected => isSelected;
    public Vector2Int CurrentGridPosition => currentGridPosition;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (tileData != null)
        {
            ApplyTileDataVisuals();
        }
    }

    public void Initialize(TileShapeData data)
    {
        tileData = data;
        ApplyTileDataVisuals();
        isPlaced = false;
        isSelected = false;
        currentGridPosition = new Vector2Int(-1, -1);
    }

    private void ApplyTileDataVisuals()
    {
        if (tileData == null || spriteRenderer == null)
            return;

        spriteRenderer.sprite = tileData.tileSprite;
        spriteRenderer.color = tileData.tileColor;
        originalColor = tileData.tileColor;

        transform.rotation = Quaternion.Euler(0f, 0f, tileData.defaultRotation);
    }

    public void SetPlaced(bool placed, Vector2Int gridPosition)
    {
        isPlaced = placed;
        currentGridPosition = placed ? gridPosition : new Vector2Int(-1, -1);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (spriteRenderer == null)
            return;

        if (selected)
        {
            spriteRenderer.color = Color.Lerp(originalColor, Color.white, 0.35f);
        }
        else
        {
            spriteRenderer.color = originalColor;
        }
    }

    public void RotateClockwise(float angle = 90f)
    {
        if (tileData == null || !tileData.canRotate)
            return;

        transform.Rotate(0f, 0f, -angle);
    }

    public void ResetVisual()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        isSelected = false;
    }

    public TileEmotionType GetEmotionType()
    {
        if (tileData == null)
            return TileEmotionType.Neutral;

        return tileData.emotionType;
    }
}
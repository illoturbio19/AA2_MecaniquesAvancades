using UnityEngine;

public class SelectionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;

    [Header("Selection")]
    [SerializeField] private MosaicTile currentlySelectedTile;

    public MosaicTile CurrentlySelectedTile => currentlySelectedTile;
    public bool HasSelection => currentlySelectedTile != null;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleTileSelection();
    }

    private void HandleTileSelection()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mouseWorld2D = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);

        RaycastHit2D hit = Physics2D.Raycast(mouseWorld2D, Vector2.zero);

        if (hit.collider == null)
            return;

        MosaicTile clickedTile = hit.collider.GetComponent<MosaicTile>();

        if (clickedTile == null)
            return;

        if (clickedTile.IsPlaced)
            return;

        SelectTile(clickedTile);
    }

    public void SelectTile(MosaicTile tile)
    {
        if (tile == null)
            return;

        if (currentlySelectedTile == tile)
            return;

        DeselectCurrentTile();

        currentlySelectedTile = tile;
        currentlySelectedTile.SetSelected(true);
    }

    public void DeselectCurrentTile()
    {
        if (currentlySelectedTile == null)
            return;

        currentlySelectedTile.SetSelected(false);
        currentlySelectedTile = null;
    }

    public MosaicTile ConsumeSelectedTile()
    {
        MosaicTile tile = currentlySelectedTile;

        if (currentlySelectedTile != null)
        {
            currentlySelectedTile.SetSelected(false);
            currentlySelectedTile = null;
        }

        return tile;
    }
}
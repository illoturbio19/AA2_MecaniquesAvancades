using UnityEngine;

public class PlacementController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private MosaicGrid mosaicGrid;
    [SerializeField] private SelectionController selectionController;
    [SerializeField] private GameLoopManager gameLoopManager;
    [SerializeField] private EmotionCombatManager emotionCombatManager;

    [Header("Controls")]
    [SerializeField] private KeyCode rotateKey = KeyCode.R;
    [SerializeField] private KeyCode cancelSelectionKey = KeyCode.Escape;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        if (gameLoopManager != null && !gameLoopManager.IsGamePlayable)
            return;

        HandleRotationInput();
        HandleCancelInput();
        HandlePlacementInput();
    }

    private void HandleRotationInput()
    {
        if (!Input.GetKeyDown(rotateKey))
            return;

        if (selectionController == null || !selectionController.HasSelection)
            return;

        selectionController.CurrentlySelectedTile.RotateClockwise();
    }

    private void HandleCancelInput()
    {
        bool cancelByMouse = Input.GetMouseButtonDown(1);
        bool cancelByKey = Input.GetKeyDown(cancelSelectionKey);

        if (!cancelByMouse && !cancelByKey)
            return;

        if (selectionController == null)
            return;

        selectionController.DeselectCurrentTile();
    }

    private void HandlePlacementInput()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (selectionController == null || !selectionController.HasSelection)
            return;

        if (mosaicGrid == null)
            return;

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mouseWorld2D = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);

        RaycastHit2D hit = Physics2D.Raycast(mouseWorld2D, Vector2.zero);

        if (hit.collider != null && hit.collider.GetComponent<MosaicTile>() != null)
            return;

        Vector2Int targetGridPosition = mosaicGrid.GetGridPosition(mouseWorldPosition);

        if (!mosaicGrid.IsValidGridPosition(targetGridPosition))
            return;

        if (!mosaicGrid.IsCellEmpty(targetGridPosition))
            return;

        MosaicTile selectedTile = selectionController.CurrentlySelectedTile;

        bool placedSuccessfully = mosaicGrid.PlaceTile(selectedTile, targetGridPosition);

        if (!placedSuccessfully)
            return;

        if (gameLoopManager != null)
        {
            gameLoopManager.SpendMove(1);
        }

        if (emotionCombatManager != null)
        {
            emotionCombatManager.ResolveTileEmotion(selectedTile);
        }

        if (gameLoopManager != null)
        {
            gameLoopManager.OnTilePlacementResolved();
        }

        selectionController.DeselectCurrentTile();
    }
}
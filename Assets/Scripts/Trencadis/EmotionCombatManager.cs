using System.Collections.Generic;
using UnityEngine;

public enum EmotionCombatResult
{
    PositiveSuccess,
    NeutralSuccess,
    NegativeSuccess
}

public class EmotionCombatManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MosaicGrid mosaicGrid;
    [SerializeField] private GameLoopManager gameLoopManager;

    [Header("Positive Result")]
    [SerializeField] private int positiveBonusMoves = 2;

    [Header("Negative Result")]
    [SerializeField] private int negativeTilesToRemove = 1;
    [SerializeField] private bool negativeAlsoCostsExtraMove = false;
    [SerializeField] private int negativeExtraMoveCost = 1;

    public EmotionCombatResult ResolveTileEmotion(MosaicTile tile)
    {
        if (tile == null)
            return EmotionCombatResult.NeutralSuccess;

        TileEmotionType emotionType = tile.GetEmotionType();

        switch (emotionType)
        {
            case TileEmotionType.Positive:
                ApplyPositiveEffect();
                return EmotionCombatResult.PositiveSuccess;

            case TileEmotionType.Negative:
                ApplyNegativeEffect();
                return EmotionCombatResult.NegativeSuccess;

            case TileEmotionType.Neutral:
            default:
                return EmotionCombatResult.NeutralSuccess;
        }
    }

    private void ApplyPositiveEffect()
    {
        if (gameLoopManager != null)
        {
            gameLoopManager.AddMoves(positiveBonusMoves);
        }
    }

    private void ApplyNegativeEffect()
    {
        RemoveRandomPlacedTiles(negativeTilesToRemove);

        if (negativeAlsoCostsExtraMove && gameLoopManager != null)
        {
            gameLoopManager.RemoveMoves(negativeExtraMoveCost);
        }
    }

    private void RemoveRandomPlacedTiles(int amount)
    {
        if (mosaicGrid == null || amount <= 0)
            return;

        List<Vector2Int> occupiedCells = mosaicGrid.GetAllOccupiedPositions();

        if (occupiedCells.Count == 0)
            return;

        int removals = Mathf.Min(amount, occupiedCells.Count);

        for (int i = 0; i < removals; i++)
        {
            if (occupiedCells.Count == 0)
                break;

            int randomIndex = Random.Range(0, occupiedCells.Count);
            Vector2Int selectedPosition = occupiedCells[randomIndex];
            occupiedCells.RemoveAt(randomIndex);

            mosaicGrid.RemoveTile(selectedPosition);
        }
    }
}
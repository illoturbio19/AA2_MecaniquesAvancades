using UnityEngine;

public class GoalZone2D : MonoBehaviour
{
    [Header("Goal Settings")]
    [SerializeField] private string requiredPieceId = "MainPiece";
    [SerializeField] private bool destroyPieceOnSuccess = false;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer targetSpriteRenderer;
    [SerializeField] private Sprite completedSprite;

    private bool completed;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (completed) return;

        PieceController2D piece = other.GetComponent<PieceController2D>();
        if (piece == null) return;
        if (piece.PieceId != requiredPieceId) return;

        completed = true;

        if (targetSpriteRenderer != null && completedSprite != null)
        {
            targetSpriteRenderer.sprite = completedSprite;
        }

        Debug.Log("LEVEL COMPLETE");

        if (destroyPieceOnSuccess)
        {
            Destroy(piece.gameObject);
        }
    }
}
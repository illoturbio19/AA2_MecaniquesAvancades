using UnityEngine;

public class GoalZone2D : MonoBehaviour
{
    [SerializeField] private string requiredPieceId = "MainPiece";
    [SerializeField] private bool destroyPieceOnSuccess = false;

    private bool completed;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (completed) return;

        PieceController2D piece = other.GetComponent<PieceController2D>();
        if (piece == null) return;
        if (piece.PieceId != requiredPieceId) return;

        completed = true;
        Debug.Log("LEVEL COMPLETE");

        if (destroyPieceOnSuccess)
        {
            Destroy(piece.gameObject);
        }
    }
}
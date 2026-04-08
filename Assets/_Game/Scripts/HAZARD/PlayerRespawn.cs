using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Current Respawn")]
    [SerializeField] private Transform currentRespawnPoint;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerSFX playerSFX;

    public Transform CurrentRespawnPoint => currentRespawnPoint;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (playerSFX == null) playerSFX = GetComponent<PlayerSFX>();
    }

    public void Die()
    {
        if (currentRespawnPoint == null)
        {
            Debug.LogWarning("No hi ha cap respawn point assignat al PlayerRespawn.");
            return;
        }

        playerSFX?.PlayDeath();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        transform.position = currentRespawnPoint.position;
    }

    public void SetRespawnPoint(Transform newPoint)
    {
        if (newPoint == null) return;
        currentRespawnPoint = newPoint;
    }

    public void RespawnPiece(PieceController2D piece)
    {
        if (piece == null) return;
        if (currentRespawnPoint == null)
        {
            Debug.LogWarning("No hi ha cap respawn point assignat per recol·locar la peça.");
            return;
        }

        piece.BreakAndRespawn(currentRespawnPoint.position);
    }
}
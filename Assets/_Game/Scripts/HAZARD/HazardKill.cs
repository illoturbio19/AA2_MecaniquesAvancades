using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AudioSource))]
public class HazardKill : MonoBehaviour
{
    [Header("Piece Break Audio")]
    [SerializeField] private AudioClip pieceBreakClip;
    [SerializeField] private float pieceBreakVolume = 1f;
    [SerializeField] private float randomPitchMin = 0.95f;
    [SerializeField] private float randomPitchMax = 1.05f;

    private AudioSource audioSource;
    private PlayerRespawn playerRespawn;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;

        playerRespawn = FindFirstObjectByType<PlayerRespawn>();
    }

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1) Si toca al player, muere
        if (other.CompareTag("Player"))
        {
            PlayerRespawn player = other.GetComponentInParent<PlayerRespawn>();
            if (player != null)
            {
                player.Die();
                return;
            }
        }

        // 2) Si toca una pieza, se rompe y vuelve al respawn actual del player
        PieceController2D piece = other.GetComponentInParent<PieceController2D>();
        if (piece != null)
        {
            PlayPieceBreakSFX();

            if (playerRespawn != null)
            {
                playerRespawn.RespawnPiece(piece);
            }
        }
    }

    private void PlayPieceBreakSFX()
    {
        if (pieceBreakClip == null || audioSource == null) return;

        audioSource.pitch = Random.Range(randomPitchMin, randomPitchMax);
        audioSource.PlayOneShot(pieceBreakClip, pieceBreakVolume);
        audioSource.pitch = 1f;
    }
}
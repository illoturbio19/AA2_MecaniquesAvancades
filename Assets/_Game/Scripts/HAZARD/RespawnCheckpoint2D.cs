using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RespawnCheckpoint2D : MonoBehaviour
{
    [Header("Respawn Target")]
    [SerializeField] private Transform respawnPoint;

    [Header("Options")]
    [SerializeField] private bool useOnlyOnce = false;

    private bool alreadyUsed = false;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Awake()
    {
        if (respawnPoint == null && transform.childCount > 0)
        {
            respawnPoint = transform.GetChild(0);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (useOnlyOnce && alreadyUsed) return;
        if (!other.CompareTag("Player")) return;

        PlayerRespawn playerRespawn = other.GetComponentInParent<PlayerRespawn>();
        if (playerRespawn == null) return;
        if (respawnPoint == null) return;

        playerRespawn.SetRespawnPoint(respawnPoint);
        alreadyUsed = true;
    }
}
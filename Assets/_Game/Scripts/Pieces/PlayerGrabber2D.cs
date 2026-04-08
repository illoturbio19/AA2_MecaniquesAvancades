using UnityEngine;

public class PlayerGrabber2D : MonoBehaviour
{
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float grabRange = 1.2f;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private LayerMask pieceMask;
    [SerializeField] private PlayerSFX playerSFX;

    private PlayerController2D player;
    private Collider2D playerCollider;
    private PieceController2D heldPiece;

    private void Awake()
    {
        player = GetComponent<PlayerController2D>();
        playerCollider = GetComponent<Collider2D>();

        if (playerSFX == null)
            playerSFX = GetComponent<PlayerSFX>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (heldPiece == null)
                TryGrab();
            else
                ThrowHeldPiece();
        }
    }

    private void LateUpdate()
    {
        if (heldPiece != null && holdPoint != null)
        {
            heldPiece.transform.position = holdPoint.position;
        }
    }

    private void TryGrab()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, grabRange, pieceMask);
        if (hit == null) return;

        PieceController2D piece = hit.GetComponent<PieceController2D>();
        if (piece == null) return;

        heldPiece = piece;
        heldPiece.SetHeld(true);

        Physics2D.IgnoreCollision(playerCollider, heldPiece.PieceCollider, true);
    }

    private void ThrowHeldPiece()
    {
        Vector2 throwDir = GetThrowDirection();
        PieceController2D piece = heldPiece;
        heldPiece = null;

        Physics2D.IgnoreCollision(playerCollider, piece.PieceCollider, false);
        piece.Throw(throwDir * throwForce);

        playerSFX?.PlayThrow();
    }

    private Vector2 GetThrowDirection()
    {
        Vector2 gravity = player.GravityDirection;
        Vector2 tangent = player.TangentDirection;
        int facing = player.FacingSign;

        if (Mathf.Abs(gravity.x) > 0.5f)
        {
            return (-gravity + tangent * 0.25f).normalized;
        }

        return (tangent * facing + (-gravity * 0.35f)).normalized;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, grabRange);
    }
}
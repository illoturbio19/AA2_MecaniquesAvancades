using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PieceController2D : MonoBehaviour, IZoneAffectable
{
    [Header("Identity")]
    [SerializeField] private string pieceId = "MainPiece";

    [Header("Base Physics")]
    [SerializeField] private float baseGravityStrength = 35f;
    [SerializeField] private Vector2 defaultGravityDirection = Vector2.down;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 0.08f;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isHeld;

    private readonly Dictionary<ZoneArea2D, ZoneProfileSO> activeZones = new();
    private ZoneProfileSO currentZone;

    public string PieceId => pieceId;
    public bool IsHeld => isHeld;
    public Collider2D PieceCollider => col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.gravityScale = 0f;
    }

    private void FixedUpdate()
    {
        ResolveCurrentZone();

        if (isHeld) return;

        Vector2 gravityDir = GetGravityDirection();
        float gravityStrength = GetGravityStrength();
        rb.AddForce(gravityDir * gravityStrength * rb.mass);

        ApplySlidingIfNeeded();
        ApplyScale();
        ClampVelocityIfNeeded();
    }

    public void SetHeld(bool held)
    {
        isHeld = held;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = held ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
    }

    public void Throw(Vector2 force)
    {
        SetHeld(false);
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    private void ApplySlidingIfNeeded()
    {
        if (currentZone == null) return;

        if (currentZone.groundDecelerationMultiplier >= 0.99f) return;

        if (!IsGrounded()) return;

        Vector2 gravityDir = GetGravityDirection();
        Vector2 tangent = new Vector2(-gravityDir.y, gravityDir.x);

        float tangentSpeed = Vector2.Dot(rb.linearVelocity, tangent);
        float gravitySpeed = Vector2.Dot(rb.linearVelocity, gravityDir);

        float decel = Mathf.Lerp(0.05f, 8f, currentZone.groundDecelerationMultiplier);
        tangentSpeed = Mathf.MoveTowards(tangentSpeed, tangentSpeed * 0.999f, decel * Time.fixedDeltaTime);

        rb.linearVelocity = tangent * tangentSpeed + gravityDir * gravitySpeed;
    }

    private bool IsGrounded()
    {
        Bounds bounds = col.bounds;
        Vector2 origin = bounds.center;
        Vector2 castSize = bounds.size * 0.92f;
        Vector2 gravityDir = GetGravityDirection();

        RaycastHit2D hit = Physics2D.BoxCast(
            origin,
            castSize,
            0f,
            gravityDir,
            groundCheckDistance,
            groundMask
        );

        return hit.collider != null;
    }

    private Vector2 GetGravityDirection()
    {
        if (currentZone != null && currentZone.gravityDirection != Vector2.zero)
            return currentZone.gravityDirection.normalized;

        return defaultGravityDirection.normalized;
    }

    private float GetGravityStrength()
    {
        if (currentZone == null) return baseGravityStrength;
        return baseGravityStrength * currentZone.pieceGravityMultiplier;
    }

    private void ApplyScale()
    {
        float scale = currentZone != null ? currentZone.scaleMultiplier : 1f;
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void ClampVelocityIfNeeded()
    {
        if (currentZone == null || !currentZone.clampVelocity) return;

        Vector2 v = rb.linearVelocity;
        v.x = Mathf.Clamp(v.x, -currentZone.maxHorizontalSpeed, currentZone.maxHorizontalSpeed);
        v.y = Mathf.Clamp(v.y, -currentZone.maxVerticalSpeed, currentZone.maxVerticalSpeed);
        rb.linearVelocity = v;
    }

    private void ResolveCurrentZone()
    {
        currentZone = null;
        int bestPriority = int.MinValue;

        foreach (var kvp in activeZones)
        {
            if (kvp.Value == null) continue;
            if (kvp.Value.priority > bestPriority)
            {
                bestPriority = kvp.Value.priority;
                currentZone = kvp.Value;
            }
        }
    }

    public void ApplyZone(ZoneProfileSO profile, ZoneArea2D source)
    {
        if (profile == null || source == null) return;
        if (!profile.affectsPiece) return;
        activeZones[source] = profile;
    }

    public void RemoveZone(ZoneArea2D source)
    {
        if (source == null) return;
        if (activeZones.ContainsKey(source))
        {
            activeZones.Remove(source);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentZone == null) return;
        if (currentZone.bounceMultiplier <= 1f) return;
        if (((1 << collision.gameObject.layer) & groundMask) == 0) return;
        if (collision.contactCount == 0) return;

        Vector2 normal = collision.GetContact(0).normal;
        Vector2 reflected = Vector2.Reflect(rb.linearVelocity, normal) * currentZone.bounceMultiplier;

        Vector2 normalComponent = Vector2.Dot(reflected, normal) * normal;
        if (normalComponent.magnitude < currentZone.minimumBounceSpeed)
        {
            reflected += normal * (currentZone.minimumBounceSpeed - normalComponent.magnitude);
        }

        rb.linearVelocity = reflected;
        ClampVelocityIfNeeded();
    }
}
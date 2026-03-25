using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController2D : MonoBehaviour, IZoneAffectable
{
    [Header("Base Movement")]
    [SerializeField] private float baseMoveSpeed = 8f;
    [SerializeField] private float baseGroundAcceleration = 60f;
    [SerializeField] private float baseAirAcceleration = 35f;
    [SerializeField] private float baseGroundDeceleration = 70f;
    [SerializeField] private float baseAirDeceleration = 25f;
    [SerializeField] private float baseJumpForce = 14f;

    [Header("Base Gravity")]
    [SerializeField] private float baseGravityStrength = 40f;
    [SerializeField] private Vector2 defaultGravityDirection = Vector2.down;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 0.12f;

    [Header("Wall Gravity")]
    [SerializeField] private float wallMoveSpeed = 7f;
    [SerializeField] private float wallJumpForce = 13f;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private Collider2D col;
    private Animator animator;

    private float moveInput;
    private bool jumpPressed;
    private bool isGrounded;
    private bool wallAttached;
    private bool facingRight = true;

    private readonly Dictionary<ZoneArea2D, ZoneProfileSO> activeZones = new();
    private ZoneProfileSO currentZone;

    public Vector2 GravityDirection => GetGravityDirection();
    public Vector2 TangentDirection => new Vector2(-GravityDirection.y, GravityDirection.x);
    public bool IsGrounded => isGrounded;
    public bool IsWallAttached => wallAttached;
    public int FacingSign => facingRight ? 1 : -1;
    public float CurrentPushMultiplier => currentZone != null ? currentZone.pushStrengthMultiplier : 1f;

    // Animator parameter hashes
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
    private static readonly int IsWallAttachedHash = Animator.StringToHash("IsWallAttached");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        ReadInput();
        UpdateAnimator();
        UpdateFacing();
    }

    private void FixedUpdate()
    {
        ResolveCurrentZone();
        UpdateGroundedState();
        UpdateWallAttachState();
        ApplyCustomGravity();
        HandleMovement();
        HandleJump();
        ClampVelocityIfNeeded();
        AlignVisualToGravity();
    }

    private void ReadInput()
    {
        // Si la gravetat �s lateral, moure's amb amunt/avall
        if (Mathf.Abs(GravityDirection.x) > 0.5f)
        {
            moveInput = 0f;
            if (Input.GetKey(KeyCode.UpArrow)) moveInput += 1f;
            if (Input.GetKey(KeyCode.DownArrow)) moveInput -= 1f;
        }
        else
        {
            moveInput = 0f;
            if (Input.GetKey(KeyCode.RightArrow)) moveInput += 1f;
            if (Input.GetKey(KeyCode.LeftArrow)) moveInput -= 1f;
        }

        if (Input.GetKeyDown(GetJumpKey()))
        {
            jumpPressed = true;
        }
    }

    private KeyCode GetJumpKey()
    {
        if (Mathf.Abs(GravityDirection.x) > 0.5f)
        {
            // Si la gravetat tira cap a la dreta, saltes "fora" amb l'esquerra
            return GravityDirection.x > 0 ? KeyCode.LeftArrow : KeyCode.RightArrow;
        }

        return KeyCode.UpArrow;
    }

    private void ApplyCustomGravity()
    {
        if (wallAttached)
        {
            Vector2 tangent = TangentDirection;
            float tangentSpeed = Vector2.Dot(rb.linearVelocity, tangent);
            rb.linearVelocity = tangent * tangentSpeed;
            return;
        }

        rb.AddForce(GravityDirection * GetGravityStrength() * rb.mass);
    }

    private void HandleMovement()
    {
        Vector2 tangent = TangentDirection;
        float currentTangentSpeed = Vector2.Dot(rb.linearVelocity, tangent);

        if (wallAttached)
        {
            float targetWallSpeed = moveInput * wallMoveSpeed;
            float newWallSpeed = Mathf.MoveTowards(currentTangentSpeed, targetWallSpeed, 80f * Time.fixedDeltaTime);
            rb.linearVelocity = tangent * newWallSpeed;
            return;
        }

        float gravityAxisSpeed = Vector2.Dot(rb.linearVelocity, GravityDirection);

        float newTangentSpeed;

        if (Mathf.Abs(moveInput) > 0.01f)
        {
            float targetSpeed = moveInput * GetMoveSpeed();
            float accel = isGrounded ? GetGroundAcceleration() : GetAirAcceleration();
            newTangentSpeed = Mathf.MoveTowards(currentTangentSpeed, targetSpeed, accel * Time.fixedDeltaTime);
        }
        else
        {
            float decel = isGrounded ? GetGroundDeceleration() : GetAirDeceleration();
            newTangentSpeed = Mathf.MoveTowards(currentTangentSpeed, 0f, decel * Time.fixedDeltaTime);
        }

        rb.linearVelocity = tangent * newTangentSpeed + GravityDirection * gravityAxisSpeed;
    }

    private void HandleJump()
    {
        if (!jumpPressed) return;
        jumpPressed = false;

        if (!isGrounded && !wallAttached) return;

        Vector2 tangent = TangentDirection;

        if (wallAttached)
        {
            rb.linearVelocity = (-GravityDirection * wallJumpForce) + tangent * moveInput * 2f;
            wallAttached = false;
            return;
        }

        float tangentSpeed = Vector2.Dot(rb.linearVelocity, tangent);
        rb.linearVelocity = tangent * tangentSpeed + (-GravityDirection * GetJumpForce());
    }

    private void UpdateGroundedState()
    {
        Bounds bounds = col.bounds;
        Vector2 origin = bounds.center;
        Vector2 castSize = bounds.size * 0.92f;

        RaycastHit2D hit = Physics2D.BoxCast(
            origin,
            castSize,
            0f,
            GravityDirection,
            groundCheckDistance,
            groundMask
        );

        isGrounded = hit.collider != null;
    }

    private void UpdateWallAttachState()
    {
        wallAttached = false;

        if (currentZone == null) return;
        if (!currentZone.enableWallAttach) return;
        if (Mathf.Abs(GravityDirection.x) < 0.5f) return;

        Bounds bounds = col.bounds;
        Vector2 origin = bounds.center;
        Vector2 castSize = bounds.size * 0.95f;

        RaycastHit2D hit = Physics2D.BoxCast(
            origin,
            castSize,
            0f,
            GravityDirection,
            groundCheckDistance + 0.05f,
            groundMask
        );

        if (hit.collider != null)
        {
            wallAttached = true;
        }
    }

    private void ClampVelocityIfNeeded()
    {
        if (currentZone == null || !currentZone.clampVelocity) return;

        Vector2 tangent = TangentDirection;
        float tangentSpeed = Vector2.Dot(rb.linearVelocity, tangent);
        float gravitySpeed = Vector2.Dot(rb.linearVelocity, GravityDirection);

        tangentSpeed = Mathf.Clamp(tangentSpeed, -currentZone.maxHorizontalSpeed, currentZone.maxHorizontalSpeed);
        gravitySpeed = Mathf.Clamp(gravitySpeed, -currentZone.maxVerticalSpeed, currentZone.maxVerticalSpeed);

        rb.linearVelocity = tangent * tangentSpeed + GravityDirection * gravitySpeed;
    }

    private void AlignVisualToGravity()
    {
        float angle = Vector2.SignedAngle(Vector2.up, -GravityDirection);
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        float scale = currentZone != null ? currentZone.scaleMultiplier : 1f;
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void UpdateFacing()
    {
        if (Mathf.Abs(moveInput) > 0.01f)
        {
            // Quan la gravetat �s normal, esquerra/dreta
            if (Mathf.Abs(GravityDirection.y) > 0.5f)
            {
                facingRight = moveInput > 0f;
            }
            // Quan la gravetat �s lateral, fem servir el moviment amunt/avall
            else
            {
                // Aix� �s opcional; mant� la cara segons el sentit del moviment vertical
                if (moveInput > 0.01f) facingRight = true;
                else if (moveInput < -0.01f) facingRight = false;
            }
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !facingRight;
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        Vector2 tangent = TangentDirection;
        float tangentSpeed = Mathf.Abs(Vector2.Dot(rb.linearVelocity, tangent));
        float verticalAlongGravity = Vector2.Dot(rb.linearVelocity, -GravityDirection);

        animator.SetFloat(SpeedHash, tangentSpeed);
        animator.SetBool(IsGroundedHash, isGrounded);
        animator.SetFloat(VerticalVelocityHash, verticalAlongGravity);
        animator.SetBool(IsWallAttachedHash, wallAttached);
    }

    private Vector2 GetGravityDirection()
    {
        if (currentZone != null && currentZone.gravityDirection != Vector2.zero)
            return currentZone.gravityDirection.normalized;

        return defaultGravityDirection.normalized;
    }

    private float GetGravityStrength()
    {
        float multiplier = currentZone != null ? currentZone.gravityMultiplier : 1f;
        return baseGravityStrength * multiplier;
    }

    private float GetMoveSpeed()
    {
        float multiplier = currentZone != null ? currentZone.moveSpeedMultiplier : 1f;
        return baseMoveSpeed * multiplier;
    }

    private float GetGroundAcceleration()
    {
        float multiplier = currentZone != null ? currentZone.groundAccelerationMultiplier : 1f;
        return baseGroundAcceleration * multiplier;
    }

    private float GetAirAcceleration()
    {
        float multiplier = currentZone != null ? currentZone.airAccelerationMultiplier : 1f;
        return baseAirAcceleration * multiplier;
    }

    private float GetGroundDeceleration()
    {
        float multiplier = currentZone != null ? currentZone.groundDecelerationMultiplier : 1f;
        return baseGroundDeceleration * multiplier;
    }

    private float GetAirDeceleration()
    {
        float multiplier = currentZone != null ? currentZone.airDecelerationMultiplier : 1f;
        return baseAirDeceleration * multiplier;
    }

    private float GetJumpForce()
    {
        float multiplier = currentZone != null ? currentZone.jumpMultiplier : 1f;
        return baseJumpForce * multiplier;
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
        if (!profile.affectsPlayer) return;
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
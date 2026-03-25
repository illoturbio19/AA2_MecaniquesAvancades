using UnityEngine;

[CreateAssetMenu(menuName = "CambraViva/Zone Profile", fileName = "ZoneProfile_")]
public class ZoneProfileSO : ScriptableObject
{
    [Header("General")]
    public string zoneId = "Zone";
    public int priority = 0;

    [Header("Affects")]
    public bool affectsPlayer = true;
    public bool affectsPiece = true;

    [Header("Gravity")]
    public Vector2 gravityDirection = Vector2.down;
    public float gravityMultiplier = 1f;

    [Header("Movement")]
    public float moveSpeedMultiplier = 1f;
    public float jumpMultiplier = 1f;
    public float groundAccelerationMultiplier = 1f;
    public float airAccelerationMultiplier = 1f;

    [Header("Deceleration / Sliding")]
    public float groundDecelerationMultiplier = 1f;
    public float airDecelerationMultiplier = 1f;

    [Header("Velocity Clamp")]
    public bool clampVelocity = false;
    public float maxHorizontalSpeed = 8f;
    public float maxVerticalSpeed = 16f;

    [Header("Bounce")]
    public float bounceMultiplier = 1f;
    public float minimumBounceSpeed = 8f;

    [Header("Special")]
    public float scaleMultiplier = 1f;
    public float pushStrengthMultiplier = 1f;
    public bool enableWallAttach = false;

    [Header("Piece")]
    public float pieceGravityMultiplier = 1f;
}
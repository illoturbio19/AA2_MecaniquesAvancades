using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ZoneVisualController : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color baseColor = new Color(0.75f, 0.65f, 0.55f, 0.85f);
    [SerializeField] private Color accentColor = new Color(0.25f, 0.55f, 0.85f, 0.85f);
    [SerializeField] private Color groutColor = new Color(0.95f, 0.92f, 0.85f, 1f);

    [Header("Pattern")]
    [SerializeField, Range(3f, 40f)] private float tileScale = 12f;
    [SerializeField, Range(0.001f, 0.15f)] private float lineWidth = 0.035f;
    [SerializeField, Range(0f, 1f)] private float colorVariation = 0.22f;

    [Header("Animation")]
    [SerializeField, Range(0f, 3f)] private float driftSpeed = 0.4f;
    [SerializeField, Range(0f, 0.2f)] private float driftAmount = 0.03f;
    [SerializeField, Range(0f, 2f)] private float pulse = 0.35f;
    [SerializeField, Range(0f, 2f)] private float sparkle = 0.25f;

    private SpriteRenderer sr;
    private MaterialPropertyBlock mpb;

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int AccentColorID = Shader.PropertyToID("_AccentColor");
    private static readonly int GroutColorID = Shader.PropertyToID("_GroutColor");
    private static readonly int TileScaleID = Shader.PropertyToID("_TileScale");
    private static readonly int LineWidthID = Shader.PropertyToID("_LineWidth");
    private static readonly int ColorVariationID = Shader.PropertyToID("_ColorVariation");
    private static readonly int DriftSpeedID = Shader.PropertyToID("_DriftSpeed");
    private static readonly int DriftAmountID = Shader.PropertyToID("_DriftAmount");
    private static readonly int PulseID = Shader.PropertyToID("_Pulse");
    private static readonly int SparkleID = Shader.PropertyToID("_Sparkle");

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();
        ApplyVisuals();
    }

    private void OnValidate()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (mpb == null) mpb = new MaterialPropertyBlock();
            ApplyVisuals();
        }
    }

    public void ApplyVisuals()
    {
        sr.GetPropertyBlock(mpb);

        mpb.SetColor(BaseColorID, baseColor);
        mpb.SetColor(AccentColorID, accentColor);
        mpb.SetColor(GroutColorID, groutColor);
        mpb.SetFloat(TileScaleID, tileScale);
        mpb.SetFloat(LineWidthID, lineWidth);
        mpb.SetFloat(ColorVariationID, colorVariation);
        mpb.SetFloat(DriftSpeedID, driftSpeed);
        mpb.SetFloat(DriftAmountID, driftAmount);
        mpb.SetFloat(PulseID, pulse);
        mpb.SetFloat(SparkleID, sparkle);

        sr.SetPropertyBlock(mpb);
    }
}
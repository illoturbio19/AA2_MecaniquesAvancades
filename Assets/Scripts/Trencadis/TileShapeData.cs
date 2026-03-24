using UnityEngine;
public enum TileEmotionType
{
    Positive,
    Neutral,
    Negative
}

[CreateAssetMenu(fileName = "TileShapeData", menuName = "Trencadis/Tile Shape Data")]
public class TileShapeData : ScriptableObject
{
    [Header("Identity")]
    public string tileId;
    public string displayName;

    [Header("Visual")]
    public Sprite tileSprite;
    public Color tileColor = Color.white;

    [Header("Logical Shape")]
    [Tooltip("Mida lògica en cel·les. Per ara la deixem simple.")]
    public Vector2Int logicalSize = Vector2Int.one;

    [Header("Placement")]
    [Tooltip("Si true, la peça es pot rotar.")]
    public bool canRotate = true;

    [Tooltip("Rotació inicial suggerida.")]
    public float defaultRotation = 0f;

    [Header("Emotion")]
    public TileEmotionType emotionType = TileEmotionType.Neutral;

    [Range(0f, 1f)]
    public float difficultyWeight = 0.5f;

    [Header("Gameplay")]
    [Tooltip("Si la peça és clau per completar una zona important.")]
    public bool isCorePiece = false;
}

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AudioSource))]
public class GoalZone2D : MonoBehaviour
{
    [Header("Goal Settings")]
    [SerializeField] private string requiredPieceId = "MainPiece";
    [SerializeField] private bool destroyPieceOnSuccess = false;

    [Header("Scene Transition")]
    [SerializeField] private bool loadNextScene = true;
    [SerializeField] private string nextSceneName = "";
    [SerializeField] private float completeDelay = 1.2f;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer targetSpriteRenderer;
    [SerializeField] private Sprite completedSprite;

    [Header("Audio")]
    [SerializeField] private AudioClip connectionClip;
    [SerializeField] private float connectionVolume = 1f;
    [SerializeField] private float randomPitchMin = 0.98f;
    [SerializeField] private float randomPitchMax = 1.02f;

    private bool completed;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (completed) return;

        PieceController2D piece = other.GetComponent<PieceController2D>();
        if (piece == null) return;
        if (piece.PieceId != requiredPieceId) return;

        completed = true;

        if (targetSpriteRenderer != null && completedSprite != null)
        {
            targetSpriteRenderer.sprite = completedSprite;
        }

        PlayConnectionSFX();

        Debug.Log("LEVEL COMPLETE");

        if (destroyPieceOnSuccess)
        {
            Destroy(piece.gameObject);
        }

        StartCoroutine(CompleteRoutine());
    }

    private IEnumerator CompleteRoutine()
    {
        yield return new WaitForSeconds(completeDelay);

        if (loadNextScene)
        {
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                int currentIndex = SceneManager.GetActiveScene().buildIndex;
                int nextIndex = currentIndex + 1;

                if (nextIndex < SceneManager.sceneCountInBuildSettings)
                {
                    SceneManager.LoadScene(nextIndex);
                }
                else
                {
                    Debug.Log("No hi ha següent escena al Build Settings.");
                }
            }
        }
    }

    private void PlayConnectionSFX()
    {
        if (connectionClip == null || audioSource == null) return;

        audioSource.pitch = Random.Range(randomPitchMin, randomPitchMax);
        audioSource.PlayOneShot(connectionClip, connectionVolume);
        audioSource.pitch = 1f;
    }
}
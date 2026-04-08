using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HintTrigger2D : MonoBehaviour
{
    [Header("Message")]
    [TextArea(2, 5)]
    [SerializeField] private string message;

    [SerializeField] private string speaker = "";
    [SerializeField] private bool showOnlyOnce = true;
    [SerializeField] private bool hideOnExit = false;
    [SerializeField] private float autoHideDelay = 3.5f;

    private bool hasShown;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController2D player = other.GetComponentInParent<PlayerController2D>();
        if (player == null) return;

        if (showOnlyOnce && hasShown) return;

        hasShown = true;

        if (HintUIManager.Instance != null)
        {
            HintUIManager.Instance.ShowMessage(message, speaker, autoHideDelay);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController2D player = other.GetComponentInParent<PlayerController2D>();
        if (player == null) return;

        if (!hideOnExit) return;

        if (HintUIManager.Instance != null)
        {
            HintUIManager.Instance.HideMessage();
        }
    }
}
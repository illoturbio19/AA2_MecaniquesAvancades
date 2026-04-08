using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HintUIManager : MonoBehaviour
{
    public static HintUIManager Instance;

    [System.Serializable]
    private class QueuedHint
    {
        public string message;
        public string speaker;
        public float autoHideDelay;

        public QueuedHint(string message, string speaker, float autoHideDelay)
        {
            this.message = message;
            this.speaker = speaker;
            this.autoHideDelay = autoHideDelay;
        }
    }

    [Header("References")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text messageText;

    [Header("Typewriter")]
    [SerializeField] private float charactersPerSecond = 35f;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool allowSkipTyping = true;
    [SerializeField] private KeyCode skipKey = KeyCode.E;

    private readonly Queue<QueuedHint> messageQueue = new();

    private Coroutine hideRoutine;
    private Coroutine typeRoutine;

    private bool isTyping;
    private bool isShowingMessage;
    private string currentFullMessage = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (root != null)
            root.SetActive(false);
    }

    private void Update()
    {
        if (!allowSkipTyping) return;
        if (!isShowingMessage) return;

        if (Input.GetKeyDown(skipKey) || Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                SkipTyping();
            }
            else
            {
                ShowNextMessageFromQueueOrHide();
            }
        }
    }

    public void ShowMessage(string message, string speaker = "", float autoHideDelay = 0f)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        QueuedHint newHint = new QueuedHint(message, speaker, autoHideDelay);

        // Si ya hay un mensaje activo o escribiéndose, lo metemos en cola.
        if (isShowingMessage || isTyping)
        {
            messageQueue.Enqueue(newHint);
            return;
        }

        StartShowingMessage(newHint);
    }

    public void HideMessage()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
            typeRoutine = null;
        }

        isTyping = false;
        isShowingMessage = false;
        currentFullMessage = "";

        if (root != null)
            root.SetActive(false);
    }

    private void StartShowingMessage(QueuedHint hint)
    {
        if (root == null || messageText == null) return;

        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
            typeRoutine = null;
        }

        isShowingMessage = true;
        currentFullMessage = hint.message;

        root.SetActive(true);

        if (speakerText != null)
        {
            bool hasSpeaker = !string.IsNullOrWhiteSpace(hint.speaker);
            speakerText.gameObject.SetActive(hasSpeaker);

            if (hasSpeaker)
                speakerText.text = hint.speaker;
        }

        typeRoutine = StartCoroutine(TypeTextRoutine(hint.message, hint.autoHideDelay));
    }

    private IEnumerator TypeTextRoutine(string fullMessage, float autoHideDelay)
    {
        isTyping = true;

        if (string.IsNullOrEmpty(fullMessage))
        {
            messageText.text = "";
            messageText.maxVisibleCharacters = 0;
            isTyping = false;

            if (autoHideDelay > 0f)
                hideRoutine = StartCoroutine(HideAfterDelay(autoHideDelay));

            yield break;
        }

        messageText.text = fullMessage;
        messageText.ForceMeshUpdate();

        int totalCharacters = messageText.textInfo.characterCount;
        messageText.maxVisibleCharacters = 0;

        if (totalCharacters <= 0)
            totalCharacters = fullMessage.Length;

        float delayPerCharacter = 1f / Mathf.Max(1f, charactersPerSecond);

        for (int i = 0; i <= totalCharacters; i++)
        {
            messageText.maxVisibleCharacters = i;

            float extraDelay = 0f;

            if (i > 0 && i <= fullMessage.Length)
            {
                char c = fullMessage[i - 1];

                if (c == ',' || c == ';' || c == ':')
                    extraDelay = 0.04f;
                else if (c == '.' || c == '!' || c == '?')
                    extraDelay = 0.08f;
            }

            if (useUnscaledTime)
                yield return new WaitForSecondsRealtime(delayPerCharacter + extraDelay);
            else
                yield return new WaitForSeconds(delayPerCharacter + extraDelay);
        }

        isTyping = false;
        typeRoutine = null;

        if (autoHideDelay > 0f)
            hideRoutine = StartCoroutine(HideAfterDelay(autoHideDelay));
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(delay);
        else
            yield return new WaitForSeconds(delay);

        ShowNextMessageFromQueueOrHide();
    }

    public void SkipTyping()
    {
        if (!isTyping) return;

        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
            typeRoutine = null;
        }

        messageText.ForceMeshUpdate();

        int totalCharacters = messageText.textInfo.characterCount;
        messageText.maxVisibleCharacters = totalCharacters > 0 ? totalCharacters : int.MaxValue;

        isTyping = false;
    }

    private void ShowNextMessageFromQueueOrHide()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        if (messageQueue.Count > 0)
        {
            QueuedHint nextHint = messageQueue.Dequeue();
            StartShowingMessage(nextHint);
        }
        else
        {
            HideMessage();
        }
    }

    public void ClearQueue()
    {
        messageQueue.Clear();
    }
}
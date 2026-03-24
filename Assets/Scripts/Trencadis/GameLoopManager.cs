using UnityEngine;

public enum GameState
{
    Setup,
    Playing,
    Won,
    Lost
}

public class GameLoopManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PuzzleStateManager puzzleStateManager;

    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.Setup;

    [Header("Moves")]
    [SerializeField] private int startingMoves = 20;
    [SerializeField] private int currentMoves;

    [Header("Debug")]
    [SerializeField] private bool autoStartOnAwake = true;

    public GameState CurrentState => currentState;
    public int CurrentMoves => currentMoves;
    public bool IsGamePlayable => currentState == GameState.Playing;

    private void Awake()
    {
        if (autoStartOnAwake)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        currentMoves = startingMoves;
        currentState = GameState.Playing;
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
    }

    public void SpendMove(int amount = 1)
    {
        if (currentState != GameState.Playing)
            return;

        currentMoves -= amount;

        if (currentMoves < 0)
            currentMoves = 0;

        EvaluateGameState();
    }

    public void AddMoves(int amount)
    {
        if (currentState != GameState.Playing)
            return;

        currentMoves += amount;
        EvaluateGameState();
    }

    public void RemoveMoves(int amount)
    {
        if (currentState != GameState.Playing)
            return;

        currentMoves -= amount;

        if (currentMoves < 0)
            currentMoves = 0;

        EvaluateGameState();
    }

    public void EvaluateGameState()
    {
        if (puzzleStateManager != null && puzzleStateManager.IsPuzzleComplete())
        {
            currentState = GameState.Won;
            return;
        }

        if (currentMoves <= 0)
        {
            currentState = GameState.Lost;
        }
    }

    public void OnTilePlacementResolved()
    {
        EvaluateGameState();
    }
}
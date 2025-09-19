using UnityEngine;
using TMPro;

public class MoveCounter : MonoBehaviour
{
    public static MoveCounter Instance;

    [Header("Move Counter")]
    public int moveCount;
    public TextMeshProUGUI moveText;

    [Header("Timer")]
    public TextMeshProUGUI timerText;
    public GameObject levelCompleteCanvas; // Canvas that ends the timer

    private float timer;
    private bool timerRunning = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("MoveCounter: Singleton instance created");
        }
        else
        {
            Debug.LogWarning($"MoveCounter: Duplicate instance found on {gameObject.name}, destroying");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        timer = 0f;
        timerRunning = true;
    }

    private void Update()
    {
        // Timer logic
        if (timerRunning)
        {
            timer += Time.deltaTime;

            if (timerText != null)
            {
                var minutes = Mathf.FloorToInt(timer / 60f);
                var seconds = Mathf.FloorToInt(timer % 60f);
                var milliseconds = Mathf.FloorToInt((timer * 100f) % 100f); // Get hundredths

                timerText.text = $"Time: {minutes:00}:{seconds:00}.{milliseconds:00}";
            }

            // Stop timer if levelCompleteCanvas is active
            if (levelCompleteCanvas != null && levelCompleteCanvas.activeSelf)
            {
                timerRunning = false;
            }
        }
    }

    public void IncrementMove()
    {
        moveCount++;
        Debug.Log($"MoveCounter: Move count incremented to {moveCount}");
        
        if (moveText != null)
            moveText.text = "Moves: " + moveCount;
        else
            Debug.LogWarning("MoveCounter: Move text UI component is not assigned!");
    }

    public void ResetCounter()
    {
        moveCount = 0;
        timer = 0f;
        timerRunning = true;
        
        Debug.Log("MoveCounter: Counter and timer reset");
        
        if (moveText != null)
            moveText.text = "Moves: 0";
        else
            Debug.LogWarning("MoveCounter: Move text UI component is not assigned!");

        if (timerText != null)
            timerText.text = "Time: 00:00.00";
        else
            Debug.LogWarning("MoveCounter: Timer text UI component is not assigned!");
    }
    public float GetElapsedTime()
    {
        return timer;
    }
}
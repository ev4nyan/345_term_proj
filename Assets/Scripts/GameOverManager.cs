using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Handles game over conditions (win at wave 10, lose at 0 resolve).
/// Pauses the game and shows a panel with restart/main menu buttons.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu";
    public int winWave = 10;

    private bool gameEnded = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Hide panel at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Setup button listeners
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    /// <summary>
    /// Call this when player wins (reached wave 10)
    /// </summary>
    public void TriggerWin()
    {
        if (gameEnded) return;
        gameEnded = true;

        ShowGameOverPanel("Victory!", "You survived all waves!");
    }

    /// <summary>
    /// Call this when player loses (0 resolve/health)
    /// </summary>
    public void TriggerLose()
    {
        if (gameEnded) return;
        gameEnded = true;

        ShowGameOverPanel("Defeat", "Your kingdom has fallen...");
    }

    void ShowGameOverPanel(string title, string message)
    {
        // Pause the game
        Time.timeScale = 0f;

        // Show panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (titleText != null)
                titleText.text = title;

            if (messageText != null)
                messageText.text = message;
        }
    }

    public void RestartGame()
    {
        // Unpause
        Time.timeScale = 1f;
        gameEnded = false;

        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        // Unpause
        Time.timeScale = 1f;
        gameEnded = false;

        // Load main menu
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public bool IsGameEnded()
    {
        return gameEnded;
    }
}

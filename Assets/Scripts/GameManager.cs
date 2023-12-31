using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverUI;
    public enum GameState
    {
        Playing,
        Paused,
        GameOver
    }

    public GameState gameState = GameState.Playing;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
            return;
        }
        Destroy(gameObject);
    }

    public void StartGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        gameState = GameState.Playing;
        gameOverUI.SetActive(false); 
    }

    public void PauseGame()
    {
        gameState = GameState.Paused;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        gameState = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void EndGame()
    {
        gameState = GameState.GameOver;
        gameOverUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResetScene()
    {
        // var scene = SceneManager.GetActiveScene();
        // StartGame();
        gameOverUI.SetActive(false);
        SceneManager.LoadScene("Forest");
        Time.timeScale = 1f;
    }

}

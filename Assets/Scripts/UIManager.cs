using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject gameOverMenu;
    [SerializeField] PostProcessVolume postProcessVol;
    [SerializeField] PostProcessProfile normalProfile;
    [SerializeField] PostProcessProfile pausedProfile;
    [SerializeField] TMP_Text scoreTextGameOver;

    

    void Awake()
    {
        GameEvents.Instance.OnPlayerDied += EndGame;
        GameEvents.Instance.OnPauseButtonPressed += PauseGame;
        GameEvents.Instance.OnUnpauseButtonPressed += ResumeGame;
        GameEvents.Instance.OnScoreUpdated += SetGameOverScoreText;
    }

    void Start()
    {
        Time.timeScale = 1;
        postProcessVol.profile = normalProfile;
    }

    void FixedUpdate()
    {
        
    }

    void EndGame()
    {
        Time.timeScale = 0;
        gameOverMenu.SetActive(true);
        postProcessVol.profile = pausedProfile;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void PauseGame()
    {
        Time.timeScale = 0;
        pauseMenu.SetActive(true);
        postProcessVol.profile = pausedProfile;
    }

    public void ResumeButtonPressed()
    {
        GameEvents.Instance.UnpauseButtonPressed();
    }

    void ResumeGame()
    {
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
        postProcessVol.profile = normalProfile;
    }

    public void MainMenuButtonPressed()
    {
        SceneManager.LoadScene(0);
    }

    void SetGameOverScoreText(int currentScore)
    {
        if (currentScore > 9999)
        {
            scoreTextGameOver.text = currentScore.ToString("X4");
        }
        else
        {
            scoreTextGameOver.text = currentScore.ToString("0000");
        }
    }

    void OnDisable()
    {
        if (GameEvents.Instance) // Prevents null reference error when closing game or changing scenes
        {
            GameEvents.Instance.OnPlayerDied -= EndGame;
            GameEvents.Instance.OnPauseButtonPressed -= PauseGame;
            GameEvents.Instance.OnUnpauseButtonPressed -= ResumeGame;
            GameEvents.Instance.OnScoreUpdated -= SetGameOverScoreText;
        }
    }
}

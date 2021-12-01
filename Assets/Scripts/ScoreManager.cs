using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ScoreManager : MonoBehaviour
{
    AudioSource audioSource;

    [SerializeField] List<AudioClip> comboSFX;
    [SerializeField] AudioClip gameOverSFX;
    [SerializeField] AudioClip critterSFX;

    [SerializeField] TMP_Text scoreText;

    [SerializeField, Range(20, 40)] int timeStepsForCombo = 20;
    [SerializeField] Slider comboTimeSlider;
    [SerializeField] TMP_Text comboText;

    [SerializeField] FoodController foodController;
    [SerializeField] Transform critterIconPosition;
    [SerializeField] TMP_Text critterTimerText;

    Transform critter;
    bool shouldDisplayCritterHUD = false;
    int currentScore = 0;
    int comboLevel = 0;
    int maxComboTime;
    int comboTime;

    void OnEnable()
    {
        GameEvents.Instance.OnScoreIncreased += IncrementScore;
        GameEvents.Instance.OnCritterSpawned += SetupCritterHUD;
        GameEvents.Instance.OnFoodTriggerEntered += PlayComboSFX;
        GameEvents.Instance.OnCritterTriggerEntered += PlayCritterSFX;
        GameEvents.Instance.OnCritterTriggerEntered += DisableCritterHUDBool;
        GameEvents.Instance.OnCritterDestroyed += DisableCritterHUDBool;
        GameEvents.Instance.OnPlayerDied += BroadcastFinalScore;
        GameEvents.Instance.OnPlayerDied += PlayGameOverSFX;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        HandleComboState();
        HandleCritterHUD();
    }

    void HandleCritterHUD()
    {
        critterTimerText.text = foodController.critterLifeRemaining.ToString("00");
        if (shouldDisplayCritterHUD)
        {
            critterIconPosition.gameObject.SetActive(true);
            critterTimerText.gameObject.SetActive(true);
        } 
        else
        {
            critterIconPosition.gameObject.SetActive(false);
            critterTimerText.gameObject.SetActive(false);
        }
    }

    void HandleComboState()
    {
        maxComboTime = timeStepsForCombo;
        comboTime = Mathf.Clamp(comboTime - 1, 0, maxComboTime);

        if (comboTime <= 0 && comboLevel > 0)
        {
            comboLevel -= 1;
            ResetComboTimer();
        }

        UpdateComboSlider();
        UpdateComboText();
    }

    void ResetComboTimer() => comboTime = maxComboTime;

    void UpdateComboSlider()
    {
        if (comboLevel >= 1)
        {
            comboTimeSlider.value = comboTime;
        }
        if (comboLevel < 1)
        {
            comboTimeSlider.value = 0;
        }
    }

    void UpdateComboText()
    {
        comboText.enabled = true;
        if (comboLevel <= 0)
        {
            comboText.enabled = false;
        }
        if (comboLevel >= 1)
        {
            comboText.text = (comboLevel + 1) + " X";
        }
    }

    void IncrementScore(int pointValue)
    {
        currentScore += pointValue * (1 + comboLevel);
        comboLevel = Mathf.Clamp(comboLevel + 1, 0, comboSFX.Count - 1);

        UpdateScoreText(currentScore);
        UpdateComboText();
        ResetComboTimer();
    }

    void UpdateScoreText(int currentScore)
    {
        scoreText.text = (currentScore > 9999) ? 
            currentScore.ToString("X4") : currentScore.ToString("0000");
    }

    void SetupCritterHUD(Transform _critter)
    {
        shouldDisplayCritterHUD = true;
        Transform critterIcon = Instantiate(_critter);
        critterIcon.parent = critterIconPosition;
        critterIcon.localPosition = new Vector3(-28, 0, 0);
    }

    void DisableCritterHUDBool()
    {
        shouldDisplayCritterHUD = false;
    }

    void BroadcastFinalScore() => GameEvents.Instance.ScoreUpdated(currentScore);

    void PlayComboSFX() => audioSource.PlayOneShot(comboSFX[comboLevel]);
    void PlayGameOverSFX() => audioSource.PlayOneShot(gameOverSFX);
    void PlayCritterSFX() => audioSource.PlayOneShot(critterSFX);

    void OnDisable()
    {
        if (GameEvents.Instance) // Prevents null reference error when closing game or changing scenes
        {
            GameEvents.Instance.OnScoreIncreased -= IncrementScore;
            GameEvents.Instance.OnCritterSpawned -= SetupCritterHUD;
            GameEvents.Instance.OnFoodTriggerEntered -= PlayComboSFX;
            GameEvents.Instance.OnCritterTriggerEntered -= PlayCritterSFX;
            GameEvents.Instance.OnCritterTriggerEntered -= DisableCritterHUDBool;
            GameEvents.Instance.OnCritterDestroyed -= DisableCritterHUDBool;
            GameEvents.Instance.OnPlayerDied -= BroadcastFinalScore;
            GameEvents.Instance.OnPlayerDied -= PlayGameOverSFX;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    static GameEvents instance;
    public static GameEvents Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<GameEvents>();
            return instance;
        }
    }

    public event Action<List<SnakeSegment>> OnSegmentsListUpdated;
    public event Action<List<Vector3>> OnDigestPositionsListUpdated;
    public event Action OnFoodTriggerEntered;
    public event Action OnCritterTriggerEntered;
    public event Action OnCritterDestroyed;
    public event Action<Transform> OnCritterSpawned;
    public event Action<int> OnScoreIncreased;
    public event Action<int> OnScoreUpdated;
    public event Action OnPauseButtonPressed;
    public event Action OnUnpauseButtonPressed;
    public event Action OnPlayerDied;

    public void SegmentsListUpdated(List<SnakeSegment> segments)
    {
        OnSegmentsListUpdated?.Invoke(segments);
    }

    public void DigestPositionsListUpdated(List<Vector3> digestPositions)
    {
        OnDigestPositionsListUpdated?.Invoke(digestPositions);
    }

    public void FoodTriggerEntered()
    {
        OnFoodTriggerEntered?.Invoke();
    }

    public void CritterTriggerEntered()
    {
        OnCritterTriggerEntered?.Invoke();
    }
    public void CritterDestroyed()
    {
        OnCritterDestroyed?.Invoke();
    }

    public void CritterSpawned(Transform critter)
    {
        OnCritterSpawned?.Invoke(critter);
    }

    public void ScoreIncreased(int pointValue)
    {
        OnScoreIncreased?.Invoke(pointValue);
    }

    public void ScoreUpdated(int currentScore)
    {
        OnScoreUpdated?.Invoke(currentScore);
    }

    public void PauseButtonPressed()
    {
        OnPauseButtonPressed?.Invoke();
    }

    public void UnpauseButtonPressed()
    {
        OnUnpauseButtonPressed?.Invoke();
    }

    public void PlayerDied()
    {
        OnPlayerDied?.Invoke();
    }
}
